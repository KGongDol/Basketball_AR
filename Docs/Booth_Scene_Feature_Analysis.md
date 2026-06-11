# 부스 행사용 씬 — Demo_BasketballGame_Click 기능 분석

> 대상 기기: iPad 11 (ARKit 지원 확정)
> 목적: 부스 행사용 새 씬 제작 시 Demo_BasketballGame_Click에서 가져올 필수 기능 정리
> 작성일: 2026-06-10

---

## 1. 필수 — 반드시 가져와야 하는 기능

### 1-1. AR 인프라 (AR 모드로 운영하는 경우)

| 오브젝트/컴포넌트 | 스크립트 | 역할 |
|---|---|---|
| `AR Session` | `ARSession`, `ARInputManager` | AR 세션 수명주기 |
| `XR Origin` + `Camera Offset` + `Main Camera` | `XROrigin`, `ARCameraManager`, `ARCameraBackground`, `TrackedPoseDriver` | AR 카메라 트래킹/배경 렌더링 |
| 평면 인식 | `ARPlaneManager`, `ARRaycastManager` | 바닥 인식 → 골대 배치 |
| 평면 인식 제어 | `ARPlaneDetectionControl` (`Publisher/AR/ARFoundationBase/PlaneDetection`) | 평면 탐지 → GameWorld 배치 흐름 제어 |
| 플레이어 제어 | `ARPlayerControl` | AR 카메라에 플레이어(던지는 위치) 부착. `BasketballGameControl.InitGameForARFoundationWithCameraCoroutine()`이 `ARPlayerControl.Current`를 직접 참조하므로 **없으면 AR 초기화가 깨짐** |
| 온보딩 UI | `UIManager` (`Publisher/AR/.../Onboarding`) | "바닥을 비춰주세요" 류 안내 UI |
| 조명 추정 | `LightEstimationControl` | 실제 조명에 맞춰 씬 라이팅 보정 — 부스 조명 환경에서 품질 향상 |
| `ARKitCoachingOverlay` | iOS 전용 코칭 오버레이 | iPad 운영이므로 유지 권장 |

### 1-2. 게임 코어 (모드 무관 필수)

| 기능 | 오브젝트/스크립트 | 비고 |
|---|---|---|
| 게임 총괄 | `BasketballGameControl` | 점수 계산, 골/실패 이벤트, 게임 초기화의 허브. 모든 필수 참조의 중심 |
| 골대 전체 | `Hoop` 계층: `Backboard`, `Pole`, `Ring`, `RingHolder`, `RingColliders`(Collider 1~19), `Net` | `BasketballHoopControl`, `BasketballRingControl` |
| 득점/실패 판정 | `NetTrigger`, `RingTrigger`, `FailZones`(Front/Back/Left/Right) | `BasketballBallControl`의 `OnGoal`/`OnFail` static 이벤트로 `BasketballGameControl`에 전달 |
| 공 던지기 | `Player` + `ThrowControl` (ThrowControlX), `InputSystem`(`PlayerInput`) | 클릭/탭 던지기 — 씬 이름의 "Click". AR용 공 프리팹(`throwingPrefabForAR`) 참조 포함 |
| 공 풀링 | `ThrowingPoolControl` (`RandomObjectPooler`) | 공 재사용. `ThrowControl`과 연동 |
| 방향 화살표 | `ArrowDirectionalControl` | **선택처럼 보이지만 필수**: `BasketballGameControl.StartGame()`이 직접 참조 (null 체크 없음). 빼려면 코드 수정 필요 |
| 오디오 | `AudioController` + `BasketballAudioControl` | 싱글톤(`BasketballAudioControl.Instance`)으로 골/실패마다 호출됨 — 없으면 NullReference |
| 점수 | `ScoreCurrentControl`, `ScoreBestControl`, `PopupTextControl` ×2 (Score / Score Clear) | `BasketballGameControl`이 모두 직접 참조. ScoreBest는 PlayerPrefs 저장 — 부스용 수정 필요 (아래 4장 참고) |
| 캐싱 | `CacheControl` + `Caching (To Avoid Freezes on 1st Appearing)` 오브젝트들 | 첫 골/폭발 연출 시 프리징 방지용 프리워밍. 유지 권장 |
| UI 기본 | `EventSystem`(`InputSystemUIInputModule`), `CanvasStart`(시작 버튼+튜토리얼), `CanvasesHUD`(점수 표시), `CanvasPopupTexts`(+점수 팝업) | |
| 월드 루트 | `GameWorldParent` → `GameWorld (just to set Pivot Point)` | AR 배치/스케일링 기준점. 계층 구조 유지 필수 |

---

## 2. 조건부 — 부스 운영 방식에 따라 결정

