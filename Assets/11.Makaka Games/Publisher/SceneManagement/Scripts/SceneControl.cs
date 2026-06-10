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
using UnityEngine.SceneManagement;

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.SceneManagement
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class SceneControl : MonoBehaviour 
	{
		[SerializeField]
		private string nameOfSceneWithLoadScreen = "LoadScreen";

		[Space]
		[SerializeField]
		private UnityEvent OnInitialized;

		private void Awake()
		{
			OnInitialized.Invoke();
		}

		public void LoadSceneWithScreenOrientationLandscapeLeft(string sceneName)
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;

			LoadScene(sceneName);
		}

		public void LoadSceneWithScreenOrientationPortrait(string sceneName)
		{
			Screen.orientation = ScreenOrientation.Portrait;

			LoadScene(sceneName);
		}

		public void LoadSceneWithScreenOrientationAuto(string sceneName)
		{
			Screen.orientation = ScreenOrientation.AutoRotation;

			LoadScene(sceneName);
		}

		public void LoadScene(string sceneName)
		{
			LoadScreenControl.Instance.LoadScene(
				sceneName, false, nameOfSceneWithLoadScreen);
		}

		public void ReloadCurrentScene()
		{
			LoadScreenControl.Instance.LoadScene(
				SceneManager.GetActiveScene().name,
				false,
				nameOfSceneWithLoadScreen);
		}

		public void QuitGame()
		{
			Application.Quit();
		}

		public void OpenLink(string link = "https://makaka.org/support")
		{
			Application.OpenURL(link);
		}

		public void SetTargetFrameRate(int frameRate)
		{
			Application.targetFrameRate = frameRate;

			DebugPrinter.Print(
				$"[SceneControl] App Target FPS => {frameRate}");
		}

		public void SetTargetFrameRateTo60()
		{
			SetTargetFrameRate(60);
		}

		public void SetTargetFrameRateTo30()
		{
			SetTargetFrameRate(30);
		}

		public void SetTargetFrameRateByDefault()
		{
			SetTargetFrameRate(-1);
		}

		public static string GetActiveSceneName()
		{
			return SceneManager.GetActiveScene().name;
		}

		public static bool IsMobilePlatform()
		{
			return Application.isMobilePlatform
				|| Application.platform == RuntimePlatform.IPhonePlayer
				|| Application.platform == RuntimePlatform.Android;
		}

		public static bool IsWebGLOnDesktop()
		{
			return !Application.isMobilePlatform
				&& Application.platform == RuntimePlatform.WebGLPlayer;
		}

		public static bool IsWebGLOnMobile()
		{
			return Application.isMobilePlatform
				&& Application.platform == RuntimePlatform.WebGLPlayer;
		}
	}
}