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

using System;
using System.Collections;

namespace MakakaGames.Publisher.AR.ARFoundationBase.UX.Onboarding
{
    public struct UXHandle
    {
        public UIManager.InstructionUI InstructionalUI;
        public UIManager.InstructionGoals Goal;

        public UXHandle(UIManager.InstructionUI ui, UIManager.InstructionGoals goal)
        {
            InstructionalUI = ui;
            Goal = goal;
        }
    }

    [HelpURL("https://makaka.org/unity-assets")]
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        private bool m_StartWithInstructionalUI = true;

        public enum InstructionUI
        {
            CrossPlatformFindAPlane,
            FindAFace,
            FindABody,
            FindAnImage,
            FindAnObject,
            ARKitCoachingOverlay,
            TapToPlace,
            None
        };

        [SerializeField]
        private InstructionUI m_InstructionalUI;

        public enum InstructionGoals
        {
            FoundAPlane,
            FoundMultiplePlanes,
            FoundAFace,
            FoundABody,
            FoundAnImage,
            FoundAnObject,
            PlacedAnObject,
            None
        };

        [SerializeField]
        private InstructionGoals m_InstructionalGoal;

        [SerializeField]
        private bool m_ShowSecondaryInstructionalUI;

        [SerializeField]
        private InstructionUI m_SecondaryInstructionUI = InstructionUI.TapToPlace;

        [SerializeField]
        private InstructionGoals m_SecondaryGoal = InstructionGoals.PlacedAnObject;

        [SerializeField]
        [Tooltip("Fallback to cross platform UI if ARKit coaching overlay " +
            "is not supported")]
        private bool m_CoachingOverlayFallback;

        [SerializeField]
        private GameObject m_ARSessionOrigin;

        private Func<bool> m_GoalReached;

        private System.Collections.Generic.Queue<UXHandle> m_UXOrderedQueue;
        private UXHandle m_CurrentHandle;

        private bool m_ProcessingInstructions;
        private bool m_PlacedObject;

        [SerializeField]
        private ARPlaneManager m_PlaneManager;

        [SerializeField]
        private ARFaceManager m_FaceManager;

        [SerializeField]
        private ARHumanBodyManager m_BodyManager;

        [SerializeField]
        private ARTrackedImageManager m_ImageManager;

        [SerializeField]
        private ARTrackedObjectManager m_ObjectManager;

        [SerializeField]
        private ARUXAnimationManager m_AnimationManager;

        private bool m_FadedOff = false;

        private Coroutine resetUXOrderedQueueCoroutineReference;

        private void OnEnable()
        {
            ARUXAnimationManager.OnFadeOffComplete += FadeComplete;

            GetManagers();

            m_UXOrderedQueue = new System.Collections.Generic.Queue<UXHandle>();

            ResetUXOrderedQueue();
        }

        private void Update()
        {
            if (m_UXOrderedQueue.Count > 0 && !m_ProcessingInstructions)
            {
                // pop off
                m_CurrentHandle = m_UXOrderedQueue.Dequeue();

                // exit instantly, if the goal is already met it will skip
                // showing the first UI and move to the next in the queue 
                m_GoalReached = GetGoal(m_CurrentHandle.Goal);

                if (m_GoalReached.Invoke())
                {
                    return;
                }

                // fade on
                FadeOnInstructionalUI(m_CurrentHandle.InstructionalUI);

                m_ProcessingInstructions = true;

                m_FadedOff = false;
            }

            if (m_ProcessingInstructions)
            {
                // start listening for goal reached
                if (m_GoalReached.Invoke())
                {
                    // if goal reached, fade off
                    FadeOff();
                }
            }
        }

        private void OnDisable()
        {
            ARUXAnimationManager.OnFadeOffComplete -= FadeComplete;
        }

        public void ResetUXOrderedQueue()
        {
            if (resetUXOrderedQueueCoroutineReference != null)
            {
                StopCoroutine(resetUXOrderedQueueCoroutineReference);

                resetUXOrderedQueueCoroutineReference = null;
            }

            resetUXOrderedQueueCoroutineReference =
                StartCoroutine(ResetUXOrderedQueueCoroutine());
        }

        private IEnumerator ResetUXOrderedQueueCoroutine()
        {
            // wait for AR Session Reset
            yield return new WaitForSeconds(0.1f);

            if (m_StartWithInstructionalUI)
            {
                m_UXOrderedQueue.Enqueue(
                    new UXHandle(m_InstructionalUI, m_InstructionalGoal));
            }

            if (m_ShowSecondaryInstructionalUI)
            {
                m_UXOrderedQueue.Enqueue(
                    new UXHandle(m_SecondaryInstructionUI, m_SecondaryGoal));
            }
        }

