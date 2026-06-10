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
using System.Collections;

namespace MakakaGames.Publisher.FPSController
{
    [Serializable]
    public class LerpControlledBob
    {
        public float BobDuration;
        public float BobAmount;

        private float m_Offset = 0f;

        // provides the offset that can be used
        public float Offset()
        {
            return m_Offset;
        }

        public IEnumerator DoBobCycle()
        {
            // make the camera move down slightly
            float t = 0f;

            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(0f, BobAmount, t / BobDuration);

                t += Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            // make it move back to neutral
            t = 0f;

            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(BobAmount, 0f, t / BobDuration);

                t += Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }

            m_Offset = 0f;
        }
    }
}