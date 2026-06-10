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

using System;

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.FPSController
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 0.2f;
        public float XSensitivityWebGLFactor = 0.1f;

        private float targetHorizontalDelta;

        [Space]
        public float YSensitivity = 0.2f;
        public float YSensitivityWebGLFactor = 0.1f;

        private float targetVerticalDelta;

        [Space]
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;

        [Space]
        public bool smooth;
        public float smoothTime = 5f;

        [Space]
        public bool lockCursor = true;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        private Mouse mouse;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;

            mouse = Mouse.current;

            if (mouse == null)
            {
                DebugPrinter.Print($"Mouse not found.");
            }
        }

        public void LookRotation(Transform character, Transform camera)
        {
            if (mouse != null)
            {
                targetVerticalDelta = mouse.delta.x.ReadValue() * XSensitivity;
                targetHorizontalDelta = mouse.delta.y.ReadValue() * YSensitivity;

    #if UNITY_WEBGL && !UNITY_EDITOR

                ApplyWebGLCorrectionForDelta();

    #endif

                m_CharacterTargetRot *= Quaternion.Euler(0f, targetVerticalDelta, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-targetHorizontalDelta, 0f, 0f);

                if(clampVerticalRotation)
                {
                    m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
                }

                if(smooth)
                {
                    character.localRotation = Quaternion.Slerp(
                        character.localRotation, m_CharacterTargetRot,
                        smoothTime * Time.deltaTime);

                    camera.localRotation = Quaternion.Slerp(
                        camera.localRotation, m_CameraTargetRot,
                        smoothTime * Time.deltaTime);
                }
                else
                {
                    character.localRotation = m_CharacterTargetRot;
                    camera.localRotation = m_CameraTargetRot;
                }

                UpdateCursorLock();
            }
        }

        public void SetCursorLock(bool value)
        {
            if (mouse != null)
            {
                lockCursor = value;

                if(!lockCursor)
                {
                    //we force unlock the cursor if the user disable 
                    // the cursor locking helper
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        public void UpdateCursorLock()
        {
            if (mouse != null)
            {
                //if the user set "lockCursor" we check & properly lock the cursos
                if (lockCursor)
                {
                    if(Keyboard.current[Key.Escape].wasReleasedThisFrame)
                    {
                        m_cursorIsLocked = false;
                    }
                    else if(Mouse.current.leftButton.wasReleasedThisFrame)
                    {
                        m_cursorIsLocked = true;
                    }

                    if (m_cursorIsLocked)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    else if (!m_cursorIsLocked)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                }
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

        private void ApplyWebGLCorrectionForDelta()
        {
            targetHorizontalDelta *= XSensitivityWebGLFactor;
            targetVerticalDelta *= YSensitivityWebGLFactor;
        }
    }
}