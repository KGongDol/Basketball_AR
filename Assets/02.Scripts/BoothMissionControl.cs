using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

using MakakaGames.Basketball;
using MakakaGames.ThrowControlX;
using MakakaGames.Publisher.Scoring;
using MakakaGames.Publisher.MaterialControlX;
using MakakaGames.Publisher.ExplosionWithSmoke;

/// <summary>
/// 부스 미션 모드: 공 N개·제한시간 내에 골을 넣으면 등급별 상품.
/// - 첫 던지기에 라운드 자동 시작 (관람객 셀프 진행)
/// - 골마다 골대가 카메라 정면 방향으로 점점 멀어짐 (회전 없음)
/// - 라운드 종료 시 등급 결과 표시 → 탭 또는 자동으로 다음 손님 대기
/// 등급/거리/시간은 모두 인스펙터에서 수정 가능.
/// </summary>
public class BoothMissionControl : MonoBehaviour
{
    private enum State { Ready, Playing, Result }

    [Header("라운드 설정")]
    [SerializeField] private int ballsPerRound = 5;

    [Tooltip("라운드 총 제한시간(초, 실시간 기준)")]
    [SerializeField] private float roundDuration = 40f;

    [Tooltip("마지막 공을 던진 후 골/실패 판정을 기다리는 최대 시간(초)")]
    [SerializeField] private float lastBallGraceTime = 4f;

    [Header("등급 (minGoals가 큰 항목부터 검사하므로 내림차순 권장)")]
    [SerializeField]
    private List<Grade> grades = new()
    {
        new Grade { minGoals = 4, label = "1등상" },
        new Grade { minGoals = 2, label = "2등상" },
        new Grade { minGoals = 1, label = "3등상" },
        new Grade { minGoals = 0, label = "참가상" },
    };

    [Serializable]
    public class Grade
    {
        public int minGoals;
        public string label;
    }

    [Header("골대 거리 (k번째 골 이후, 시작 위치 기준 로컬 Z 오프셋)")]
    [Tooltip("배열 길이를 넘어서는 골은 마지막 값을 유지")]
    [SerializeField] private float[] hoopZOffsetPerGoal = { 2f, 4f, 6f, 8f };

    [Header("게임 참조")]
    [SerializeField] private ThrowControl throwControl;
    [SerializeField] private ScoreCurrentControl scoreCurrentControl;

    [Header("골대 참조 (HoopPivot의 BasketballHoopControl과 동일 대상)")]
    [SerializeField] private GameObject hoop;
    [SerializeField] private MaterialControl backboardFading;
    [SerializeField] private MaterialControl poleFading;
    [SerializeField] private MaterialControl netFading;
    [SerializeField] private MaterialControl ringHolderFading;
    [SerializeField] private MaterialControl ringFading;
    [SerializeField] private ExplosionControl explosionOnFadeOut;
    [SerializeField] private ExplosionControl explosionOnFadeIn;

