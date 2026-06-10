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

using System;
using System.Linq;

using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.MaterialControlX;

#pragma warning disable 649

namespace MakakaGames.ThrowControlX
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ThrowingObject : MonoBehaviour
    {
        public Rigidbody rigidbody3D;
        private Collider[] colliders3D;

        public MaterialControl materialControl;

        [Header("Custom Data")]
        [Tooltip("It’s useful to assign a specific control script for a unique type"
            + "of Throwing Object and access to it outside the Throwing System.")]
        public MonoBehaviour monoBehaviourCustom;

        [Header("Force")]
        [Tooltip("Controls the Throwing Force. The higher the last position of the"
            + " mouse cursor (or finger) during the click (or tap, or key press),"
            + " the greater the Throwing Force that is formed from User Input"
            + " and parameters in the Throwing System:"
            + "\n\n\tThrowControl.cs:\n\tInput Sensitivity, Force Factor Extra."
            + "\n\n\tThrowingObject.cs:\n\tForce Factor, Force Direction Extra.")]
        [SerializeField]
        private float forceFactor;
        private Vector3 forceDirection;
        private Vector2 forceBase;
        private Vector2 inputPositionDelta;

        [Tooltip("Force Direction is formed automatically based on User Input and"
            + " Input Sensitivity, but you can clarify the direction with an Extra"
            + " value based on Axis of the Camera’s Transform.")]
        [SerializeField]
        private CameraAxes forceDirectionExtra = CameraAxes.TransformUp;
        private Vector3 forceDirectionExtraVector3;
        public enum CameraAxes
        {
            TransformUp,
            TransformForward,
            TransformRight,
            TransformUpRight,
            TransformLeft,
            TransformUpLeft
        }

        [Header("Torque")]
        public CameraAxes torqueAxis = CameraAxes.TransformRight;

        private Vector3 torqueAxisVector3;

        private float torqueAngleBasic;

        [SerializeField]
        private float torqueAngle;

        [SerializeField]
        private float torqueFactor;

        private Quaternion torqueRotation;

        [Tooltip("It clamps Torque.")]
        [SerializeField]
        private float maxAngularVelocityAtAwake = 7f;

        [Header("Center Of Mass (Com)")]
        [Space]
        [Tooltip("Overrides the same exposed property of Rigidbody component just"
            + " for convenience.")]
        [SerializeField]
        private bool isComCustomizedAtAwake = false;

        [Tooltip("It’s relevant when using flag:\nIs Com Customized At Awake.")]
        [SerializeField]
        private Vector3 comCustom;

        [Space]
        [SerializeField]
        [Tooltip("If the Center of Mass by Default is not correct,"
            + "\nyou can use it as a base point for improving with Custom value.")]
        private bool isComByDefaultLoggedAtAwake = false;

        [Space]
        [SerializeField]
        [Tooltip("The way for Debugging Center of Mass visually.")]
        private bool isComCustomDrawnWithGizmo = false;

        [SerializeField]
        [Tooltip("It’s relevant when using flag:\nIs Com Custom Drawn With Gizmo.")]
        private float comCustomGizmoRadius = 0.08f;

    #if UNITY_EDITOR

        [Space]
        [SerializeField]
        [Tooltip("To Customize Center Of Mass without Restart in FixedUpdate().")]
        private bool isComCustomizedAtFUpdateInEditor = false;

    #endif

        private Quaternion rotationByDefault;
        public enum RotationsForNextThrow
        {
            Default,
            Random,
            Custom
        }

        [Header("Position")]
        [Tooltip("Spawn position [x, y]. Middle is at the bottom of the screen:"
            + " (0.5f, 0.1f). Y must be less than"
            + " Input Position Fixed Screen Factor Y of ThrowControl.cs in case of"
            + " using.")]
        public Vector2 positionInViewportOnReset = new(0.5f, 0.1f);

        [Tooltip("Spawn position [z]. Used for Z coordinate of 3D Position On"
            +" Reset.")]
        public float cameraNearClipPlaneFactorOnReset = 7.5f;

        [Header("Rotation")]
        [SerializeField]
        [Tooltip("Affects the Rotation at the Moment of the Throw.")]
        private bool isObjectRotatedInThrowDirection = true;

        [SerializeField]
        [Tooltip("Affects the Rotation before the Throw.")]
        private RotationsForNextThrow rotationOnReset =
            RotationsForNextThrow.Default;

        [SerializeField]
        [Tooltip("This Vector3 will be used when setting\nRotation On Reset to"
            +" Custom.")]
        private Vector3 rotationOnResetCustom = new(0f, 90f, 0f);

        [Header("Scale")]
        [SerializeField]
        [Tooltip("Affects the Rotation at the Moment of the Throw.")]
        private bool isScaleCustomizedAtAwake = false;

        [Tooltip("It’s relevant when using flag:\nIs Scale Customized At Awake.")]
        [SerializeField]
        private Vector3 scaleCustom = Vector3.one;

        [Header("Audio")]
        [SerializeField]
        [Tooltip("The more Audio Sources you have, the less distortion there will"
            + " be (e.g., for many collisions at a time).")]
        private AudioSource[] audioSources;
        private float[] audioSourcesPlayTimes;

        [SerializeField]
        [Tooltip("Array for customizing any dynamic Audio Data based on speed,"
            + " pitch, and volume factors of Throwing Object. Access from"
            + " ThrowControl.cs or ThrowingObject.cs with"
            + " PlayAudioRandomlyDependingOnSpeed() function:"
            + "\n\n— 0th Element is used for Throw Audio (Whoosh)."
            + "\n\n— Other Elements can be used for any Custom Game Logic"
            + " (e.g., for Customizing Collisions with different surfaces"
            + " (floor, wall, etc.).")]
        private AudioData[] audioDataCustom;

        [HideInInspector]
        public bool isThrown = false;

        private RigidbodyInterpolation interpolationByDefault;

        public event Action OnThrow;
        public event Action OnResetPhysicsBase;

        private void Awake()
        {
            if (isScaleCustomizedAtAwake)
            {
                transform.localScale = scaleCustom;
            }

            rigidbody3D.maxAngularVelocity = maxAngularVelocityAtAwake;

            if (isComByDefaultLoggedAtAwake)
            {
                DebugPrinter.Print($"[Center Of Mass] by Default: {name}");
                DebugPrinter.Print($"Vector3(" +
                    $"{rigidbody3D.centerOfMass.x}," +
                    $"{rigidbody3D.centerOfMass.y}," +
                    $"{rigidbody3D.centerOfMass.z})");
            }

            if (isComCustomizedAtAwake)
            {
                rigidbody3D.centerOfMass = comCustom;
            }

            colliders3D = GetComponentsInChildren<Collider>();

            rotationByDefault = rigidbody3D.rotation;

            interpolationByDefault = rigidbody3D.interpolation;

            if (!materialControl)
            {
                Debug.LogWarning(gameObject.name + " — materialControl is Null!");
            }

            audioSourcesPlayTimes = new float[audioSources.Length];
        }

    #if UNITY_EDITOR

        private void FixedUpdate()
        {
            if (isComCustomizedAtFUpdateInEditor)
            {
                rigidbody3D.centerOfMass = comCustom;
            }
        }

    #endif

        private void OnDrawGizmos()
        {
            if (isComCustomDrawnWithGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(
                    transform.position
                        + transform.rotation * comCustom,
                    comCustomGizmoRadius);
            }
        }

        public void SetRendererEnabled(bool enabled)
        {
            if (materialControl)
            {
                materialControl.SetRendererEnabled(enabled);
            }
        }

        public void SetMaterial(Material material)
        {
            if (materialControl)
            {
                materialControl.SetMaterial(material);
            }
        }

        public void SetMaterial(int index)
        {
            if (materialControl)
            {
                materialControl.SetMaterial(index);
            }
        }

        public void ThrowBase(
            Vector2 inputPositionFirst,
            Vector2 inputPositionLast,
            Vector2 inputSensitivity,
            Transform cameraMain,
            int screenHight,
            float forceFactorExtra,
            float torqueFactorExtra,
            float torqueAngleExtra)
        {
            inputPositionDelta = inputPositionLast - inputPositionFirst;

            if (inputPositionLast.y < screenHight / 2
                && Mathf.Abs(inputPositionDelta.y) > 0f)
            {
                inputPositionDelta.x *= inputPositionLast.y / inputPositionDelta.y;

                //DebugPrinter.Print("[Correction] strengthFactor")
            }

            inputPositionDelta /= screenHight;

            forceBase.y = inputSensitivity.y * inputPositionDelta.y;
            forceBase.x = inputSensitivity.x * inputPositionDelta.x;

            forceDirection = new Vector3(forceBase.x, 0f, 1f);
            forceDirection =
                cameraMain.transform.TransformDirection(forceDirection);

            torqueAngleBasic = Mathf.Sign(inputPositionDelta.x)
                * Vector3.Angle(cameraMain.transform.forward, forceDirection);

            torqueRotation = Quaternion.AngleAxis(
                torqueAngleBasic + torqueAngle + torqueAngleExtra,
                cameraMain.transform.up);

            rigidbody3D.useGravity = true;
            rigidbody3D.interpolation = interpolationByDefault;

            forceDirectionExtraVector3 =
                GetCameraAxis(cameraMain, forceDirectionExtra);

            rigidbody3D.AddForce(
                (forceFactor + forceFactorExtra)
                * forceBase.y
                * (forceDirection + forceDirectionExtraVector3));

            if (isObjectRotatedInThrowDirection)
            {
                rigidbody3D.rotation =
                    Quaternion.AngleAxis(
                        Mathf.Sign(inputPositionDelta.x) * Vector3.Angle(
                            cameraMain.transform.forward, forceDirection),
                        cameraMain.transform.up)
                    * rigidbody3D.rotation;
            }

            torqueAxisVector3 = GetCameraAxis(cameraMain, torqueAxis);

            rigidbody3D.AddTorque(torqueRotation * torqueAxisVector3
                * (torqueFactor + torqueFactorExtra));

            OnThrow?.Invoke();
        }

        private Vector3 GetCameraAxis(Transform cameraMain, CameraAxes cameraAxis)
        {
            return cameraAxis switch
            {
                CameraAxes.TransformUp => cameraMain.transform.up,

                CameraAxes.TransformForward => cameraMain.transform.forward,

                CameraAxes.TransformRight => cameraMain.transform.right,

                CameraAxes.TransformUpRight =>
                    cameraMain.transform.right + cameraMain.transform.up,

                CameraAxes.TransformLeft => cameraMain.transform.right * -1f,

                CameraAxes.TransformUpLeft =>
                    cameraMain.transform.right * -1f + cameraMain.transform.up,

                _ => Vector3.zero
            };
        }

        public void ResetPhysicsBase()
        {
            //Debug.Log("ResetPhysics()");

            rigidbody3D.useGravity = false;
            rigidbody3D.linearVelocity = Vector3.zero;
            rigidbody3D.angularVelocity = Vector3.zero;
            rigidbody3D.interpolation = RigidbodyInterpolation.None;

            OnResetPhysicsBase?.Invoke();
        }

        public void ResetPosition(Camera cameraMain)
        {
            Vector3 positionTargetTemp =
                cameraMain.ViewportToWorldPoint(new Vector3(
                    positionInViewportOnReset.x,
                    positionInViewportOnReset.y,
                    cameraMain.nearClipPlane * cameraNearClipPlaneFactorOnReset));

            transform.position = positionTargetTemp;
        }

        public void ResetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void ResetRotation(Transform parent)
        {
            //DebugPrinter.Print(rotationByDefault.eulerAngles);

            Quaternion rotationTargetTemp;

            switch (rotationOnReset)
            {
                case RotationsForNextThrow.Default:
                default:

                    if (parent)
                    {
                        rotationTargetTemp = parent.rotation * rotationByDefault;
                    }
                    else
                    {
                        rotationTargetTemp = rotationByDefault;
                    }

                    break;

                case RotationsForNextThrow.Random:

                    rotationTargetTemp = GetRandomRotation();

                    break;

                case RotationsForNextThrow.Custom:

                    if (parent)
                    {
                        rotationTargetTemp = parent.rotation
                            * Quaternion.Euler(rotationOnResetCustom);
                    }
                    else
                    {
                        rotationTargetTemp =
                            Quaternion.Euler(rotationOnResetCustom);
                    }

                    break;
            }

            transform.rotation = rotationTargetTemp;
        }

        private Quaternion GetRandomRotation()
        {
            Quaternion randomRotation = new()
            {
                eulerAngles = new Vector3(
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f),
                    UnityEngine.Random.Range(0f, 360f))
            };

            return randomRotation;
        }

        public void PlayAudioWhoosh()
        {
            PlayAudioRandomlyDependingOnSpeed(0, true);
        }

        public void PlayAudioRandomlyDependingOnSpeed(
            int index,
            bool isStoppedBeforePlay,
            AudioSource audioSource = null)
        {
            if (index >= 0
                && index < audioDataCustom.Length
                && audioDataCustom[index] != null)
            {
                if (audioSource)
                {
                    PlayAudioRandomlyDependingOnSpeed(
                        audioDataCustom[index], isStoppedBeforePlay, audioSource);
                }
                else
                {
                    PlayAudioRandomlyDependingOnSpeed(
                        audioDataCustom[index], isStoppedBeforePlay);
                }
            }
            else
            {
                DebugPrinter.Print("Audio Data doesn't exist at index: " + index);
            }
        }

        public void PlayAudioRandomlyDependingOnSpeed(
            AudioData audioData,
            bool isStoppedBeforePlay)
        {
            int availableIndex = 
                Array.FindIndex(audioSources, a => !a.isPlaying);

            if (availableIndex == -1) // all sources are playing
            {
                //DebugPrinter.Print($"LRU Playing AudioSource will be used");

                // Least Recently Used => the oldest playing AudioSource
                availableIndex = Array.IndexOf(
                    audioSourcesPlayTimes, audioSourcesPlayTimes.Min());
            }
            else
            {
                //DebugPrinter.Print($"NOT Playing AudioSource will be used");
            }

            // DebugPrinter.Print($"AudioSource: {availableIndex},"
            //     + $" time: {audioSourcesPlayTimes[availableIndex]}");

            audioSourcesPlayTimes[availableIndex] = Time.time;

            PlayAudioRandomlyDependingOnSpeed(
                audioData, isStoppedBeforePlay, audioSources[availableIndex]);
        }

        public void PlayAudioRandomlyDependingOnSpeed(
            AudioData audioData, bool isStoppedBeforePlay, AudioSource audioSource)
        {
            float speedClamp = Mathf.Clamp(
                rigidbody3D.linearVelocity.magnitude,
                audioData.speedClampMin,
                audioData.speedClampMax);

            audioSource.pitch =
                audioData.pitchMin + speedClamp * audioData.pitchFactor;

            if (isStoppedBeforePlay)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(audioData.GetRandomClip(),
                speedClamp * audioData.volumeFactor);

            //DebugPrinter.Print(
            //    $"AudioSource: {audioSource.name}, Volume: {audioSource.volume}");
        }

        public void ActivateTriggersOnColliders(bool isTrigger)
        {
            for (int i = 0; i < colliders3D.Length; i++)
            {
                colliders3D[i].isTrigger = isTrigger;
            }
        }

        public void SetCollidersEnabled(bool enabled)
        {
            for (int i = 0; i < colliders3D.Length; i++)
            {
                colliders3D[i].enabled = enabled;
            }
        }

        [System.Serializable]
        public class AudioData
        {
            public string nameForLog;

            public AudioClip[] audioClips;

            public float speedClampMin = 0f;
            public float speedClampMax = 15f;

            [Range(-3f, 3f)]
            public float pitchMin = 0.8f;
            public float pitchFactor = 0.02f;

            public float volumeFactor = 0.125f;

            public AudioData() { }

            public AudioData(
                string nameForLog,
                AudioClip[] audioClips,
                float speedClampMin = 0f,
                float speedClampMax = 15f,
                float pitchMin = 0.8f,
                float pitchFactor = 0.02f,
                float volumeFactor = 0.125f)
            {
                this.nameForLog = nameForLog;
                this.audioClips = audioClips;
                this.speedClampMin = speedClampMin;
                this.speedClampMax = speedClampMax;
                this.pitchMin = pitchMin;
                this.pitchFactor = pitchFactor;
                this.volumeFactor = volumeFactor;
            }

            public AudioClip GetRandomClip()
            {
                if (audioClips != null && audioClips.Length > 0)
                {
                    return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}