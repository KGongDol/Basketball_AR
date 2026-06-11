using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using MakakaGames.ThrowControlX;

/// <summary>
/// 부스 모드: 공 드래그(플릭) 궤적을 라인렌더러로 화면에 표시한다.
/// ThrowControl의 Flick 모드와 같은 조건(아직 던져지지 않은 공 위에서
/// 드래그 시작)일 때만 그리고, 손을 떼면 지운다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BoothDragLineControl : MonoBehaviour
{
    [Tooltip("AR 시작 후 cameraMain이 런타임에 할당되므로 ThrowControl을 경유해 읽는다")]
    [SerializeField] private ThrowControl throwControl;

    [Tooltip("라인이 그려지는 카메라 앞 깊이(미터)")]
    [SerializeField] private float depthFromCamera = 0.6f;

    [Tooltip("궤적 최대 포인트 수 (초과 시 꼬리부터 제거)")]
    [SerializeField] private int maxPoints = 48;

    [Tooltip("포인트 추가 최소 간격(월드 거리, 미터)")]
    [SerializeField] private float minPointDistance = 0.005f;

    [Tooltip("드래그 종료(발사) 후 라인이 사라질 때까지의 시간(초, 실시간 기준)")]
    [SerializeField] private float clearDelay = 2f;

    private LineRenderer lineRenderer;
    private Coroutine clearCoroutine;
    private readonly List<Vector3> points = new();
    private bool isDragging;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }

    private void Update()
    {
        Camera cam = throwControl ? throwControl.cameraMain : null;
        Pointer pointer = Pointer.current;

        if (!cam || pointer == null)
        {
            Clear();
            return;
        }

        bool isPressed = pointer.press.isPressed;
        Vector2 screenPosition = pointer.position.ReadValue();

        if (isPressed && !isDragging && pointer.press.wasPressedThisFrame
            && throwControl.enabled
            && IsOnThrowableObject(cam, screenPosition))
        {
            isDragging = true;

            CancelPendingClear();

            points.Clear();
        }

        if (!isDragging)
        {
            return;
        }

        if (isPressed)
        {
            AddPoint(cam, screenPosition);

            lineRenderer.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }
        }
        else
        {
            isDragging = false;

            clearCoroutine = StartCoroutine(ClearAfterDelayCoroutine());
        }
    }

    private bool IsOnThrowableObject(Camera cam, Vector2 screenPosition)
    {
        if (Physics.Raycast(
            cam.ScreenPointToRay(screenPosition), out RaycastHit hit, 100f))
        {
            ThrowingObject throwingObject =
                hit.collider.GetComponentInParent<ThrowingObject>();

            return throwingObject && !throwingObject.isThrown;
        }

        return false;
    }

    private void AddPoint(Camera cam, Vector2 screenPosition)
    {
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(
            screenPosition.x, screenPosition.y, depthFromCamera));

        if (points.Count == 0
            || (points[^1] - worldPoint).sqrMagnitude
                > minPointDistance * minPointDistance)
        {
            points.Add(worldPoint);

            if (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }
        }
    }

    private void Clear()
    {
        points.Clear();

        if (lineRenderer.positionCount != 0)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void CancelPendingClear()
    {
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);

            clearCoroutine = null;
        }
    }

    private IEnumerator ClearAfterDelayCoroutine()
    {
        yield return new WaitForSecondsRealtime(clearDelay);

        Clear();

        clearCoroutine = null;
    }
}
