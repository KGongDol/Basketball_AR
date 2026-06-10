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

using System.Collections;

#pragma warning disable 649

namespace MakakaGames.Basketball
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class BasketballAudioControl : MonoBehaviour 
	{
		public static BasketballAudioControl Instance;
		
		[Header("Goal: Normal")]
		[SerializeField]
		private AudioSource goalNormalAudioSource;
		
		[SerializeField]
		private AudioClip goalNormal;
		
		[SerializeField]
		private float goalNormalDelay = 0f;
		
		[Header("Goal: Clear")]
		[SerializeField]
		private AudioSource goalClearAudioSource;
		
		[SerializeField]
		private AudioClip goalClear;
		
		[SerializeField]
		private float goalClearDelay = 0f;

		[Header("Goal: Set Big Ring")]
		[SerializeField]
		private AudioSource goalSetBigRingAudioSource;
		
		[SerializeField]
		private AudioClip goalSetBigRing;
		
		[SerializeField]
		private float goalSetBigRingDelay = 0f;

		[Header("Goal: Hoop Movement")]
		[SerializeField]
		private AudioSource goalHoopMovementAudioSource;
		
		[SerializeField]
		private AudioClip goalHoopMovement;
		
		[SerializeField]
		private float goalHoopMovementDelay = 0f;

		[Header("Fail")]
		[SerializeField]
		private AudioSource failAudioSource;
		
		[SerializeField]
		private AudioClip[] failCollisions;

		[Header("Collisions")]
		public AudioSource floorAudioSource;
		public int floorSoundsIndex = 1;

		[Space]
		public AudioSource poleAudioSource;
		public int poleSoundsIndex = 2;

		[Space]
		public AudioSource backboardAudioSource;
		public int backboardSoundsIndex = 3;

		[Space]
		public AudioSource ringAudioSource;
		public int ringSoundsIndex = 4;

		[Space]
		public AudioSource netAudioSource;
		public int netSoundsIndex = 5;

		private void Start()
		{
			Instance = this;
		}
		
		public void PlayGoalNormal()
		{
			StartCoroutine(PlayGoalNormalCoroutine());
		}

		public IEnumerator PlayGoalNormalCoroutine()
		{
			yield return new WaitForSeconds(goalNormalDelay);

			goalNormalAudioSource.PlayOneShot(goalNormal);
		}
		
		public void PlayGoalClear()
		{
			StartCoroutine(PlayGoalClearCoroutine());
		}

		public IEnumerator PlayGoalClearCoroutine()
		{
			yield return new WaitForSeconds(goalClearDelay);

			goalClearAudioSource.PlayOneShot(goalClear);
		}
		
		public void PlayGoalSetBigRing()
		{
			StartCoroutine(PlayGoalSetBigRingCoroutine());
		}

		public IEnumerator PlayGoalSetBigRingCoroutine()
		{
			yield return new WaitForSeconds(goalSetBigRingDelay);

			goalSetBigRingAudioSource.PlayOneShot(goalSetBigRing);
		}

		public void PlayGoalHoopMovement()
		{
			StartCoroutine(PlayGoalHoopMovementCoroutine());
		}

		public IEnumerator PlayGoalHoopMovementCoroutine()
		{
			yield return new WaitForSeconds(goalHoopMovementDelay);

			goalHoopMovementAudioSource.PlayOneShot(goalHoopMovement);
		}

		public void PlayFail()
		{
			failAudioSource.PlayOneShot(
				failCollisions[UnityEngine.Random.Range(0, failCollisions.Length)]);
		}
	}
}