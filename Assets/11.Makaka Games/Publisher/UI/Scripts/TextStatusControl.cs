/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

using System.Collections;

using UnityEngine;

using TMPro;

namespace MakakaGames.Publisher.UI
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class TextStatusControl : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        public TextMeshProUGUI text;

        public float clearingDelayDefault = 3f;

        private Coroutine showCoroutineLast;

        public void Show(string status)
        {
            if (showCoroutineLast != null)
            {
                StopCoroutine(showCoroutineLast);
            }

            showCoroutineLast = StartCoroutine(ShowCoroutine(status));
        }

        public void ShowAndClearWithDelay(string status, float delay)
        {
            if (showCoroutineLast != null)
            {
                StopCoroutine(showCoroutineLast);
            }

            showCoroutineLast = StartCoroutine(
                ShowAndClearWithDelayCoroutine(status, delay));
        }

        public void ShowAndClearWithDelayDefault(string status)
        {
            ShowAndClearWithDelay(status, clearingDelayDefault);
        }

        public void ShowAndClearWithDelay6(string status)
        {
            ShowAndClearWithDelay(status, 6f);
        }

        private IEnumerator ShowAndClearWithDelayCoroutine(string status, float delay)
        {
            yield return StartCoroutine(ShowCoroutine(status));

            yield return new WaitForSeconds(delay);

            Clear();
        }

        private IEnumerator ShowCoroutine(string status)
        {
            text.text = status;

            if (canvasGroup)
            {
                canvasGroup.alpha = 1f;
            }

            yield return null;
        }

        public void Clear()
        {
            //DebugPrinter.Print("Text Status > Clear");

            text.text = string.Empty;

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
            }
        }
    }
}