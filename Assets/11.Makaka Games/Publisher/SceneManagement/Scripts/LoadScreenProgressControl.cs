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

namespace MakakaGames.Publisher.SceneManagement
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class LoadScreenProgressControl : MonoBehaviour 
	{
		UnityEngine.UI.Slider slider;
		
		void Start () 
		{
			slider = GetComponent<UnityEngine.UI.Slider>();
		}
		
		void Update () 
		{
			// Use the property Progress to get the load percentage! Remember the value is between 0 and 100.
			slider.value = LoadScreenControl.Instance.Progress;
		}
	}
}