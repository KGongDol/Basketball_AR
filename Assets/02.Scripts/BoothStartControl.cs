using UnityEngine;

/// <summary>
/// 부스(키오스크) 모드 부트스트랩.
/// 원본 데모의 AR/Non-AR 선택 화면을 건너뛰고 바로 AR 평면 인식을 시작한다.
/// 평면 인식 오브젝트가 활성화되면 ARPlaneDetectionControl.Start()가
/// OnStarted → BasketballGameControl.StartAR()를 호출하므로
/// 원본 이벤트 체인은 그대로 유지된다.
/// </summary>
// 다른 모든 Start()(특히 BasketballGameControl.Start의 선택 화면 활성화) 이후에
// 실행되어야 하므로 실행 순서를 뒤로 미룬다.
[DefaultExecutionOrder(32000)]
public class BoothStartControl : MonoBehaviour
{
    [Tooltip("원본 데모의 AR/Non-AR 선택 캔버스 (CanvasSelectMode)")]
    [SerializeField] private GameObject canvasSelectMode;

    [Tooltip("AR Foundation - Plane Detection 프리팹 인스턴스 (비활성 상태로 시작)")]
    [SerializeField] private GameObject arPlaneDetection;

    private void Start()
    {
        // 부스 운영 중 화면 꺼짐 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        // 모드 선택 화면을 건너뛰고 바로 AR 시작
        canvasSelectMode.SetActive(false);
        arPlaneDetection.SetActive(true);
    }
}
