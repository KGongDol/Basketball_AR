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

using System.Collections;

using TMPro;

using MakakaGames.ThrowControlX;
using MakakaGames.Publisher.Scoring;
using MakakaGames.Publisher.ArrowDirectional;
using MakakaGames.Publisher.AR.ARFoundationBase.PlaneDetection;

#pragma warning disable 649

namespace MakakaGames.Basketball
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class BasketballGameControl : MonoBehaviour 
    {
        [SerializeField]
        private ThrowControl throwControl;

        [SerializeField]
        private GameObject throwingPrefabForAR;

        [SerializeField]
        private float timeScale = 1.5f;

        private bool isFirstStart = true;

        [Header("Points")]
        [SerializeField]
        private float pointsGoalNormal = 2f;
        
        [SerializeField]
        private float pointsGoalClear = 3f;

        [SerializeField]
        private bool isPointsComboOn = true;

        private int pointsCombo;

        [Header("Points - Distance To Basket")]
        [SerializeField]
        private bool isPointsDistanceToBasketOn = true;
        
        [SerializeField]
        private float pointsDistanceToBasketFactor = 0.2f;
        private float distanceToBasket;

        [Header("Clear Ball = Big Ring")]
        [SerializeField]
        private int bigRingComboAimOfGoalsClear = 1;

        [Tooltip("Must be bigger than Combo Aim")]
        [SerializeField]
        private int bigRingLimitOfGoalsAny = 1;
        
        private int bigRingCurrentGoalsAnyCount;
        private int bigRingComboOfCurrentGoalsClear;
        private bool isBigRing = false;

        [Header("Hoop Movement (if Normal Goal & Normal Ring)")]
        [SerializeField]
        private BasketballHoopControl basketballHoopControl;

        [SerializeField]
        private Transform hoopMovementPivot;
        
        [SerializeField]
        private int hoopMovementComboAimOfGoals = 2;
        
        private int hoopMovementComboOfCurrentGoals;

        [Header("UI")]
        [SerializeField]
        private GameObject canvasSelectMode;

        [SerializeField]
        private Canvas canvasStart;

        [SerializeField]
        private TextMeshProUGUI canvasStartTextTutorial;

        [SerializeField]
        [TextArea(3, 4)]
        private string canvasStartTextTutorialAR;

        [SerializeField]
        [TextArea(3, 4)]
        private string canvasStartTextTutorialGyro;

        [SerializeField]
        [TextArea(3, 4)]
        private string canvasStartTextTutorialAccelerometer;

        [Space]
        [SerializeField]
        private Canvas canvasPause;

        [Space]
        [SerializeField]
        private ArrowDirectionalControl arrowDirectionalControl;

        [SerializeField]
        private ScoreBestControl scoreBestControl;

        [SerializeField]
        private ScoreCurrentControl scoreCurrentControl;

        [SerializeField]
        private PopupTextControl textPopupScore;
        
        [SerializeField]
        private PopupTextControl textPopupScoreClear;

        [Header("Events")]
        [Space]
        [SerializeField]
        private UnityEvent OnUnityStart;

        [Space]
        [SerializeField]
        private UnityEvent OnARStarted;

        [Space]
        [SerializeField]
        private UnityEvent OnARFoundationGameWorldInitialization;

        [Space]
        [SerializeField]
        private UnityEvent OnARFoundationGameWorldScaling;

        [Space]
        [SerializeField]
        private UnityEvent OnInitialized;

        private void Awake() 
        {
            Time.timeScale = timeScale;
        }
        
        private void OnEnable()
        {
            BasketballBallControl.OnGoal += Goal;
            BasketballBallControl.OnFail += Fail;
        }

        private void OnDisable()
        {
            BasketballBallControl.OnGoal -= Goal;
            BasketballBallControl.OnFail -= Fail;
        }
        
        private void Start()
        {
            canvasSelectMode.SetActive(true);
            
            OnUnityStart.Invoke();
        }

        public void InitTutorialForGyro()
        {
            canvasStartTextTutorial.text = canvasStartTextTutorialGyro;
        }

        public void InitTutorialForAccelerometer()
        {
            canvasStartTextTutorial.text = canvasStartTextTutorialAccelerometer;
        }

        public void InitGameForNonAR()
        {
            canvasSelectMode.SetActive(false);

            canvasStart.gameObject.SetActive(true);

            InitThrowing(null);
        }

        public void StartAR()
        {
            OnARStarted?.Invoke();

            canvasSelectMode.SetActive(false);
        }

        public void InitGameForARFoundationWithCamera(Transform camera)
        {
            canvasStart.gameObject.SetActive(true);

            canvasStartTextTutorial.text = canvasStartTextTutorialAR;

            StartCoroutine(
                InitGameForARFoundationWithCameraCoroutine(camera));
        }

        private IEnumerator InitGameForARFoundationWithCameraCoroutine(
            Transform camera)
        {
            Vector3 playerLocalPosLast =
                ARPlayerControl.Current.transform.localPosition;

            Quaternion playerLocalRotLast =
                ARPlayerControl.Current.transform.localRotation;

            ARPlayerControl.Current.transform.parent = camera;
            ARPlayerControl.Current.transform.localPosition = playerLocalPosLast;
            ARPlayerControl.Current.transform.localRotation = playerLocalRotLast;

            yield return null;

            OnARFoundationGameWorldInitialization.Invoke();

            yield return null;

            OnARFoundationGameWorldScaling.Invoke();

            yield return null;

            InitThrowing(camera.GetComponent<Camera>());
        }

        private void InitThrowing(Camera camera)
        {
            StartCoroutine(InitThrowingCoroutine(camera));
        }

        private IEnumerator InitThrowingCoroutine(Camera camera)
        {
            if (camera)
            {
                throwControl.cameraMain = camera;
                throwControl.SetPrefabBeforeInit(
                    throwingPrefabForAR);
            }

            yield return null;

            throwControl.OnInitialized.AddListener(InitGame);
            throwControl.gameObject.SetActive(true);
        }

        private void InitGame()
        {
            InitNetAndDistanceToBasket();

            OnInitialized.Invoke();
        }

        /// <summary>
        /// It's used On Click For "Start" Button.
        /// </summary>
        public void StartGame()
        {
            canvasStart.gameObject.SetActive(false);

            if (isFirstStart)
            {
                isFirstStart = false;
            }

            throwControl.GetFirstThrow();

            //to bind camera when AR version
            arrowDirectionalControl.cameraMain = throwControl.cameraMain.transform;
        }

        public void PauseGameWhenPlayerLeftSafeZone()
        {
            canvasPause.gameObject.SetActive(true);

            if (isFirstStart)
            {
                canvasStart.gameObject.SetActive(false);
            }
        }

        public void PlayGameWhenPlayerEnteredSafeZone()
        {
            canvasPause.gameObject.SetActive(false);

            if (isFirstStart)
            {
                canvasStart.gameObject.SetActive(true);
            }
        }

        private void InitNetAndDistanceToBasket()
        {
            InitBasketballNet(throwControl.GetObjectCount());

            throwControl.OnNextThrowGetting.AddListener(
                (throwingObject) => 
                {
                    RegisterSphereCollidersOfCurrentBallForNet(throwingObject); 
                    CalculateDistanceToBasket(throwingObject);
                });
            
            throwControl.OnThrow.AddListener(
                (throwingObject) => 
                {
                    AnnulSphereCollidersOfCurrentBallForNet(
                        throwingObject, throwControl.resetDelay - 0.1f);
                });
        }

        private void CalculateDistanceToBasket(ThrowingObject throwingObject)
        {
            distanceToBasket = 
                (throwingObject.transform.position
                    - basketballHoopControl.GetRingPosition()).magnitude;
        }

        private void InitBasketballNet(int countOfThrowingObjects)
        {
            basketballHoopControl.InitBasketballNet(countOfThrowingObjects);
        }

        private void RegisterSphereCollidersOfCurrentBallForNet(
            ThrowingObject throwingObject)
        {
            BasketballBallControl basketballBallControlTemp =
                BasketballBallControl.GetComponent(throwingObject);

            if (basketballBallControlTemp)
            { 
                basketballHoopControl.RegisterSphereColliderForNet(
                    basketballBallControlTemp.sphereCollider);
            }
        }

        private void AnnulSphereCollidersOfCurrentBallForNet(
            ThrowingObject throwingObject, float delay)
        {
            StartCoroutine(AnnulSphereCollidersOfCurrentBallForNetCoroutine(
                throwingObject, delay));
        }

        private IEnumerator AnnulSphereCollidersOfCurrentBallForNetCoroutine(
            ThrowingObject throwingObject, float delay)
        {
            yield return new WaitForSeconds(delay);

            BasketballBallControl basketballBallControlTemp =
                BasketballBallControl.GetComponent(throwingObject);

            if (basketballBallControlTemp)
            { 
                basketballHoopControl.AnnulSphereColliderForNet(
                    basketballBallControlTemp.sphereCollider);
            }
        }
        
        private void Goal(bool isClearBall)
        {
            float pointsGoalCurrent;

            if (isClearBall)
            {
                CheckBigRingBonus();

                BasketballAudioControl.Instance.PlayGoalClear();

                textPopupScoreClear.ResetText();

                pointsGoalCurrent = pointsGoalClear;
            }
            else
            {
                BasketballAudioControl.Instance.PlayGoalNormal();

                pointsGoalCurrent = pointsGoalNormal;
            }

            CheckBigRingReset();

            if (isPointsDistanceToBasketOn)
            {
                pointsGoalCurrent *=
                    distanceToBasket * pointsDistanceToBasketFactor;
            }

            if (!isPointsComboOn)
            {
                pointsCombo = 0;
            }

            pointsCombo += (int)pointsGoalCurrent;

            textPopupScore.SetText("+" + pointsCombo);
            textPopupScore.ResetText();

            AddScore(pointsCombo);
        }

        private void Fail()
        {
            //DebugPrinter.Print("Fail");

            BasketballAudioControl.Instance.PlayFail();

            SetHoopMovement(false);

            ResetBigRing();

            scoreCurrentControl.Reset();

            pointsCombo = 0;
        }

        private void AddScore(int value) 
        {
            scoreCurrentControl.Add(value);

            //DebugPrinter.Print(scoreCurrent);   

            if (scoreCurrentControl.GetValue() > scoreBestControl.GetValue())
            {   
                scoreBestControl.SaveAndShow(scoreCurrentControl.GetValue());

                //DebugPrinter.Print(scoreBest);   
            }
        }

        private void SetHoopMovement(bool value)
        {
            if (value)
            {
                if (hoopMovementComboAimOfGoals > 0)
                {
                    hoopMovementComboOfCurrentGoals++;

                    if (hoopMovementComboOfCurrentGoals
                        == hoopMovementComboAimOfGoals)
                    {
                        BasketballAudioControl.Instance.PlayGoalHoopMovement();

                        basketballHoopControl.RotateAround(
                            hoopMovementPivot.position);

                        hoopMovementComboOfCurrentGoals = 0;
                    }
                }
            }
            else
            {
                hoopMovementComboOfCurrentGoals = 0;
            }
        }

        private void CheckBigRingReset()
        {
            if (isBigRing)
            {
                bigRingCurrentGoalsAnyCount += 1;

                if (bigRingCurrentGoalsAnyCount > bigRingLimitOfGoalsAny)
                {
                    ResetBigRing();
                }
            }
            else
            {
                SetHoopMovement(true);
            }
        }

        private void CheckBigRingBonus()
        {
            if (!isBigRing && bigRingComboAimOfGoalsClear > 0)
            {
                bigRingComboOfCurrentGoalsClear += 1;

                if (bigRingComboOfCurrentGoalsClear >= bigRingComboAimOfGoalsClear)
                {
                    isBigRing = true;

                    StartCoroutine(SetBigRingCoroutine());
                }
            }
        }

        private void ResetBigRing()
        {
            if (isBigRing)
            {
                isBigRing = false;

                StartCoroutine(SetNormalRingCoroutine());

                bigRingComboOfCurrentGoalsClear = bigRingCurrentGoalsAnyCount = 0;
            }
        }

        private IEnumerator SetBigRingCoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            basketballHoopControl.SetBigRing();

            BasketballAudioControl.Instance.PlayGoalSetBigRing();
        }

        private IEnumerator SetNormalRingCoroutine()
        {
            yield return new WaitForSeconds(0.5f);

            basketballHoopControl.SetNormalRing();
        }
    }
}