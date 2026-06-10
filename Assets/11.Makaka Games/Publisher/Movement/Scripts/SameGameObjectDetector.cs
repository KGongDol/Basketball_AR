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
using System.Collections.Generic;

using MakakaGames.Publisher.Debugging;

namespace MakakaGames.Publisher.Movement
{
    [HelpURL("https://makaka.org/unity-assets")]
    public class SameGameObjectDetector : MonoBehaviour
    {
        [SerializeField]
        private string nameForLog;

        [SerializeField]
        private bool isDebugLogging = false;

        [Space]
        [Tooltip("During this time, the object is considered the same.")]
        [SerializeField]
        [Range(0f, 20f)]
        private float safeTime = 3f;
        private float currentTime;
        private float previousTime;

        private Dictionary<GameObject, float> objectsWithTimeStaps;
        private GameObject currentObject;

        private event Action OnInitialized;

        public void Init(int countOfObjectsForInteraction, Action OnInitialized)
        {
            this.OnInitialized += OnInitialized;

            // Memory Allocation before Game Start
            objectsWithTimeStaps =
                new Dictionary<GameObject, float>(countOfObjectsForInteraction);

            this.OnInitialized?.Invoke();
        }

        /// <summary>
        /// Register - Memorize the Time.
        /// </summary>
        public bool? DetectOrRegister(GameObject other)
        {
            if (objectsWithTimeStaps != null)
            {
                currentTime = Time.time;
                currentObject = other;

                if (!objectsWithTimeStaps.TryGetValue(
                    currentObject, out previousTime))
                {
                    if (isDebugLogging)
                    {
                        DebugPrinter.Print("DetectOrRegister1 - Registration: "
                            + nameForLog);
                    }

                    objectsWithTimeStaps.Add(currentObject, currentTime);
                }

                //DebugPrinter.Print(
                //    $"{currentTime} - {previousTime} > {safeTime}");

                if (currentTime - previousTime > safeTime || previousTime < 0.001f)
                {
                    if (isDebugLogging)
                    {
                        DebugPrinter.Print("DetectOrRegister2 - from: "
                            + nameForLog + " - Diff Objects or Registration");
                    }

                    objectsWithTimeStaps[currentObject] = currentTime;

                    return false;
                }
                else
                {
                    if (isDebugLogging)
                    {
                        DebugPrinter.Print("DetectOrRegister2 - from: "
                            + nameForLog + " - Same Objects");
                    }

                    return true;
                }
            }
            else
            {
                if (isDebugLogging)
                {
                    DebugPrinter.Print("DetectOrRegisterX - from: "
                        + nameForLog + " - Not Initialized!");
                }

                return null;
            }
        }
    }
}