    [Tooltip("골 직후 골대 이동 연출 시작까지의 지연(초)")]
    [SerializeField] private float hoopMoveDelay = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Tooltip("결과 화면이 자동으로 닫히기까지의 시간(초)")]
    [SerializeField] private float resultAutoResetDelay = 15f;

    [Tooltip("결과 화면 표시 직후 탭 무시 시간(초) — 던지기 손떼기 오탭 방지")]
    [SerializeField] private float resultTapIgnoreTime = 1f;

    private State state = State.Ready;

    // 골대 하위 물리 오브젝트(Ring의 Rigidbody+HingeJoint, RingTrigger 등)는
    // 텔레포트 시 월드 앵커 기준으로 재정렬되며 어긋나므로,
    // 이동 시마다 시작 시점의 로컬 포즈로 강제 복원한다.
    private struct HoopPhysicsPose
    {
        public Transform transform;
        public Rigidbody rigidbody;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private readonly List<HoopPhysicsPose> hoopPhysicsPoses = new();

    // 월드 고정점(connectedBody == null)에 연결된 조인트들.
    // autoConfigure 재계산에 맡기면 재활성화 타이밍에 따라 림이 홉 돌아가므로
    // 이동 시마다 앵커를 직접 새 위치로 고정한다.
    private readonly List<Joint> hoopWorldJoints = new();

    private int ballsThrown;
    private int ballsResolved;
    private int goals;
    private float timeLeft;
    private float resultShownAt;
    private float currentHoopZOffset;
    private bool isHoopMoving;

    private void Awake()
    {
        // 등급은 minGoals 내림차순으로 평가
        grades.Sort((a, b) => b.minGoals.CompareTo(a.minGoals));
    }

    private void OnEnable()
    {
        BasketballBallControl.OnGoal += HandleGoal;
        BasketballBallControl.OnFail += HandleFail;

        if (throwControl)
        {
            throwControl.OnThrow.AddListener(HandleThrow);
        }
    }

    private void OnDisable()
    {
        BasketballBallControl.OnGoal -= HandleGoal;
        BasketballBallControl.OnFail -= HandleFail;

        if (throwControl)
        {
            throwControl.OnThrow.RemoveListener(HandleThrow);
        }
    }

    private void Start()
    {
        CaptureHoopPhysicsPoses();

        ShowReady();
    }

    private void CaptureHoopPhysicsPoses()
    {
        if (!hoop)
        {
            return;
        }

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
            {
                hoopWorldJoints.Add(joint);
            }
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

        // 복원된 포즈 기준으로 월드 앵커를 재고정
        foreach (Joint joint in hoopWorldJoints)
        {
            if (joint)
            {
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor =
                    joint.transform.TransformPoint(joint.anchor);
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
        // 테스트용 치트: 스페이스바 = 골 처리 (에디터 전용, 빌드에는 미포함)
        if (Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (state == State.Ready)
            {
                StartRound();
            }

            if (state == State.Playing)
            {
                BasketballBallControl.OnGoal?.Invoke(false);
            }
        }
#endif

        if (state == State.Playing)
        {
            timeLeft -= Time.unscaledDeltaTime;

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;

                EndRound();
            }
            else
            {
                UpdateMissionHud();
            }
        }
        else if (state == State.Result)
        {
            bool isTapped = Pointer.current != null
                && Pointer.current.press.wasPressedThisFrame
                && Time.unscaledTime - resultShownAt > resultTapIgnoreTime;

            bool isTimeout =
                Time.unscaledTime - resultShownAt > resultAutoResetDelay;

            if (isTapped || isTimeout)
            {
                ResetRound();
            }
        }
    }

    private void HandleThrow(ThrowingObject throwingObject)
    {
        if (state == State.Ready)
        {
            StartRound();
        }

        if (state != State.Playing)
        {
            return;
        }

        ballsThrown++;

        UpdateMissionHud();

        if (ballsThrown >= ballsPerRound)
        {
            // 추가 던지기 차단 (현재 날아가는 공의 물리/판정은 계속 동작)
            throwControl.enabled = false;

            StartCoroutine(EndAfterLastBallCoroutine());
        }
    }

    private void HandleGoal(bool isClearBall)
    {
        if (state != State.Playing)
        {
            return;
        }

        goals++;
        ballsResolved++;

        UpdateMissionHud();

        MoveHoopForGoalCount(goals);
    }

    private void HandleFail()
    {
        if (state != State.Playing)
        {
            return;
        }

        ballsResolved++;
    }

    private void StartRound()
    {
        state = State.Playing;

        ballsThrown = 0;
        ballsResolved = 0;
        goals = 0;
        timeLeft = roundDuration;

        UpdateMissionHud();
    }

    private IEnumerator EndAfterLastBallCoroutine()
    {
        float elapsed = 0f;

        while (state == State.Playing
            && ballsResolved < ballsThrown
            && elapsed < lastBallGraceTime
            && timeLeft > 0f)
        {
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (state == State.Playing)
        {
            EndRound();
        }
    }

    private void EndRound()
    {
        state = State.Result;

        throwControl.enabled = false;

        resultShownAt = Time.unscaledTime;

        if (resultText)
        {
            resultText.text = $"골 {goals}개!\n<size=70%>{GetGradeLabel(goals)}</size>";
        }

        if (resultPanel)
        {
            resultPanel.SetActive(true);
        }
    }

    private void ResetRound()
    {
        StopAllCoroutines();

        isHoopMoving = false;

        ResetHoopPosition();

        if (scoreCurrentControl)
        {
            scoreCurrentControl.Reset();
        }

        if (resultPanel)
        {
            resultPanel.SetActive(false);
        }

        throwControl.enabled = true;

        state = State.Ready;

        ShowReady();
    }

    private string GetGradeLabel(int goalCount)
    {
        for (int i = 0; i < grades.Count; i++)
        {
            if (goalCount >= grades[i].minGoals)
            {
                return grades[i].label;
            }
        }

        return string.Empty;
    }

    private void ShowReady()
    {
        if (missionText)
        {
            missionText.text =
                $"공을 던지면 시작!  (공 {ballsPerRound}개 / {Mathf.RoundToInt(roundDuration)}초)";
        }
    }

    private void UpdateMissionHud()
    {
        if (missionText)
        {
            missionText.text = $"공 {ballsPerRound - ballsThrown}  |  "
                + $"남은 시간 {Mathf.CeilToInt(timeLeft)}  |  골 {goals}";
        }
    }

    private void MoveHoopForGoalCount(int goalCount)
    {
        if (hoopZOffsetPerGoal == null || hoopZOffsetPerGoal.Length == 0
            || isHoopMoving || !hoop)
        {
            return;
        }

        int index = Mathf.Min(goalCount - 1, hoopZOffsetPerGoal.Length - 1);

        float targetOffset = hoopZOffsetPerGoal[index];

        if (Mathf.Approximately(targetOffset, currentHoopZOffset))
        {
            return;
        }

        StartCoroutine(MoveHoopCoroutine(targetOffset));
    }

    /// <summary>
    /// BasketballHoopControl.RotateAroundCoroutine의 페이드 연출을 그대로 따르되,
    /// 회전·랜덤 Z 대신 지정된 Z 오프셋으로 직진 이동만 한다 (카메라 정면 유지).
    /// </summary>
    private IEnumerator MoveHoopCoroutine(float targetOffset)
    {
        isHoopMoving = true;

        yield return new WaitForSeconds(hoopMoveDelay);

        explosionOnFadeOut.Show();

        StartCoroutine(poleFading.FadeCoroutine(false, false));
        StartCoroutine(netFading.FadeCoroutine(false, false));
        StartCoroutine(ringHolderFading.FadeCoroutine(false, false));
        StartCoroutine(ringFading.FadeCoroutine(false, false));

        yield return StartCoroutine(
            backboardFading.FadeCoroutine(false, false));

        hoop.SetActive(false);

        yield return new WaitForFixedUpdate();

        hoop.transform.Translate(0f, 0f, targetOffset - currentHoopZOffset);

        currentHoopZOffset = targetOffset;

        // 림(HingeJoint)·트리거가 옛 월드 위치에 남지 않도록 로컬 포즈 복원
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

        yield return StartCoroutine(
            backboardFading.FadeCoroutine(true, true));

        isHoopMoving = false;
    }

    private void ResetHoopPosition()
    {
        if (!hoop)
        {
            return;
        }

        if (!Mathf.Approximately(currentHoopZOffset, 0f))
        {
            hoop.transform.Translate(0f, 0f, -currentHoopZOffset);

            currentHoopZOffset = 0f;
        }

        RestoreHoopPhysicsPoses();

        // 이동 연출 도중 리셋된 경우를 대비한 복구
        hoop.SetActive(true);

        ZeroHoopPhysicsVelocities();

        StartCoroutine(poleFading.FadeCoroutine(true, true));
        StartCoroutine(netFading.FadeCoroutine(true, true));
        StartCoroutine(ringHolderFading.FadeCoroutine(true, true));
        StartCoroutine(ringFading.FadeCoroutine(true, true));
        StartCoroutine(backboardFading.FadeCoroutine(true, true));
    }
}
