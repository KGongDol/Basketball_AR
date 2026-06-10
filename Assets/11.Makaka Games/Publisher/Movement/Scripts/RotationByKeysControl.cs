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
    public class RotationByKeysControl : MonoBehaviour
    {
        [Header("Horizontal")]
        [Tooltip("Object for Horizontal Rotation")]
        [SerializeField]
        private Transform horizontal;

        [SerializeField]
        private float speedHorizontal = 75f;

        [SerializeField]
        private float inputHorizontalGravity = 6f;

        [SerializeField]
        private float inputHorizontalSensitivity = 1.5f;

        private float inputHorizontal = 0f;
        private float inputHorizontalTarget = 0f;

        [Header("Vertical")]
        [Tooltip("Object for Vertical Rotation")]
        [SerializeField]
        private Transform vertical;

        [SerializeField]
        private float speedVertical = -90f;

        [SerializeField]
        private float inputVerticalGravity = 4f;

        [SerializeField]
        private float inputVerticalSensitivity = 1f;

        private float inputVertical = 0f;
        private float inputVerticalTarget = 0f;

        private float deltaTimeNormalized;

        private readonly Key[] positiveHorizontalKeys = new[] { Key.D, Key.RightArrow };
        private readonly Key[] negativeHorizontalKeys = new[] { Key.A, Key.LeftArrow };

        private readonly Key[] positiveVerticalKeys = new[] { Key.W, Key.UpArrow };
        private readonly Key[] negativeVerticalKeys = new[] { Key.S, Key.DownArrow };

        private Keyboard keyboard;

        private void Start()
        {   
            keyboard = Keyboard.current;

            if (keyboard == null)
            {
                DebugPrinter.Print($"Keyboard not found.");
            }
        }

        private void LateUpdate()
        {
            if (keyboard != null)
            {
                // Determine target input values
                
                inputHorizontalTarget =
                    GetAxis(positiveHorizontalKeys, negativeHorizontalKeys);

                inputVerticalTarget =
                    GetAxis(positiveVerticalKeys, negativeVerticalKeys);

                // Smooth the inputs
                deltaTimeNormalized = Time.deltaTime / Time.timeScale;

                inputHorizontal = Mathf.MoveTowards(inputHorizontal,
                    inputHorizontalTarget * inputHorizontalSensitivity,
                    inputHorizontalGravity * deltaTimeNormalized);

                inputVertical = Mathf.MoveTowards(inputVertical,
                    inputVerticalTarget * inputVerticalSensitivity,
                    inputVerticalGravity * deltaTimeNormalized);

                // Apply rotation
                horizontal.Rotate(0f, inputHorizontal * speedHorizontal * deltaTimeNormalized, 0f);
                vertical.Rotate(inputVertical * speedVertical * deltaTimeNormalized, 0f, 0f);
            }
        }

        /// <summary>
        /// Returns an axis value in the range [-1, 1] by checking
        /// any number of positive and negative keys.
        /// </summary>
        private float GetAxis(Key[] positiveKeys, Key[] negativeKeys)
        {   
            bool positivePressed = false, negativePressed = false;

            foreach (var key in positiveKeys)
                positivePressed |= keyboard[key].isPressed;

            foreach (var key in negativeKeys)
                negativePressed |= keyboard[key].isPressed;

            return (positivePressed ? 1f : 0f) - (negativePressed ? 1f : 0f);
        }
    }
}