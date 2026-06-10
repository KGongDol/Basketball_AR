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

using MakakaGames.Publisher.Scoring;
using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.ArrowDirectional;

namespace MakakaGames.ThrowControlX
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class ThrowAndScoreGameControl : MonoBehaviour
	{
		[Header("UI")]
		[SerializeField]
		private GameObject canvasPause;

		[SerializeField]
		private GameObject canvasStart;

		[SerializeField]
		private TextMeshProUGUI canvasStartTextInfo;

		[SerializeField]
		[TextArea(3, 4)]
		private string canvasStartTextInfoGyro;

		[SerializeField]
		[TextArea(3, 4)]
		private string canvasStartTextInfoAccelerometer;

		[Space]
		[SerializeField]
		private GameObject canvasesHUD;

		[Header("Game")]
		[SerializeField]
		private ThrowControl throwControl;

		private bool isFirstStart = true;

		[SerializeField]
		private FailControl failControl;

		[SerializeField]
		private ArrowDirectionalControl arrowDirectionalControl;

		[Space]
		[SerializeField]
		private int floorSoundsIndex = 1;

		[SerializeField]
		private int containerSoundsIndex = 2;

		[Header("Task")]
		[SerializeField]
		[Range(1, 10)]
		private int minTask = 1;

		[SerializeField]
		[Range(1, 10)]
		private int maxTask = 5;

		[Header("Score")]
		[SerializeField]
		private ScoreBestControl scoreBestControl;

		[SerializeField]
		private ScoreCurrentControl scoreCurrentControl;

		[SerializeField]
		private ScoreControl[] scoreControls;
		private ScoreControl scoreControlTemp;
		private ScoreControl scoreControlsTempCurrent;

		private int initedScoreControls = 0;
		private int scoreControlsValidCount = 0;
		private int maxNumberOfAttemptsToSetTaskForNewScoreControl = 11;


		[Header("Events")]
		[Space]
		[SerializeField]
		private UnityEvent OnInitialized;

		private void Start()
		{
			InitGame();
		}

		public void InitTutorialForGyro()
		{
			canvasStartTextInfo.text = canvasStartTextInfoGyro;
		}

		public void InitTutorialForAccelerometer()
		{
			canvasStartTextInfo.text = canvasStartTextInfoAccelerometer;
		}

		private void InitGame()
		{
			canvasStart.SetActive(true);
			
			InitThrowing();
		}

		public void RestartGame()
		{
			canvasStart.SetActive(false);

			if (isFirstStart)
			{
				isFirstStart = false;

				SetNextTask();

				throwControl.GetFirstThrow();
			}
		}

		private void InitThrowing()
		{
			StartCoroutine(InitThrowingCoroutine());
		}

		private IEnumerator InitThrowingCoroutine()
		{
			yield return null;

			throwControl.OnInitialized.AddListener(InitScoreControls);
			throwControl.gameObject.SetActive(true);
		}

		private void InitScoreControls()
		{
			for (int i = 0; i < scoreControls.Length; i++)
			{
				scoreControlTemp = scoreControls[i];

				if (scoreControlTemp)
				{
					scoreControlsValidCount++;
				}
			}

			for (int i = 0; i < scoreControls.Length; i++)
			{
				scoreControlTemp = scoreControls[i];

				if (scoreControlTemp)
				{
					scoreControlTemp.Init(
						throwControl.GetObjectCount(),
						CountInitedScoreControl,
						SetNextTask,
						OnTaskCleared,
						AddScore,
						TouchContainer);
				}
			}
		}

		private void CountInitedScoreControl()
		{
			initedScoreControls++;

			//DebugPrinter.Print("Inited: #" + var);

			if (initedScoreControls == scoreControlsValidCount)
			{
				failControl.Init(
					throwControl.GetObjectCount(),
					() => OnInitialized.Invoke(),
					TouchFailPlane,
					Fail);

				//DebugPrinter.Print("All Items Inited");
			}
		}

		private void TouchContainer(GameObject touchingObject)
		{
			throwControl.PlayAudioRandomlyDependingOnSpeed(
				containerSoundsIndex, touchingObject, false);
		}

		private void TouchFailPlane(GameObject failedObject)
		{
			throwControl.PlayAudioRandomlyDependingOnSpeed(
				floorSoundsIndex, failedObject, false);
		}

		private void Fail(GameObject failedObject)
		{
			scoreCurrentControl.Reset();

			throwControl.SetMaterial(failControl.failMaterialIndex, failedObject);
		}

		private void AddScore(GameObject scoredObject)
		{
			// Memorize Time of Scoring
			failControl.RegisterAfterScoring(scoredObject);

			scoreCurrentControl.Add(1);

			//DebugPrinter.Print(scoreCurrent);   

			if (scoreCurrentControl.GetValue() > scoreBestControl.GetValue())
			{
				scoreBestControl.SaveAndShow(scoreCurrentControl.GetValue());

				//DebugPrinter.Print(scoreBest);   
			}
		}

		private void SetNextTask()
		{
			for (int i = 0; i <= maxNumberOfAttemptsToSetTaskForNewScoreControl;
				i++)
			{
				scoreControlTemp =
					scoreControls[Random.Range(0, scoreControls.Length)];

				if (i < maxNumberOfAttemptsToSetTaskForNewScoreControl)
				{
					if (scoreControlTemp
						&& !ReferenceEquals(scoreControlTemp,
							scoreControlsTempCurrent))
					{
						for (int j = 0; j < scoreControls.Length; j++)
						{
							if (!ReferenceEquals(scoreControls[j],
								scoreControlsTempCurrent))
							{
								scoreControls[j].ClearTask();
							}
						}
						
						SetTaskTo(scoreControlTemp);

						scoreControlsTempCurrent = scoreControlTemp;

						return;
					}
				}
				else if (scoreControlsTempCurrent)
				{
					SetTaskTo(scoreControlsTempCurrent);

					//DebugPrinter.Print("Set Task for Previous Score Control");
				}
				else
				{
					DebugPrinter.Print(
						"Oops. No more Score Controls for Task Setting");
				}
			}
		}

		private void SetTaskTo(ScoreControl scoreControl)
		{
			scoreControl.SetTask(Random.Range(minTask, maxTask + 1));

			if (arrowDirectionalControl)
			{
				arrowDirectionalControl.target = scoreControl.transform;
			}
		}

		private void OnTaskCleared()
		{
			//DebugPrinter.Print("Task Cleared!");
		}
	}
}