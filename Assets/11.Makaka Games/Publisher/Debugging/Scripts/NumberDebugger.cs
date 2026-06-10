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

namespace MakakaGames.Publisher.Debugging
{
  [HelpURL("https://makaka.org/unity-assets")]
  public class NumberDebugger
  {
    private float valuePreviousForDebugFloatAbsChanging;
    private int counterForDebugFloatAbsChanging;

      public void DebugFloatAbsChanging(float delta, float valueCurrent)
      {
      valueCurrent = Mathf.Abs(valueCurrent);

          if (Mathf.Abs(valuePreviousForDebugFloatAbsChanging - valueCurrent) > delta)
          {
              DebugPrinter.Print(
          counterForDebugFloatAbsChanging++ + ". "
          + "New = " + valueCurrent 
          + ", Old = " + valuePreviousForDebugFloatAbsChanging);
          }

      valuePreviousForDebugFloatAbsChanging = valueCurrent;
      }
  }
}
