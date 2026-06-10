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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

using Unity.XR.CoreUtils;

using System.Collections;
using System.Collections.Generic;

using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.AR.ARFoundationBase.UX.Common;
using MakakaGames.Publisher.AR.ARFoundationBase.UX.Onboarding;

namespace MakakaGames.Publisher.AR.ARFoundationBase.PlaneDetection
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class ARPlaneDetectionControl : MonoBehaviour
    {
        [SerializeField]
        private ARSession arSession;

        [SerializeField]
        private ARInputManager arInputManager;

        [SerializeField]
        private ARKitCoachingOverlay arKitCoachingOverlay;

        [SerializeField]
        private XROrigin xrOrigin;

        [SerializeField]
        private ARPlaneManager arPlaneManager;

        [SerializeField]
        private ARPointCloudManager arPointCloudManager;

        [SerializeField]
        private ARRaycastManager arRaycastManager;

        private static List<ARRaycastHit> hitResults = new();

        [SerializeField]
        private ARCameraManager arCameraManager;

        private bool isFirstCameraFrameReceived = false;

        [SerializeField]
        private LightEstimationControl lightEstimationControl;

        [Space]
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        private GameObject placedPrefab;

        /// <summary>
        /// The object instantiated as a result of
        /// a successful raycast intersection with a plane.
        /// </summary>
        [SerializeField]
        private GameObject placedObject;

        [SerializeField]
        private bool isPlacedObjectRotatedToCamera = true;

        [SerializeField]
        private bool isObjectPlacementActivated = false;

        private bool isObjectPlacementForThe1stTime = true;

        [Header("UI")]
        [SerializeField]
        private UIManager uiManager;

        [SerializeField]
        private GameObject canvasTutorial;

        [SerializeField]
        private GameObject canvasConfirmation;

        private Vector3 posOfSpawnedObjectOnDetectedPlane;

        private bool IsDetectedPlaneConfirmed;

        [Header("Events")]
        [Space]
        [SerializeField]
        private UnityEvent OnStarted;

        [Space]
        public UnityEvent OnCameraFrameFirstReceived;

        [Space]
        [SerializeField]
        private UnityEvent OnAREnabling = null;

        private Coroutine enableARCoroutineReference;

        [Space]
        public UnityEvent<Vector3> OnPlacementHitPoseChanged = null;

        [Space]
        public UnityEvent OnObjectPlaced = null;

        [Space]
        public UnityEvent<Vector3> OnPlaneConfirmedWithDetectedPoint;

        [Space]
        public UnityEvent<Transform> OnPlaneConfirmedWithCamera;

        [Space]
        public UnityEvent OnResetARSession;

        private void OnEnable()
        {
            arCameraManager.frameReceived += OnCameraFrameReceived;
        }

        private IEnumerator Start()
        {
            OnStarted?.Invoke();

            arInputManager.enabled = true;

            yield return null;

            xrOrigin.gameObject.SetActive(true);

            arSession.gameObject.SetActive(true);

            canvasTutorial.SetActive(true);
        }

        private void OnDisable()
        {
            arCameraManager.frameReceived -= OnCameraFrameReceived;
        }

        public void EnableAR()
        {
            if (enableARCoroutineReference != null)
            {
                StopCoroutine(enableARCoroutineReference);

                enableARCoroutineReference = null;
            }

            enableARCoroutineReference = StartCoroutine(EnableARCoroutine());
        }

        private IEnumerator EnableARCoroutine()
        {
            uiManager.gameObject.SetActive(true);

            yield return null;

            arSession.Reset();

            uiManager.ResetUXOrderedQueue();

            canvasTutorial.SetActive(false);

            yield return null;

            arPlaneManager.enabled = true;

            arPointCloudManager.enabled = true;

            SetObjectPlacementActivated(true);

            OnPlacementHitPoseChanged.AddListener(
                SavePositionOfSpawnedObjectOnDetectedPlane);

            OnAREnabling?.Invoke();

            arKitCoachingOverlay.enabled = true;

            yield return null;

            enableARCoroutineReference = null;
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            if (!isFirstCameraFrameReceived)
            {
                DebugPrinter.Print(
                    "\n AR Foundation Camera - First Frame Received");

                isFirstCameraFrameReceived = true;

                OnCameraFrameFirstReceived.Invoke();
            }
        }

        public void SavePositionOfSpawnedObjectOnDetectedPlane(
            Transform transformForPos)
        {
            SavePositionOfSpawnedObjectOnDetectedPlane(transformForPos.position);
        }

        public void SavePositionOfSpawnedObjectOnDetectedPlane(Vector3 pos)
        {
            posOfSpawnedObjectOnDetectedPlane = pos;

            if (!IsDetectedPlaneConfirmed)
            {
                canvasConfirmation.SetActive(true);
            }
        }

        public void ConfirmDetectedPlane()
        {
            IsDetectedPlaneConfirmed = true;

            canvasConfirmation.SetActive(false);

            StopObjectPlacement();

            // detected planes will be stay on them places
            arPlaneManager.SetTrackablesActive(false);
            arPlaneManager.enabled = false;

            arPointCloudManager.SetTrackablesActive(false);
            arPointCloudManager.enabled = false;

            OnPlaneConfirmedWithDetectedPoint.Invoke(
                posOfSpawnedObjectOnDetectedPlane);

            OnPlaneConfirmedWithCamera.Invoke(arCameraManager.transform);

            lightEstimationControl.enabled = true;

            uiManager.ReachGoalsForcibly();
        }

        public void ResetARSession()
        {
            DeactivatePlacedObject();

            arSession.Reset();

            OnResetARSession.Invoke();

            uiManager.ResetUXOrderedQueue();
        }

        public void PlaceObjectOnConfirmedPlaneUnderCamera(Transform targetObject)
        {
            xrOrigin.MakeContentAppearAt(
                targetObject,
                new Vector3(
                    arCameraManager.transform.position.x,
                    posOfSpawnedObjectOnDetectedPlane.y,
                    arCameraManager.transform.position.z));
        }

        public void PlaceObjectOnConfirmedPlaneUnderDetectedPoint(
            Transform targetObject)
        {
            xrOrigin.MakeContentAppearAt(
                targetObject,
                posOfSpawnedObjectOnDetectedPlane);
        }

        public void MoveObjectXZLocalPosToCamera(
            Transform targetObject)
        {
            targetObject.transform.localPosition = new Vector3(
                arCameraManager.transform.position.x
                    - xrOrigin.transform.position.x,
                targetObject.transform.localPosition.y,
                arCameraManager.transform.position.z
                    - xrOrigin.transform.position.z);
        }

        public void SetARScaleInverted(float scale = 2f)
        {
            xrOrigin.transform.localScale =
                Vector3.one * scale;
        }

        public void SetContentRotation(Transform content, Quaternion rotation)
        {
            xrOrigin.MakeContentAppearAt(
                content, content.transform.position, rotation);
        }

        public void AddObjectOnDetectedPlane(InputAction.CallbackContext context)
        {
            if (isObjectPlacementActivated)
            {
                Vector2 touchPosition = context.ReadValue<Vector2>();

                //DebugPrinter.Print(touchPosition);

                // On the Desktop: Vector2.zero is returned in the last frame 
                // in Binding with One modifier, so this is Fix for Unity Bug

                if (touchPosition == Vector2.zero)
                {
                    return;
                }

                if (IsPointerOverUIObject(touchPosition))
                {
                    //DebugPrinter.Print("UI object blocks AR Raycast Hit");
                }
                else
                {
                    if (arRaycastManager.Raycast(touchPosition, hitResults,
                        TrackableType.PlaneWithinPolygon))
                    {
                        // Raycast hits are sorted by distance, so the first one
                        // will be the closest hit.
                        Pose hitPose = hitResults[0].pose;

                        //DebugPrinter.Print($"hitPose: {hitPose}");

                        OnPlacementHitPoseChanged.Invoke(hitPose.position);

                        if (placedObject)
                        {
                            if (isObjectPlacementForThe1stTime
                                || !placedObject.activeSelf)
                            {
                                isObjectPlacementForThe1stTime = false;

                                placedObject.SetActive(true);

                                DebugPrinter.Print("OnObjectPlaced.Invoke()");

                                uiManager.SetObjectPlacedOnPlane(true);

                                OnObjectPlaced.Invoke();
                            }

                            placedObject.transform.position = hitPose.position;
                        }
                        else
                        {
                            placedObject = Instantiate(
                                placedPrefab, hitPose.position, hitPose.rotation);

                            if (placedObject)
                            {
                                DebugPrinter.Print("OnObjectPlaced.Invoke()");

                                uiManager.SetObjectPlacedOnPlane(true);

                                OnObjectPlaced.Invoke();
                            }
                        }

                        if (isPlacedObjectRotatedToCamera)
                        {
                            RotatePlacedObjectToCamera();
                        }
                    }
                }
            }
        }

        private bool IsPointerOverUIObject(Vector2 touchPosition)
        {
            PointerEventData pointerEventData =
                new(EventSystem.current)
                {
                    position = touchPosition
                };

            List<RaycastResult> raycastResults = new();

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            return raycastResults.Count > 0;
        }

        private void StopObjectPlacement()
        {
            SetObjectPlacementActivated(false);

            DeactivatePlacedObject();
        }

        private void DeactivatePlacedObject()
        {
            if (placedObject)
            {
                placedObject.SetActive(false);
            }
        }

        public void SetObjectPlacementActivated(bool isActivated)
        {
            isObjectPlacementActivated = isActivated;

            arRaycastManager.enabled = isActivated;
        }

        public Vector3 RotatePlacedObjectToCamera()
        {
            placedObject.transform.LookAt(arCameraManager.transform);

            return placedObject.transform.eulerAngles = new Vector3(
                0f, placedObject.transform.eulerAngles.y, 0f);
        }

        public Vector3 GetPlacedObjectRotation()
        {
            return placedObject.transform.eulerAngles;
        }

        public void SetPlacedObject(GameObject placedObject)
        {
            this.placedObject = placedObject;
        }
    }
}