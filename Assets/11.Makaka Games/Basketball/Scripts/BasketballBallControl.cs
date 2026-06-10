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

using System;

using MakakaGames.ThrowControlX;
using MakakaGames.Publisher.Debugging;

#pragma warning disable 649

namespace MakakaGames.Basketball
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class BasketballBallControl : MonoBehaviour 
	{
		[SerializeField]
		private ThrowingObject throwingObject;

		public SphereCollider sphereCollider;

		[SerializeField]
		private int failMaterialIndex = 0;
		
		private bool isFloored = false; 
		private bool isRingTriggerPassed = false; 
		private bool isNetTriggerPassed = false;
		private bool isFail = false;
		private bool isGoaled = false;
		private bool isClear = false;
		
		public static Action OnFail;
		public static Action <bool> OnGoal;
		
		private void Awake()
		{
			throwingObject.OnResetPhysicsBase += ResetBall;
		}

		public static BasketballBallControl GetComponent(
			ThrowingObject throwingObject)
		{
			if (throwingObject & throwingObject.monoBehaviourCustom)
			{
				return (BasketballBallControl)throwingObject.monoBehaviourCustom;
			}
			else
			{
				return null;
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.gameObject.CompareTag(BasketballTagControl.NetTrigger)) 
			{
				if (isRingTriggerPassed)
				{
					SetGoaled();
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			switch (other.gameObject.tag) 
			{
				case BasketballTagControl.RingTrigger:

					if (!isNetTriggerPassed)
					{
						isRingTriggerPassed = true;
					}

					//DebugPrinter.Print("Ball — OnTriggerEnter() with Tag: "
					//+ other.gameObject.tag);

					break;

				case BasketballTagControl.NetTrigger:

					throwingObject.PlayAudioRandomlyDependingOnSpeed(
						BasketballAudioControl.Instance.netSoundsIndex, 
						false,
						BasketballAudioControl.Instance.netAudioSource);

					isNetTriggerPassed = true;

					if (!isRingTriggerPassed)
					{
						SetFailed();

						//DebugPrinter.Print("failed, touched basket");
					}

					break;
			}
			
			if (other.gameObject.CompareTag(BasketballTagControl.FailZone)) 
			{
				SetFailed();

				//DebugPrinter.Print("failed, FailZone: " + other.gameObject.tag);
			}
		}

		private void OnCollisionEnter (Collision other)
		{
			if (BasketballAudioControl.Instance)
			{
				switch (other.gameObject.tag)
				{
					case BasketballTagControl.Ring:

						isClear = false;

						throwingObject.PlayAudioRandomlyDependingOnSpeed(
							BasketballAudioControl.Instance.ringSoundsIndex,
							false,
							BasketballAudioControl.Instance.ringAudioSource);

						break;

					case BasketballTagControl.Floor:

						if (!isFloored)
						{
							isFloored = true;

							SetFailed();

							//DebugPrinter.Print("failed, floor");
						}

						throwingObject.PlayAudioRandomlyDependingOnSpeed(
							BasketballAudioControl.Instance.floorSoundsIndex,
							false,
							BasketballAudioControl.Instance.floorAudioSource);

						break;

					case BasketballTagControl.Backboard:

						throwingObject.PlayAudioRandomlyDependingOnSpeed(
							BasketballAudioControl.Instance.backboardSoundsIndex,
							false,
							BasketballAudioControl.Instance.backboardAudioSource);

						break;

					case BasketballTagControl.Pole:

						throwingObject.PlayAudioRandomlyDependingOnSpeed(
							BasketballAudioControl.Instance.poleSoundsIndex,
							false,
							BasketballAudioControl.Instance.poleAudioSource);

						break;

					case BasketballTagControl.Net:

						throwingObject.PlayAudioRandomlyDependingOnSpeed(
							BasketballAudioControl.Instance.netSoundsIndex,
							false,
							BasketballAudioControl.Instance.netAudioSource);

						break;
				}
			}
			else
			{
				DebugPrinter.Print(
					"BasketballAudioControl.Instance does not exists!");
			}
		}

		private void ResetBall()
		{
			//DebugPrinter.Print("Reset Ball.");

			isFloored = isRingTriggerPassed = isNetTriggerPassed =
				isFail = isGoaled = false;
			
			isClear = true;
		}
		
		private void SetGoaled()
		{
			if (!isGoaled && !isFail) 
			{
				isGoaled = true;

				OnGoal?.Invoke(isClear);
			}
		}

		private void SetFailed()
		{
			if (!isFail && !isGoaled) 
			{
				isFail = true;

				throwingObject.SetMaterial(failMaterialIndex);

				OnFail?.Invoke();
			}
		}

	}
}