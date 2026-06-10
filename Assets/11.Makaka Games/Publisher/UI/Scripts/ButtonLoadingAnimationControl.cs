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
using UnityEngine.UI;

namespace MakakaGames.Publisher.UI
{
	public class ButtonLoadingAnimationControl : MonoBehaviour
	{
		[Space]
		[SerializeField]
		private Button button;

		[SerializeField]
		private Image imageOverlayOnComplete;

		[SerializeField]
		private GameObject loadingAnimation;

		[SerializeField]
		private bool isCompletedOnStart = false;

		private void Start()
		{
			if (isCompletedOnStart)
			{
				Complete();
			}
			else
			{
				if (!imageOverlayOnComplete.enabled)
				{
					button.interactable = false;
				}
			}
		}

		public void Complete()
		{
			button.interactable = true;

			imageOverlayOnComplete.enabled = true;

			loadingAnimation.SetActive(false);
		}
	}
}