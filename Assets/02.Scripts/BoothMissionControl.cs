using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

using MakakaGames.Basketball;
using MakakaGames.ThrowControlX;
using MakakaGames.Publisher.MaterialControlX;
using MakakaGames.Publisher.ExplosionWithSmoke;

/// <summary>
/// 부스 미션 모드: 시작 버튼 → 30초 타이머 → 골 수 결과.
/// - 공 수 제한 없음, 제한시간만 존재
/// - 결과 화면의 다시하기 버튼으로 리셋
/// </summary>
public class BoothMissionControl : MonoBehaviour
{
    private enum State { Ready, Playing, Result }

    [Header("라운드 설정")]
    [Tooltip("라운드 총 제한시간(초, 실시간 기준)")]
    [SerializeField] private float roundDuration = 30f;

    [Header("골대 거리 (k번째 골 이후, 시작 위치 기준 로컬 Z 오프셋)")]
    [Tooltip("배열 길이를 넘어서는 골은 마지막 값 유지")]
    [SerializeField] private float[] hoopZOffsetPerGoal = { 2f, 4f, 6f, 8f };

    [Header("게임 참조")]
    [SerializeField] private ThrowControl throwControl;

    [Header("골대 참조")]
    [SerializeField] private GameObject hoop;
    [SerializeField] private MaterialControl backboardFading;
    [SerializeField] private MaterialControl poleFading;
    [SerializeField] private MaterialControl netFading;
    [SerializeField] private MaterialControl ringHolderFading;
    [SerializeField] private MaterialControl ringFading;
    [SerializeField] private ExplosionControl explosionOnFadeOut;
    [SerializeField] private ExplosionControl explosionOnFadeIn;
    [SerializeField] private float hoopMoveDelay = 0.5f;

    [Header("UI")]
    [SerializeField] private GameObject canvasStart;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private TextMeshProUGUI goalCountText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject confettiFullscreen;

    [Tooltip("CanvasStart 안의 시작 버튼 이름 (비워두면 첫 번째 Button 사용)")]
    [SerializeField] private string startButtonName = "ButtonStartLoadingAnimation";

    private State state = State.Ready;

    private struct HoopPhysicsPose
    {
        public Transform transform;
        public Rigidbody rigidbody;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private readonly List<HoopPhysicsPose> hoopPhysicsPoses = new();
    private readonly List<Joint> hoopWorldJoints = new();

    private int goals;
    private float timeLeft;
    private float currentHoopZOffset;
    private bool isHoopMoving;

    private void OnEnable()
    {
        BasketballBallControl.OnGoal += HandleGoal;
    }

    private void OnDisable()
    {
        BasketballBallControl.OnGoal -= HandleGoal;
    }

    private void Start()
    {
        CaptureHoopPhysicsPoses();
        RegisterStartButton();
        ShowReady();
    }

    // Persistent listener는 씬 저장/재로드 시 유실될 수 있으므로 런타임에 직접 등록한다.
    private void RegisterStartButton()
    {
        if (canvasStart == null) return;

        Transform btnTransform = null;
        if (!string.IsNullOrEmpty(startButtonName))
            btnTransform = canvasStart.transform.Find("PanelStart/" + startButtonName);

        var btn = btnTransform != null
            ? btnTransform.GetComponent<UnityEngine.UI.Button>()
            : canvasStart.GetComponentInChildren<UnityEngine.UI.Button>(true);

        if (btn != null)
            btn.onClick.AddListener(OnStartButtonPressed);
    }

    private void CaptureHoopPhysicsPoses()
    {
        if (!hoop) return;

        foreach (Rigidbody rb in hoop.GetComponentsInChildren<Rigidbody>(true))
        {
            hoopPhysicsPoses.Add(new HoopPhysicsPose
            {
                transform = rb.transform,
                rigidbody = rb,
                localPosition = rb.transform.localPosition,
                localRotation = rb.transform.localRotation,
            });
        }

        foreach (Joint joint in hoop.GetComponentsInChildren<Joint>(true))
        {
            if (joint.connectedBody == null)
                hoopWorldJoints.Add(joint);
        }
    }

    private void RestoreHoopPhysicsPoses()
    {
        foreach (HoopPhysicsPose pose in hoopPhysicsPoses)
        {
            if (pose.transform)
            {
                pose.transform.localPosition = pose.localPosition;
                pose.transform.localRotation = pose.localRotation;
            }
        }

        foreach (Joint joint in hoopWorldJoints)
        {
            if (joint)
            {
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = joint.transform.TransformPoint(joint.anchor);
            }
        }
    }

