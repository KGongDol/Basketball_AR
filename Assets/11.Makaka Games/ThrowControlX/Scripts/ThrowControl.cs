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
using UnityEngine.InputSystem.EnhancedTouch;

using System.Collections;
using System.Collections.Generic;

using MakakaGames.Publisher.Debugging;
using MakakaGames.Publisher.SceneManagement;
using MakakaGames.Publisher.RandomObjectPool;
using MakakaGames.Publisher.TagSelectorPropertyDrawerX;

#pragma warning disable 649

namespace MakakaGames.ThrowControlX
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class ThrowControl : MonoBehaviour
	{
		[SerializeField]
		private RandomObjectPooler randomObjectPooler;

		[Space]
		[Tooltip("Event that is dispatched after All Throwing Objects have been" 
			+ " Created and Configured — before Getting the First Throw.")]
		public UnityEvent OnInitialized;

		[Header("FPS (throw force takes into account the speed" +
			" of the player's movement) ")]
		[Tooltip("It's useful when creating FPS Games.")]
		public CharacterController characterControllerFPS;
		private float characterControllerFPSSpeedCurrent = 0f;

		[Header("Camera")]
		[Tooltip("It's intended for First-Person View and used to position Throwing" 
			+ " Objects on the Screen.")]
		public Camera cameraMain;

		[Header("Modes")]
		[Tooltip("Throwing Mode by Default.")]
		public Mode modeAtAwake = Mode.ClickOrTap;
		
		[Tooltip("Throwing Mode for Mobile Platforms.")]
		public Mode modeAtAwakeForMobile = Mode.ClickOrTap;

		public enum Mode
		{
			Flick,
			ClickOrTap,
			PressKey
		}

		[Header("Press Key Mode")]
		[Tooltip("Selected Key will be used when setting Throwing Mode to Press"
			+ " Key.")]
		[SerializeField]
		private Key KeyForPressKey;

		[Header("Flick Mode")]
		[Tooltip("The flag will be used when setting Throwing Mode to Flick."
			+ " If it’s false, then it allows fast flicks only."
			+ "\n\nPositions of the mouse cursor (or finger) in the last & previous"
			+ " frames are considered."
			+ "\n\nFalse Example:\nForce Factor Extra = 45"
			+ " and Input Sensitivity X = 7.")]
		public bool isFullPathForFlick = true;

		[Tooltip("The value will be used when setting Throwing Mode to Flick."
			+ " The higher the value, the faster the Throwing Object follows the"
			+ " mouse cursor (or finger) when held (touched).")]
		public float lerpTimeFactorOnTouchForFlick = 20f;

		private bool isTouchForFlick = false;

		[Header("Throw")]
		[Tooltip("If it’s true, Throwing will not work when clicking UI, e.g. Menu"
			+ " Button. It checks whether the Pointer (mouse cursor or finger) is"
			+ " over a UI object or not.")]
		public bool isThrowingBlockedWhenClickingUI = true;

		[Space]
		[Tooltip("It’s relevant when using Character Controller FPS."
			+ "\n\nIf it’s false, then Input Position Fixed Screen Factor X/Y are"
			+ " used instead of the position of the mouse cursor (or finger).")]
		public bool isInputPositionFixed = false;

		[Tooltip("It’s relevant when using flag:\nIs Input Position Fixed.")]
		[Range(0.01f, 1f)]
		public float inputPositionFixedScreenFactorX = 0.48f;

		[Tooltip("It’s relevant when using flag:\nIs Input Position Fixed.")]
		[Range(0.01f, 1f)]
		public float inputPositionFixedScreenFactorY = 0.52f;

		[Space]
		[Tooltip("It changes User Input and affects Throwing Force for All Throwing"
			+ " Objects at once.")]
		public Vector2 inputSensitivity = new(1f, 100f);

		[Tooltip("It allows changing parameter described in ThrowingObject.cs but"
			+ " for All Throwing Objects at once with an Extra value.")]
		public float forceFactorExtra = 10f;

		[Tooltip("It allows changing parameter described in ThrowingObject.cs but"
			+ " for All Throwing Objects at once with an Extra value.")]
		public float torqueFactorExtra = 60f;

		[Tooltip("It allows changing parameter described in ThrowingObject.cs but"
			+ " for All Throwing Objects at once with an Extra value.")]
		public float torqueAngleExtra;

		[Tooltip("It’s used to avoid affecting the Movement of the Throwing Object"
			+ " by Movement of Pool Parent, described in RandomObjectPooler.cs"
			+ "\n(e.g., Main Camera)."
			+ "\n\nAfter the End of Throw during the \"Reset\" stage, the Throwing"
			+ " Object’s parent will be set to the Pool Parent.")]
		public Transform parentOnThrow;

		[Space]
		[Tooltip("Event that is dispatched at the Moment of the Throw: after"
			+ " completing all the preparations for the Throw.")]
		public UnityEventWithThrowingObject OnThrow;

		[Header("Next Throw")]
		[Tooltip("Seconds to start Getting Next Throw after the Moment of the"
			+ " Throw.")]
		[Range(0.1f, 10f)]
		public float nextThrowGettingDelay = 0.1f;

		/// <summary> Seconds for next try of coroutine call (min = 0.1f).</summary>
		private const float nextCoroutineCallTryDelay = 0.1f;
		private bool isNextThrowGetting;

		private GameObject gameObjectTemp;
		private ThrowingObject throwingObjectTemp;

		[Space]
		[Tooltip("Event that is dispatched at the first stage of the Throwing"
			+ " Cycle: after finding the available Throwing Object in the Object"
			+ " Pool and preparing it for the Throw.")]
		public UnityEventWithThrowingObject OnNextThrowGetting;

		[Header("Tag")]
		[Tooltip("Enables assigning tags for Throwing Objects at Runtime, keeping"
			+ " your prefabs untouched.")]
		public bool isTagCustomSetOnInit = false;

		[Tooltip("It’s relevant when using flag:\nIs Tag Custom Set On Init.")]
		[TagSelector]
		public string tagCustomOnInit = TagSelectorAttribute.Untagged;

		[Header("Layer Changing (to avoid unwanted and mutual collisions)")]
		[Tooltip("Allows avoiding unwanted and mutual collisions related to"
			+ " Throwing Objects on\n\"Initialize Throwing Objects\" stage."
			+ "\n\nLayer thatcalled NoCollisionsInLayer is set by Default,"
			+ "  that is customized in\n\"Getting Started\" Tutorial. ")]
		public bool isLayerCustomSetOnInit = true;

		[Tooltip("It’s relevant when using flag:\nIs Layer Custom Set On Init.")]
		[TagSelector]
		public LayerMask layerOnInit;

		[Space]
		[Tooltip("After the Moment of the Throw, it enables interacting of"
			+ " Throwing Objects with the Environment and between each other."
			+ "\n\nOn the “Reset” stage, it allows setting a neutral layer without"
			+ " collisions (NoCollisionsInLayer is set by Default, that is"
			+ " customized in the “Getting Started” Tutorial) that avoids"
			+ " overlapping of Throwing Objects on each other and on other"
			+ " physical objects. ")]
		public bool isLayerCustomSetOnThrowAndReset = true;

		[Tooltip("Seconds after the Moment of the Throw to avoid overlapping of"
			+ " Throwing Objects on each other and on other physical objects.")]
		[Range(0f, 5f)]
		public float layerSettingOnThrowDelay = 1f;

		[Tooltip("It’s relevant when using flag:"
			+ "\nIs Layer Custom Set On Throw And Reset.")]
		public LayerMask layerOnThrow;

		[Tooltip("It’s relevant when using flag:"
			+ "\nIs Layer Custom Set On Throw And Reset.")]
		public LayerMask layerOnReset;

		[Tooltip("May be useful in very rare cases. E.g., when you play with Time"
			+ " Scale and Layers can’t be changed quickly.")]
		[Range(0f, 1f)]
		public float layerSettingOnResetFinishingDelay = 0f;

		private int layerIndexOnInit;
		private int layerIndexOnThrow;
		private int layerIndexOnReset;

		[Header("Reset (must be called after the end of “Fade Out” stage)")]
		[Tooltip("Seconds to start “Reset” stage after the Moment of the Throw.")]
		[Range(0f, 10f)]
		public float resetDelay = 4f;

		[Space]
		[Tooltip("Event that is dispatched at the last stage of the Throwing"
			+ " Cycle: after preparing the Throwing Object to be available in the"
			+ " Object Pool.")]
		public UnityEventWithThrowingObject OnReset;

		private GameObject gameObjectCurrent;
		private ThrowingObject throwingObjectCurrent;

		private RaycastHit raycastHit;

		private bool isInputBegan = false;
		private bool isInputEnded = false;
		private bool isInputHeldDown = false;

		private Vector3 inputPositionCurrent;
		private Vector3 inputPositionPivot;

		[Header("Fade")]
		[Tooltip("Enables Fading In & Fading Out for All Throwing Objects at once."
			+ " Customization for Individual prefab of Throwing Object can be done"
			+ " on the Material Control component.")]
		public bool isFadingOn = true;

		[Header("Fade Out (must be completed before “Reset” stage)")]
		[Space]
		[Tooltip("Event that is dispatched when Fading Out actually starts — timing"
			+ " is synchronized with the Delay Out parameter of the Material"
			+ " Control component on Individual prefab of Throwing Object.")]
		public UnityEventWithThrowingObject OnFadingOut;

		[System.Serializable]
		public class UnityEventWithThrowingObject : UnityEvent<ThrowingObject> { }

		// ---------
		// DEBUGGING
		// ---------

	#if DEBUG

		// private NumberDebugger positionDebugger = new();

		public void TestEvent(int i)
		{
			DebugPrinter.Print(
				"Event Call: " + i + ", " + System.DateTime.Now.TimeOfDay);
		}

	#endif

		protected void OnEnable()
		{
			if (SceneControl.IsMobilePlatform())
			{
				EnhancedTouchSupport.Enable();
			}
		}

		protected void OnDisable()
		{
			if (SceneControl.IsMobilePlatform())
			{
				EnhancedTouchSupport.Disable();
			}
		}

		/// <summary>Call after pool initialisation.</summary>
		public void InitThrowingObjects()
		{
			StartCoroutine(InitThrowingObjectsCoroutine());
		}

		/// <summary>Init physics correctly.</summary>
		private IEnumerator InitThrowingObjectsCoroutine()
		{
			if (randomObjectPooler)
			{
				if (cameraMain)
				{
					InitLayerIndexes();

					randomObjectPooler.InitControlScripts(typeof(ThrowingObject));

					for (int i = 0; i < randomObjectPooler.pooledObjects.Count; i++)
					{
						gameObjectTemp = randomObjectPooler.pooledObjects[i];

						if (gameObjectTemp)
						{
							gameObjectTemp.SetActive(true);

							throwingObjectTemp =
								randomObjectPooler.RegisterControlScript(
									gameObjectTemp) as ThrowingObject;

							throwingObjectTemp.SetRendererEnabled(false);

							if (isTagCustomSetOnInit)
							{
								throwingObjectTemp.tag = tagCustomOnInit;
							}

							if (isLayerCustomSetOnInit)
							{
								ChangeLayer(throwingObjectTemp.gameObject,
									layerIndexOnInit);
							}

							yield return new WaitForFixedUpdate();

							StartCoroutine(ResetCoroutine
								(nextCoroutineCallTryDelay, throwingObjectTemp,
								true));

							yield return new WaitForFixedUpdate();
						}
					}

					yield return new WaitForSeconds(nextCoroutineCallTryDelay);

					yield return null;
					yield return null;
					yield return null;

					if (SceneControl.IsMobilePlatform())
					{
						modeAtAwake = modeAtAwakeForMobile;
					}

					OnInitialized.Invoke();
				}
				else
				{
					DebugPrinter.PrintError(
						"Camera Main is Null. Assign it in the Editor.");
				}
			}
			else
			{
				DebugPrinter.PrintError("Random Object Pooler is Null." +
					" Assign it in the Editor.");
			}
		}

		public void GetFirstThrow()
		{
			GetNextThrow(nextCoroutineCallTryDelay);
		}

		private void Update()
		{
			if (!isNextThrowGetting && gameObjectCurrent
				&& throwingObjectCurrent && !throwingObjectCurrent.isThrown)
			{
				if (SceneControl.IsMobilePlatform())
				{
					if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 1)
					{
						var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

						isInputBegan = touch.phase == UnityEngine.InputSystem.TouchPhase.Began;
						isInputEnded = touch.phase == UnityEngine.InputSystem.TouchPhase.Ended;
						isInputHeldDown = true;

						inputPositionCurrent = isInputPositionFixed
							? GetInputPositionFixed()
							: (Vector3)touch.screenPosition;
					}
					else
					{
						return;
					}
				}
				else
				{
					if (modeAtAwake == Mode.PressKey)
					{
						isInputBegan = Keyboard.current[KeyForPressKey].wasPressedThisFrame;
						isInputEnded = Keyboard.current[KeyForPressKey].wasReleasedThisFrame;
						isInputHeldDown = Keyboard.current[KeyForPressKey].isPressed;
					}
					else
					{
						isInputBegan = Mouse.current.leftButton.wasPressedThisFrame;
						isInputEnded = Mouse.current.leftButton.wasReleasedThisFrame;
						isInputHeldDown = Mouse.current.leftButton.isPressed;
					}

					inputPositionCurrent =
						isInputPositionFixed
						? GetInputPositionFixed()
						: Mouse.current.position.ReadValue();
				}

				if (isTouchForFlick)
				{
					//DebugPrinter.Print("isTouchForFlick");

					StartCoroutine(OnTouchForFlickCoroutine());
				}

				//DebugPrinter.Print("isThrown check: " + isThrown);

				if (isInputBegan && !IsPointerOverUIObject(inputPositionCurrent))
				{
					//DebugPrinter.Print("isInputBegan");

					if (Physics.Raycast(cameraMain.ScreenPointToRay(
						inputPositionCurrent), out raycastHit, 100f))
					{
						if (modeAtAwake == Mode.Flick)
						{
							if (raycastHit.rigidbody
								== throwingObjectCurrent.rigidbody3D)
							{
								isTouchForFlick = true;

								inputPositionPivot = inputPositionCurrent;
							}
							// If click or tap OUTSIDE of Throwing Game Object
							// when Flick Mode => No Throw
							else
							{
								inputPositionPivot = Vector3.zero;
							}
						}
					}
					// If click or tap OUTSIDE Any Object
					else
					{
						if (modeAtAwake == Mode.Flick)
						{
							inputPositionPivot = Vector3.zero;
						}
					}
				}

				// Next Update()
				if (isInputEnded)
				{
					//DebugPrinter.Print("isInputEnded");

					if (modeAtAwake == Mode.Flick
						&& inputPositionPivot != Vector3.zero)
					{
						//DebugPrinter.Print("Mode.Flick => Throw()");

						//DebugPrinter.Print(inputPositionPivot);
						//DebugPrinter.Print(inputPositionCurrent);

						throwingObjectCurrent.isThrown = true;

						StartCoroutine(ThrowCoroutine(
							inputPositionPivot,
							inputPositionCurrent,
							throwingObjectCurrent));

						isTouchForFlick = false;

						inputPositionPivot = Vector3.zero;
					}
				}

				if (isInputHeldDown && !IsPointerOverUIObject(inputPositionCurrent))
				{
					//DebugPrinter.Print("isInputHeldDown");

					//It allows fast flicks only. See Tooltip for isFullPathForFlick
					if (modeAtAwake == Mode.Flick && !isFullPathForFlick)
					{
						if (inputPositionPivot != Vector3.zero)
						{
							inputPositionPivot = inputPositionCurrent;
						}
					}
					else if ((modeAtAwake == Mode.ClickOrTap
						|| modeAtAwake == Mode.PressKey)
						&& inputPositionPivot.y < inputPositionCurrent.y)
					{
						//DebugPrinter.Print("Mode.ClickOrTap => Throw()");

						inputPositionPivot = cameraMain.ViewportToScreenPoint(
							throwingObjectCurrent.positionInViewportOnReset);

						throwingObjectCurrent.isThrown = true;

						StartCoroutine(ThrowCoroutine(
							inputPositionPivot,
							inputPositionCurrent,
							throwingObjectCurrent));
					}
				}
			}
		}

		private Vector3 GetInputPositionFixed()
		{
			return new Vector3(
				Screen.width * inputPositionFixedScreenFactorX,
				Screen.height * inputPositionFixedScreenFactorY,
				0f);
		}

		private IEnumerator OnTouchForFlickCoroutine()
		{
			yield return new WaitForFixedUpdate();

			inputPositionCurrent.z = cameraMain.nearClipPlane
				* throwingObjectCurrent.cameraNearClipPlaneFactorOnReset;

			throwingObjectCurrent.transform.position = Vector3.Lerp(
				throwingObjectCurrent.transform.position,
				cameraMain.ScreenToWorldPoint(inputPositionCurrent),
				Time.fixedDeltaTime * lerpTimeFactorOnTouchForFlick
			);
		}

		private IEnumerator ThrowCoroutine(
			Vector2 inputPositionFirst,
			Vector2 inputPositionLast,
			ThrowingObject throwingObject)
		{
			throwingObject.transform.parent = parentOnThrow;

			if (modeAtAwake == Mode.ClickOrTap || modeAtAwake == Mode.PressKey)
			{
				yield return new WaitForFixedUpdate();

				throwingObject.SetCollidersEnabled(true);

				//DebugPrinter.Print(throwingObject.gameObject.name
				//	+ " : SetCollidersEnabled(true)");
			}

			yield return new WaitForFixedUpdate();

			if (isLayerCustomSetOnThrowAndReset)
			{
				StartCoroutine(ChangeLayerCoroutine(
					layerSettingOnThrowDelay,
					throwingObject.gameObject,
					layerIndexOnThrow));
			}

			if (characterControllerFPS)
			{
				characterControllerFPSSpeedCurrent =
					characterControllerFPS.transform.InverseTransformDirection(
						characterControllerFPS.velocity).z;

				//DebugPrinter.Print(characterControllerFPSSpeedCurrent);
			}

			//DebugPrinter.Print(characterControllerFPSSpeedCurrent);

			throwingObject.ThrowBase(
				inputPositionFirst,
				inputPositionLast,
				inputSensitivity,
				cameraMain.transform,
				Screen.height,
				forceFactorExtra + characterControllerFPSSpeedCurrent,
				torqueFactorExtra,
				torqueAngleExtra);

			//DebugPrinter.Print(throwingObject.gameObject.name
			//	+ " : IEnumerator Throw()");

			if (isFadingOn && throwingObject.materialControl)
			{
				throwingObject.materialControl.Fade(false, false);

				StartCoroutine(FadeOutCoroutine(
					throwingObject.materialControl.delayOut, throwingObject));
			}

			// Wait for physics changing
			yield return new WaitForFixedUpdate();

			throwingObject.PlayAudioWhoosh();

			OnThrow.Invoke(throwingObject);

			StartCoroutine(ResetCoroutine(resetDelay, throwingObject));

			GetNextThrow(nextThrowGettingDelay);
		}

		private IEnumerator FadeOutCoroutine(
			float delay, ThrowingObject throwingObject)
		{
			yield return new WaitForSeconds(delay);

			OnFadingOut.Invoke(throwingObject);
		}

		private IEnumerator ResetCoroutine(
			float delay, ThrowingObject throwingObject,
			bool isPositionInitial = false)
		{
			yield return new WaitForSeconds(delay);
			yield return new WaitForFixedUpdate();

			if (isLayerCustomSetOnThrowAndReset)
			{
				ChangeLayer(throwingObject.gameObject, layerIndexOnReset);

				yield return new WaitForSeconds(layerSettingOnResetFinishingDelay);
			}

			throwingObject.isThrown = false;
			throwingObject.ResetPhysicsBase();

			if (modeAtAwake == Mode.ClickOrTap || modeAtAwake == Mode.PressKey)
			{
				throwingObject.SetCollidersEnabled(false);

				//DebugPrinter.Print(throwingObject.gameObject.name
				//	+ " : SetCollidersEnabled(false)");
			}
			else
			{
				throwingObject.ActivateTriggersOnColliders(true);

				//DebugPrinter.Print(throwingObject.gameObject.name
				//	+ " : ActivateTriggersOnColliders(true)");
			}

			throwingObject.SetRendererEnabled(false);

			yield return new WaitForFixedUpdate();

			if (isPositionInitial && randomObjectPooler.positionAtInit)
			{
				throwingObject.ResetPosition(
					randomObjectPooler.positionAtInit.position);
			}
			else
			{
				throwingObject.ResetPosition(cameraMain);
			}

			throwingObject.ResetRotation(randomObjectPooler.poolParent);

			yield return new WaitForFixedUpdate();

			throwingObject.transform.parent = randomObjectPooler.poolParent;
			throwingObject.gameObject.SetActive(false);

			OnReset.Invoke(throwingObject);
		}

		private void GetNextThrow(float delay)
		{
			isNextThrowGetting = true;

			StartCoroutine(GetNextThrowCoroutine(delay));
		}

		private IEnumerator GetNextThrowCoroutine(float delay)
		{
			gameObjectCurrent = null;
			throwingObjectCurrent = null;

			yield return new WaitForSeconds(delay);

			gameObjectTemp = randomObjectPooler.GetPooledObject();

			if (gameObjectTemp)
			{
				gameObjectTemp.SetActive(true);

				throwingObjectTemp = randomObjectPooler.RegisterControlScript(
					gameObjectTemp) as ThrowingObject;

				throwingObjectTemp.ResetPosition(cameraMain);
				throwingObjectTemp.ResetRotation(randomObjectPooler.poolParent);

				//DebugPrinter.Print(
				//	throwingObjectTemp.rigidbody3D.rotation.eulerAngles);

				if (modeAtAwake == Mode.Flick)
				{
					yield return new WaitForFixedUpdate();

					throwingObjectTemp.ActivateTriggersOnColliders(false);

					//DebugPrinter.Print(throwingObjectTemp.name
					//	+ " : ActivateTriggersOnColliders(false)");
				}

				yield return new WaitForFixedUpdate();

				throwingObjectTemp.SetRendererEnabled(true);

				if (isFadingOn && throwingObjectTemp.materialControl)
				{
					throwingObjectTemp.materialControl.Fade(true, true, true);
				}

				// ---------------------
				// DEBUGGING OF POSITION
				// ---------------------

	// #if DEBUG

	// 			positionDebugger.DebugFloatAbsChanging(
	// 				2f, throwingObjectTemp.rigidbody3D.position.x);

	// #endif

				throwingObjectCurrent = throwingObjectTemp;
				gameObjectCurrent = gameObjectTemp;

				isNextThrowGetting = false;

				OnNextThrowGetting.Invoke(throwingObjectCurrent);
			}
			else
			{
				//DebugPrinter.Print("GetNextThrowBase() => false");

				StartCoroutine(GetNextThrowCoroutine(nextCoroutineCallTryDelay));
			}
		}

		public void PlayAudioRandomlyDependingOnSpeed(
			int index,
			GameObject to,
			bool isStoppedBeforePlay,
			AudioSource audioSource = null)
		{
			ThrowingObject throwingObjectTemp =
				randomObjectPooler.RegisterControlScript(to) as ThrowingObject;

			if (throwingObjectTemp)
			{
				throwingObjectTemp.PlayAudioRandomlyDependingOnSpeed(
					index, isStoppedBeforePlay, audioSource);
			}
		}

		public void PlayAudioRandomlyDependingOnSpeed(
			ThrowingObject.AudioData audioData,
			GameObject to,
			bool isStoppedBeforePlay,
			AudioSource audioSource = null)
		{
			ThrowingObject throwingObjectTemp =
				randomObjectPooler.RegisterControlScript(to) as ThrowingObject;

			if (throwingObjectTemp)
			{
				throwingObjectTemp.PlayAudioRandomlyDependingOnSpeed(
					audioData, isStoppedBeforePlay, audioSource);
			}
		}

		public void SetMaterial(Material material, GameObject to)
		{
			ThrowingObject throwingObjectTemp =
				randomObjectPooler.RegisterControlScript(to) as ThrowingObject;

			if (throwingObjectTemp)
			{
				throwingObjectTemp.SetMaterial(material);
			}
		}

		public void SetMaterial(int index, GameObject to)
		{
			ThrowingObject throwingObjectTemp =
				randomObjectPooler.RegisterControlScript(to) as ThrowingObject;

			if (throwingObjectTemp)
			{
				throwingObjectTemp.SetMaterial(index);
			}
		}

		private IEnumerator ChangeLayerCoroutine(
			float delay, GameObject to, int layerIndex)
		{
			yield return new WaitForSeconds(delay);

			ChangeLayer(to, layerIndex);
		}

		private void ChangeLayer(GameObject to, int layerIndex)
		{
			to.layer = layerIndex;

			//DebugPrinter.Print(layerIndex);
		}

		private int LayerMaskValueToIndex(int value)
		{
			return Mathf.RoundToInt(Mathf.Log(value, 2));
		}

		private void InitLayerIndexes()
		{
			layerIndexOnInit = LayerMaskValueToIndex(layerOnInit.value);
			layerIndexOnThrow = LayerMaskValueToIndex(layerOnThrow.value);
			layerIndexOnReset = LayerMaskValueToIndex(layerOnReset.value);
		}

		private bool IsPointerOverUIObject(Vector2 touchPosition)
		{
			if (isThrowingBlockedWhenClickingUI)
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
			else
			{
				return false;
			}
		}

		public int GetObjectCount()
		{
			return randomObjectPooler.controlScripts.Count;
		}

		public void SetPrefabBeforeInit(GameObject gameObject)
		{
			randomObjectPooler.prefab = gameObject;
		}

		public void SetPrefabsBeforeInit(GameObject[] gameObjects)
		{
			randomObjectPooler.prefabs = gameObjects;
		}
	}
}