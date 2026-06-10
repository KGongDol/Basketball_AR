/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

using UnityEngine;
using UnityEngine.InputSystem;

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.Movement
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class RotationByMouseControl : MonoBehaviour
    { 
        [Tooltip("Smoothing time for gravity-like effect")]
        [SerializeField]
        [Range(0f, 1f)]
        private float smoothTime = 0.1f;

        // Threshold for detecting near-zero rotation
        private const float dampingThreshold = 0.01f;

        private Mouse mouse;

        [Header("Button")]
        [SerializeField]
        private bool isRotationWithButton = true;

        [Tooltip("(e.g., 'leftButton', 'rightButton', 'middleButton')")]
        [SerializeField]
        private string buttonName = "rightButton";

        private InputControl mouseButton;

        private float horizontalVelocity = 0f;
        private float verticalVelocity = 0f;

        [Header("Horizontal")]
        [Tooltip("Object for Horizontal Rotation")]
        [SerializeField]
        private bool isHorizontalParent = false;
        
        [SerializeField]
        private Transform horizontal;
        
        [SerializeField]
        private float horizontalDeltaWebGLFactor = 0.1f;
        
        [SerializeField]
        private float speedHorizontal = 8f;

        private float horizontalDelta;
        private float targetHorizontalDelta;

        [Header("Vertical")]
        [Tooltip("Object for Vertical Rotation")]
        [SerializeField]
        private bool isVerticalParent = false;
        
        [SerializeField]
        private Transform vertical;
        
        [SerializeField]
        private float verticalDeltaWebGLFactor = 0.1f;
        
        [SerializeField]
        private float speedVertical = -8f;

        private float verticalDelta;
        private float targetVerticalDelta;

        private float deltaTimeNormalized;

        private void Start()
        {   
            mouse = Mouse.current;

            if (mouse != null)
            {
                mouseButton = mouse.TryGetChildControl(buttonName);

                if (mouseButton == null)
                {
                    DebugPrinter.Print($"Control '{buttonName}' not found on Mouse.");
                }
            }
            else
            {
                DebugPrinter.Print($"Mouse not found.");
            }
        }

        private void LateUpdate()
        {
            if (mouse != null)
            {
                if (isRotationWithButton)
                {
                    if (mouseButton != null && mouseButton.IsPressed())
                    {
                        Rotate(true);
                    }
                    else
                    {
                        Rotate(false);
                    }
                }
                else
                {
                    Rotate(true);
                }
            }
        }

        private void Rotate(bool isActive)
        {
            targetHorizontalDelta = mouse.delta.x.ReadValue();
            targetVerticalDelta = mouse.delta.y.ReadValue();

            bool isNotDamping =
                Mathf.Abs(horizontalDelta) < dampingThreshold &&
                Mathf.Abs(verticalDelta) < dampingThreshold;

            bool isMouseNotMoving =
                Mathf.Abs(targetHorizontalDelta) < dampingThreshold &&
                Mathf.Abs(targetVerticalDelta) < dampingThreshold;

            if (!isActive && isNotDamping || isNotDamping & isMouseNotMoving)
            {  
                return;
            }

            // DebugPrinter.Print($"-------");
            // DebugPrinter.Print($"H :{horizontalDelta}, V :{verticalDelta}");
            // DebugPrinter.Print($"Ht:{targetHorizontalDelta}, Vt:{targetVerticalDelta}");

            #if UNITY_WEBGL && !UNITY_EDITOR

            ApplyWebGLCorrectionForDelta();

            #endif

            deltaTimeNormalized = Time.deltaTime / Time.timeScale;

            horizontalDelta = Mathf.SmoothDamp(horizontalDelta,
                isActive ? targetHorizontalDelta : 0f, ref horizontalVelocity, 
                smoothTime, Mathf.Infinity, deltaTimeNormalized);

            verticalDelta = Mathf.SmoothDamp(verticalDelta, 
                isActive ? targetVerticalDelta : 0f, ref verticalVelocity, 
                smoothTime, Mathf.Infinity, deltaTimeNormalized);

            if (horizontal)
            {
                (isHorizontalParent ? horizontal.parent : horizontal).Rotate(
                    0f,
                    horizontalDelta * speedHorizontal,
                    0f);
            }

            if (vertical)
            {
                (isVerticalParent ? vertical.parent : vertical).Rotate(
                    verticalDelta * speedVertical,
                    0f,
                    0f);
            }
        }

        private void ApplyWebGLCorrectionForDelta()
        {
            targetHorizontalDelta *= horizontalDeltaWebGLFactor;
            targetVerticalDelta *= verticalDeltaWebGLFactor;
        }

    }
}