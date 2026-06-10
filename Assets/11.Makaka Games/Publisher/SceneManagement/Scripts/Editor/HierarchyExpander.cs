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
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.SceneManagement;

namespace MakakaGames.Publisher.SceneManagement.Editor
{
    [InitializeOnLoad]
    public static class HierarchyExpandOnLoad
    {
        private const int MAX_DEPTH = 2;

        static HierarchyExpandOnLoad()
        {
            Init();
        }

        private static void Init()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EditorApplication.delayCall += () => ExpandAllRootObjects(scene);
        }
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EditorApplication.delayCall += () => ExpandAllRootObjects(scene);
        }

        private static void ExpandAllRootObjects(Scene scene)
        {
            if (scene.IsValid())
            {
                GameObject[] rootGameObjects = scene.GetRootGameObjects();

                foreach (var gameObject in rootGameObjects)
                {
                    ExpandRecursively(gameObject, 0);
                }

                // -------------------------------------------------
                // Persistent Objects like "DontDestroyOnLoad" Scene
                // -------------------------------------------------

                GameObject[] persistentObjects = 
                    Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

                foreach (GameObject persistentObject in persistentObjects)
                {
                    // Skip scene objects since we already expanded them
                    if (!persistentObject.scene.isLoaded)
                    {
                        // Start recursion for DontDestroyOnLoad objects
                        ExpandRecursively(persistentObject, 0);
                    }
                }
            }
        }

        private static void ExpandRecursively(GameObject gameObject, int currentDepth)
        {
            if (currentDepth > MAX_DEPTH)
            {
                return;
            }

            // Ping the object to expand it in the Hierarchy
            EditorGUIUtility.PingObject(gameObject);

            foreach (Transform child in gameObject.transform)
            {
                ExpandRecursively(child.gameObject, currentDepth + 1);
            }
        }
    }
}