| 기능 | 구성 | 판단 기준 |
|---|---|---|
| AR 지원 체크 + 모드 선택 | `AR Foundation Support Checker - Vertical`(`ARFoundationSupportChecker`), `CanvasSelectMode`, `Button StartAR/StartNonAR` | iPad 11은 AR 지원 확정 → **제거 가능**. 대신 시작 시 `BasketballGameControl.StartAR()`를 바로 호출하도록 변경. 단, 제거 시 Non-AR 폴백도 사라지므로 리허설에서 AR 동작 검증 필수 |
| Non-AR 모드 경로 | `InitGameForNonAR()`, Gyro/Accelerometer 튜토리얼 텍스트, `RotationByMouseControl`, `RotationByKeysControl`, `BreathControl` | AR 전용으로 운영하면 **제거**. 데스크톱 테스트용 마우스/키보드 회전은 부스에서 불필요 |
| SafeZone | `SafeZone`, `PlayerSafeZone`, `CanvasPauseBySafeZone` | 플레이어가 골대에 너무 접근하면 일시정지시키는 기능. 부스 공간이 좁고 관람객 통제가 안 되면 **유지가 오히려 유리**, 운영 인력이 통제하면 제거해 단순화 |
| 골대 이동 (`Goal - Hoop Movement`) | 연속 골 시 골대가 회전 이동 | 짧은 체험(1인당 30초~1분)에는 과한 난이도 → 인스펙터에서 `hoopMovementComboAimOfGoals = 0`으로 비활성 가능 (오브젝트 삭제 불필요) |
| 빅 링 보너스 (`Goal - Set Big Ring`, `Net Big`) | 클린 골 시 링이 커지는 보상 | 체험용으로 재미 요소 — 유지 권장. 끄려면 `bigRingComboAimOfGoalsClear = 0` |
| 거리 비례 점수 | `isPointsDistanceToBasketOn` | 부스에선 점수 체계가 단순한 게 좋음 — 끄는 것 고려 |
| 폭발 연출 | `ExplosionControl`, `ExplosionPivot`, `MaterialControl` | 시각 효과 — 유지 권장 (체험 만족도). 빼면 Caching 오브젝트도 같이 정리 |
| 포인트 클라우드 | `ARPointCloudManager` | 평면 인식 중 시각 피드백. 온보딩에 도움되므로 유지 권장 |

---

## 3. 제거 — 부스용에 불필요

| 기능 | 이유 |
|---|---|
| `SceneControl` (LoadScreen 경유 씬 전환) | 부스용은 단일 씬 운영. 메뉴 씬으로 돌아갈 일 없음 |
| 키보드 단축키 (`Key Escape/R/Space` 패턴) | 터치 전용 기기 |
| `PanelPause` 의 수동 일시정지/종료 버튼 | 관람객이 누르면 운영 사고. SafeZone 일시정지를 쓸 경우 해당 캔버스만 유지 |
| Makaka 로고/에셋스토어 링크 버튼 | 행사 브랜딩과 무관 |
| `ButtonLoadingAnimationControl` | 씬 전환이 없으면 불필요 |
| `Bat` 오브젝트 | 데모 연출용으로 추정 — 역할 확인 후 제거 (씬에서 용도 불명) |

---

## 4. 부스용으로 수정이 필요한 기존 기능

1. **ScoreBestControl (최고 점수)** — PlayerPrefs에 영구 저장됨. 부스에서는:
   - 사람이 바뀔 때마다 현재 점수 리셋 필요 → "다시 시작" 플로우 추가
   - 베스트 스코어를 "오늘의 최고 기록"으로 쓸 거면 유지, 아니면 비표시
2. **시작 플로우 단순화** — `CanvasSelectMode → CanvasStart → 게임` 2단계를 `CanvasStart → 게임` 1단계로. `Start()`에서 `canvasSelectMode.SetActive(true)` 대신 바로 `StartAR()` 호출
3. **튜토리얼 텍스트** — 행사 관람객 대상 문구로 교체 (한국어, 큰 글씨)
4. **`Time.timeScale = 1.5f`** — `BasketballGameControl.Awake()`에서 게임 속도를 1.5배로 설정 중. 체험 난이도에 영향 — 부스에서 의도한 값인지 확인

---

## 5. 부스용 신규 추가 권장 기능 (씬에 없음)

| 기능 | 이유 |
|---|---|
| **유휴 자동 리셋** | 일정 시간 입력 없으면 점수/상태 초기화 + 시작 화면 복귀 (다음 관람객 대비) |
| **제한 시간 or 공 개수 모드** | 1인당 체험 시간 통제 (예: 60초 or 공 10개) → 회전율 확보 |
| **운영자용 숨김 리셋 제스처** | 화면 구석 길게 누르기 등으로 즉시 리셋/골대 재배치 |
| **골대 재배치 버튼** | AR 앵커가 틀어졌을 때 재스캔 없이 빠른 복구 |
| `Screen.sleepTimeout = NeverSleep` | 행사 중 화면 꺼짐 방지 |
| iOS Guided Access(사용법 안내) | 관람객의 홈 이동/앱 종료 방지 — 앱 기능은 아니지만 운영 체크리스트에 포함 |

---

## 적용 내역 (2026-06-10, Assets/01.Scenes/BasketballGame.unity)

원본 이벤트 체인 분석 결과, "Start AR 버튼"의 실제 역할은 평면 인식 오브젝트를 `SetActive(true)` 하는 것뿐이었다.
평면 인식 오브젝트(`ARPlaneDetectionControl`)는 활성화되면 스스로 `OnStarted → StartAR()` 호출 + XR Origin/AR Session 활성화까지 처리한다.
따라서 **삭제 대신 "비활성화 + 부트스트랩" 방식**으로 원본 체인을 100% 보존하면서 부스 플로우를 구현했다.