        public void SetObjectPlacedOnPlane(bool isPlaced)
        {
            m_PlacedObject = isPlaced;
        }

        public void ReachGoalsForcibly()
        {
            if (m_UXOrderedQueue.Count > 0 || !m_ProcessingInstructions)
            {
                m_UXOrderedQueue.Clear();

                FadeOff();
            }
        }

        private void FadeOff()
        {
            if (!m_FadedOff)
            {
                m_FadedOff = true;

                m_AnimationManager.FadeOffCurrentUI();
            }
        }

        private void GetManagers()
        {
            if (m_ARSessionOrigin)
            {
                if (m_ARSessionOrigin.TryGetComponent(
                    out ARPlaneManager arPlaneManager))
                {
                    m_PlaneManager = arPlaneManager;
                }

                if (m_ARSessionOrigin.TryGetComponent(
                    out ARFaceManager arFaceManager))
                {
                    m_FaceManager = arFaceManager;
                }

                if (m_ARSessionOrigin.TryGetComponent(
                    out ARHumanBodyManager arHumanBodyManager))
                {
                    m_BodyManager = arHumanBodyManager;
                }

                if (m_ARSessionOrigin.TryGetComponent(
                    out ARTrackedImageManager arTrackedImageManager))
                {
                    m_ImageManager = arTrackedImageManager;
                }

                if (m_ARSessionOrigin.TryGetComponent(
                    out ARTrackedObjectManager arTrackedObjectManager))
                {
                    m_ObjectManager = arTrackedObjectManager;
                }
            }
        }

        private Func<bool> GetGoal(InstructionGoals goal)
        {
            return goal switch
            {
                InstructionGoals.FoundAPlane => PlanesFound,
                InstructionGoals.FoundMultiplePlanes => MultiplePlanesFound,
                InstructionGoals.FoundABody => BodyFound,
                InstructionGoals.FoundAFace => FaceFound,
                InstructionGoals.FoundAnImage => ImageFound,
                InstructionGoals.FoundAnObject => ObjectFound,
                InstructionGoals.PlacedAnObject => PlacedObject,
                _ => () => false,
            };
        }

        private void FadeOnInstructionalUI(InstructionUI ui)
        {
            switch (ui)
            {
                case InstructionUI.CrossPlatformFindAPlane:

                    m_AnimationManager.ShowCrossPlatformFindAPlane();

                    break;

                case InstructionUI.FindAFace:

                    m_AnimationManager.ShowFindFace();

                    break;

                case InstructionUI.FindABody:

                    m_AnimationManager.ShowFindBody();

                    break;

                case InstructionUI.FindAnImage:

                    m_AnimationManager.ShowFindImage();

                    break;

                case InstructionUI.FindAnObject:

                    m_AnimationManager.ShowFindObject();

                    break;

                case InstructionUI.ARKitCoachingOverlay:

                    if (m_AnimationManager.ARKitCoachingOverlaySupported())
                    {
                        m_AnimationManager.ShowCoachingOverlay();
                    }
                    else
                    {
                        // fall back to cross platform overlay
                        if (m_CoachingOverlayFallback)
                        {
                            m_AnimationManager.ShowCrossPlatformFindAPlane();
                        }
                    }

                    break;

                case InstructionUI.TapToPlace:

                    m_AnimationManager.ShowTapToPlace();

                    break;

                case InstructionUI.None:
                default:

                    break;
            }
        }

        private bool PlanesFound() =>
            m_PlaneManager && m_PlaneManager.trackables.count > 0;

        private bool MultiplePlanesFound() =>
            m_PlaneManager && m_PlaneManager.trackables.count > 1;

        private bool FaceFound() =>
            m_FaceManager && m_FaceManager.trackables.count > 0;

        private bool BodyFound() =>
            m_BodyManager && m_BodyManager.trackables.count > 0;

        private bool ImageFound() =>
            m_ImageManager && m_ImageManager.trackables.count > 0;

        private bool ObjectFound() =>
            m_ObjectManager && m_ObjectManager.trackables.count > 0;

        private void FadeComplete()
        {
            m_ProcessingInstructions = false;
        }

        private bool PlacedObject()
        {
            // reset flag to be used multiple times
            if (m_PlacedObject)
            {
                m_PlacedObject = false;

                return true;
            }

            return m_PlacedObject;
        }

        public void AddToQueue(UXHandle uxHandle)
        {
            m_UXOrderedQueue.Enqueue(uxHandle);
        }

        public void TestFlipPlacementBool()
        {
            m_PlacedObject = true;
        }
    }
}