    private void ZeroHoopPhysicsVelocities()
    {
        foreach (HoopPhysicsPose pose in hoopPhysicsPoses)
        {
            if (pose.rigidbody)
            {
                pose.rigidbody.linearVelocity = Vector3.zero;
                pose.rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (state == State.Ready) OnStartButtonPressed();
            else if (state == State.Playing) BasketballBallControl.OnGoal?.Invoke(false);
        }
#endif

        if (state != State.Playing) return;

        timeLeft -= Time.unscaledDeltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerHud();
            EndRound();
        }
        else
        {
            UpdateTimerHud();
        }
    }

    /// <summary>CanvasStart의 시작 버튼 onClick에 연결.</summary>
    public void OnStartButtonPressed()
    {
        if (state != State.Ready) return;

        if (missionText) missionText.gameObject.SetActive(true);

        state = State.Playing;
        goals = 0;
        timeLeft = roundDuration;
        UpdateGoalCountText();
        UpdateTimerHud();

        throwControl.enabled = true;
    }

    private void HandleGoal(bool isClearBall)
    {
        if (state != State.Playing) return;

        goals++;
        UpdateGoalCountText();
        MoveHoopForGoalCount(goals);
    }

    private void EndRound()
    {
        state = State.Result;
        throwControl.enabled = false;

        if (resultText) resultText.text = $"골 {goals}개!";
        if (resultPanel) resultPanel.SetActive(true);
        if (confettiFullscreen) confettiFullscreen.SetActive(true);
    }

    /// <summary>결과 화면의 다시하기 버튼 onClick에 연결. 시작 화면 없이 즉시 재시작.</summary>
    public void ResetRound()
    {
        StopAllCoroutines();
        isHoopMoving = false;

        ResetHoopPosition();

        if (confettiFullscreen) confettiFullscreen.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);

        // state를 Ready로 복원한 뒤 OnStartButtonPressed를 직접 호출해 바로 재시작
        state = State.Ready;
        OnStartButtonPressed();
    }

    private void ShowReady()
    {
        if (missionText) missionText.gameObject.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (confettiFullscreen) confettiFullscreen.SetActive(false);
        goals = 0;
        UpdateGoalCountText();
    }

    private void UpdateTimerHud()
    {
        if (missionText) missionText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    private void UpdateGoalCountText()
    {
        if (goalCountText) goalCountText.text = goals.ToString();
    }

    private void MoveHoopForGoalCount(int goalCount)
    {
        if (hoopZOffsetPerGoal == null || hoopZOffsetPerGoal.Length == 0 || isHoopMoving || !hoop) return;

        int index = Mathf.Min(goalCount - 1, hoopZOffsetPerGoal.Length - 1);
        float targetOffset = hoopZOffsetPerGoal[index];

        if (Mathf.Approximately(targetOffset, currentHoopZOffset)) return;

        StartCoroutine(MoveHoopCoroutine(targetOffset));
    }

    private IEnumerator MoveHoopCoroutine(float targetOffset)
    {
        isHoopMoving = true;

        yield return new WaitForSeconds(hoopMoveDelay);

        explosionOnFadeOut.Show();
        StartCoroutine(poleFading.FadeCoroutine(false, false));
        StartCoroutine(netFading.FadeCoroutine(false, false));
        StartCoroutine(ringHolderFading.FadeCoroutine(false, false));
        StartCoroutine(ringFading.FadeCoroutine(false, false));
        yield return StartCoroutine(backboardFading.FadeCoroutine(false, false));

        hoop.SetActive(false);
        yield return new WaitForFixedUpdate();

        hoop.transform.Translate(0f, 0f, targetOffset - currentHoopZOffset);
        currentHoopZOffset = targetOffset;
        RestoreHoopPhysicsPoses();

        yield return new WaitForFixedUpdate();
        hoop.SetActive(true);
        ZeroHoopPhysicsVelocities();
        yield return new WaitForFixedUpdate();

        explosionOnFadeIn.Show();
        StartCoroutine(poleFading.FadeCoroutine(true, true));
        StartCoroutine(netFading.FadeCoroutine(true, true));
        StartCoroutine(ringHolderFading.FadeCoroutine(true, true));
        StartCoroutine(ringFading.FadeCoroutine(true, true));
        yield return StartCoroutine(backboardFading.FadeCoroutine(true, true));

        isHoopMoving = false;
    }

    private void ResetHoopPosition()
    {
        if (!hoop) return;

        if (!Mathf.Approximately(currentHoopZOffset, 0f))
        {
            hoop.transform.Translate(0f, 0f, -currentHoopZOffset);
            currentHoopZOffset = 0f;
        }

        RestoreHoopPhysicsPoses();
        hoop.SetActive(true);
        ZeroHoopPhysicsVelocities();

        StartCoroutine(poleFading.FadeCoroutine(true, true));
        StartCoroutine(netFading.FadeCoroutine(true, true));
        StartCoroutine(ringHolderFading.FadeCoroutine(true, true));
        StartCoroutine(ringFading.FadeCoroutine(true, true));
        StartCoroutine(backboardFading.FadeCoroutine(true, true));
    }
}
