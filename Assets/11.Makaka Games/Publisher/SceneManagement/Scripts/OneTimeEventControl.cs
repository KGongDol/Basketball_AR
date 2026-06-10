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
using UnityEngine.InputSystem;

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.SceneManagement
{
	[HelpURL("https://makaka.org/unity-assets")]
	public class OneTimeEventControl : MonoBehaviour 
	{
		[TextArea(2,3)]
		[SerializeField]
		private string description;

		[Space]
		[SerializeField]
		private bool isDebugLogging = false;

		[SerializeField]
		private bool isExecutedAgain = false;

		[Space]
		[SerializeField]
		private Key oneTimeFunctionKey = Key.Enter;

		[Space]
		[SerializeField]
		private UnityEvent OnPressOneTimeFunctionKey;

		private bool isOneTimeFunctionCalled = false;

		private void Update() 
		{
			if (Keyboard.current[oneTimeFunctionKey].wasPressedThisFrame)
			{
				if (isExecutedAgain)
				{
					ExecuteAgain();
				}
				else
				{
					Execute();
				}
			}
		}

		public void Execute()
		{
			if (!isOneTimeFunctionCalled)
			{
				ExecuteAgain();
			}
		}

		public void ExecuteAgain()
		{
			isOneTimeFunctionCalled = true;

			if (isDebugLogging)
			{
				DebugPrinter.Print(description);
			}

			OnPressOneTimeFunctionKey.Invoke();
		}
	}
}