| 변경 | 내용 |
|---|---|
| 신규 `Assets/02.Scripts/BoothStartControl.cs` | `[DefaultExecutionOrder(32000)]` 부트스트랩. Start()에서 ① 화면 꺼짐 방지 ② 60fps ③ CanvasSelectMode 숨김 ④ 평면 인식 오브젝트 활성화(= Start AR 버튼 클릭과 동일) |
| 씬: `BoothStartControl` 루트 오브젝트 추가 | 위 스크립트 + 참조 연결 (canvasSelectMode, arPlaneDetection) |
| 씬: `BasketballGameControl` `m_Enabled: 0 → 1` | 원래는 센서 카메라 init이 런타임에 활성화했음. 부트스트랩과의 실행 순서 경쟁을 없애기 위해 처음부터 활성 |
| 씬: `AR Foundation Support Checker - Vertical` 비활성화 | iPad 11은 AR 확정 지원 — 체크 불필요. 삭제하지 않은 이유: Button StartAR가 참조 중 (참조 깨짐 방지, 쉬운 롤백) |
| 씬: `canvasStartTextTutorialAR` 한국어 교체 | "화면을 탭해서 공을 던져 골대에 넣어보세요!..." |

**의도적으로 유지한 것 (분석 문서와 다른 결정):**
- **센서 카메라(비-AR 폴백) 오브젝트**: 삭제하지 않음. ① AR 시작 전 유일한 렌더링 카메라 ② `OnARStarted`가 원래 자동으로 꺼줌 ③ 삭제 시 `ThrowControl.cameraMain` 등 참조가 깨질 위험. 비-AR "선택지"는 CanvasSelectMode 스킵으로 이미 차단됨
- **CanvasSelectMode 오브젝트**: `BasketballGameControl`이 null 체크 없이 참조하므로 유지하되 부트스트랩이 숨김
- **SafeZone 일시정지**: 부스 관람객 통제용으로 유지
- **`Time.timeScale = 1.5`**: 원본 데모와 동일하게 유지 (난이도 변경 원하면 BasketballGameControl 인스펙터에서 조정)

**추가 적용 (2026-06-10, Unity MCP로 에디터에서 직접 작업):**
- 던지기 방식: 클릭/탭 → **드래그(Flick)** 전환 + 드래그 궤적 라인렌더러 표시 (`BoothDragLineControl`, 발사 후 2초 유지)
- **부스 미션 시스템** (`BoothMissionControl` + `CanvasBoothMission`): 공 5개/40초, 첫 던지기에 자동 시작, 등급 테이블 인스펙터 편집 가능(현재 placeholder: 4골=1등상/2골=2등상/1골=3등상/0골=참가상), 골마다 골대가 카메라 정면 방향으로 +2/+4/+6/+8m 멀어짐(회전 없음), 결과 화면 탭 또는 15초 후 자동 리셋
- 기존 골대 랜덤 이동(`hoopMovementComboAimOfGoals=0`)·빅 링(`bigRingComboAimOfGoalsClear=0`) 비활성화 — 미션 공정성 확보

**미적용 (다음 단계):**
- ScoreBest 처리 (미션 모드에서는 골 수 기준이라 점수 캔버스 자체를 숨길지 결정 필요)
- 운영자용 숨김 리셋 제스처 / 골대 재배치 버튼

**테스트 시 주의:** 에디터에서는 XR Simulation을 켜야 평면 인식 테스트 가능. 실기기(iPad) 테스트 필수.

---

## 6. 의존성 주의사항 (새 씬 구성 시)

- `BasketballGameControl`은 다음을 **null 체크 없이 직접 참조**: `ThrowControl`, `BasketballHoopControl`, `ArrowDirectionalControl`, `ScoreCurrentControl`, `ScoreBestControl`, `PopupTextControl` ×2, `canvasSelectMode`, `canvasStart`, `canvasPause` → 하나라도 빠지면 NullReference. UI를 빼고 싶으면 코드 수정이 같이 필요
- `BasketballBallControl.OnGoal/OnFail`은 **static 이벤트** → 공 프리팹에 `BasketballBallControl`이 붙어 있어야 점수가 동작
- `BasketballAudioControl`은 **싱글톤** (`Instance`) → 씬에 1개 필수
- AR 초기화 순서: `StartAR()` → 평면 인식 → `InitGameForARFoundationWithCamera()` → `OnARFoundationGameWorldInitialization` → `OnARFoundationGameWorldScaling` → `InitThrowing()` → `OnInitialized` — 이 UnityEvent 체인이 인스펙터에 연결되어 있으므로 **씬 복제 후 이벤트 연결 확인 필수**
- 가장 안전한 제작 방법: **Demo 씬을 복제 → 불필요 요소 제거** 방식 (빈 씬에서 새로 조립하는 것보다 인스펙터 이벤트 연결 누락 위험이 훨씬 적음)
