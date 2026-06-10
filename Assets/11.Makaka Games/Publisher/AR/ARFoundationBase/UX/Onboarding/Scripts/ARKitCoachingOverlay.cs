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
using UnityEngine.XR.ARFoundation;

#if UNITY_IOS

using UnityEngine.XR.ARKit;

#endif

using System;

using MakakaGames.Publisher.Debugging;

/// <summary>
/// This example shows how to activate the [ARCoachingOverlayView]
/// (https://developer.apple.com/documentation/arkit/arcoachingoverlayview)
/// </summary>

namespace MakakaGames.Publisher.AR.ARFoundationBase.UX.Onboarding
{
    [RequireComponent(typeof(ARSession))]
    [HelpURL("https://makaka.org/unity-assets")]
    public class ARKitCoachingOverlay : MonoBehaviour
    {
        // Duplicate the ARCoachingGoal enum
        // so that we can use it on a serialized field
        private enum CoachingGoal
        {
            Tracking,
            HorizontalPlane,
            VerticalPlane,
            AnyPlane
        }

        [SerializeField]
        [Tooltip("The coaching goal associated with the coaching overlay.")]

    #if !UNITY_IOS

        #pragma warning disable CS0414

    #endif

        private CoachingGoal m_Goal = CoachingGoal.Tracking;

    #if !UNITY_IOS

        #pragma warning restore CS0414

    #endif

    #if UNITY_IOS

        /// <summary>
        /// https://developer.apple.com/documentation/arkit/arcoachinggoal
        /// associated with the coaching overlay
        /// </summary>
        public ARCoachingGoal Goal
        {
            get
            {
                if (GetComponent<ARSession>().subsystem
                    is ARKitSessionSubsystem sessionSubsystem)
                {
                    return sessionSubsystem.requestedCoachingGoal;
                }
                else
                {
                    return (ARCoachingGoal)m_Goal;
                }
            }

            set
            {
                m_Goal = (CoachingGoal)value;

                if (Supported && GetComponent<ARSession>().subsystem
                    is ARKitSessionSubsystem sessionSubsystem)
                {
                    sessionSubsystem.requestedCoachingGoal = value;
                }
            }
        }

    #endif

        [SerializeField]
        [Tooltip("Whether the coaching overlay activates automatically.")]
        private bool m_ActivatesAutomatically = true;

        /// <summary>
        /// Whether the coaching overlay activates automatically
        /// </summary>
        public bool ActivatesAutomatically
        {
            get
            {

    #if UNITY_IOS

                if (Supported && GetComponent<ARSession>().subsystem
                    is ARKitSessionSubsystem sessionSubsystem)
                {
                    return sessionSubsystem.coachingActivatesAutomatically;
                }

    #endif

                return m_ActivatesAutomatically;
            }

            set
            {
                m_ActivatesAutomatically = value;

    #if UNITY_IOS

                if (Supported && GetComponent<ARSession>().subsystem
                    is ARKitSessionSubsystem sessionSubsystem)
                {
                    sessionSubsystem.coachingActivatesAutomatically = value;
                }

    #endif

            }
        }

        /// <summary>
        /// Whether the [ARCoachingGoal]
        /// (https://developer.apple.com/documentation/arkit/arcoachinggoal)
        /// is supported.
        /// </summary>
        public bool Supported
        {
            get
            {

    #if UNITY_IOS

                return ARKitSessionSubsystem.coachingOverlaySupported;

    #else

                return false;
    #endif

            }
        }

        private void OnEnable()
        {

    #if UNITY_IOS

            if (Supported && GetComponent<ARSession>().subsystem
                is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.requestedCoachingGoal = (ARCoachingGoal)m_Goal;
                sessionSubsystem.coachingActivatesAutomatically =
                    m_ActivatesAutomatically;
            }
            else

    #endif

            {
                DebugPrinter.Print(
                    "ARCoachingOverlayView is not supported by this device.");
            }
        }

        /// <summary>
        /// Activates the [ARCoachingGoal]
        /// (https://developer.apple.com/documentation/arkit/arcoachinggoal)
        /// </summary>
        /// <param name="animated">If <c>true</c>, the coaching overlay is animated,
        /// e.g. fades in. If <c>false</c>, the coaching overlay appears instantly,
        /// without any transition.</param>
        public void ActivateCoaching(bool animated)
        {

    #if UNITY_IOS

            if (Supported && GetComponent<ARSession>().subsystem
                is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.SetCoachingActive(
                    true,
                    animated ? ARCoachingOverlayTransition.Animated
                        : ARCoachingOverlayTransition.Instant);
            }
            else

    #endif

            {
                throw new NotSupportedException(
                    "ARCoachingOverlay is not supported");
            }
        }

        
        public void DisableCoaching(bool animated)
        {

    #if UNITY_IOS

            if (Supported && GetComponent<ARSession>().subsystem
                is ARKitSessionSubsystem sessionSubsystem)
            {
                sessionSubsystem.SetCoachingActive(
                    false,
                    animated ? ARCoachingOverlayTransition.Animated
                        : ARCoachingOverlayTransition.Instant);
            }
            else

    #endif

            {
                throw new NotSupportedException(
                    "ARCoachingOverlay is not supported");
            }
        }
    }
}