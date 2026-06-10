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
using UnityEditor;

namespace MakakaGames.Readme.Editor
{
    [CustomEditor(typeof(Readme))]
    [InitializeOnLoad]
    public class ReadmeEditor : UnityEditor.Editor
    {
        static readonly string s_ShowedReadmeSessionStateName =
            "ReadmeEditor.showedReadmeMakaka";
        
        static readonly string urlOnlineDocs = "https://makaka.org/unity-assets";
        static readonly string urlSupport = "https://makaka.org/support";

        const float k_Space = 16f;
        const float before_button_Space = 5f;

        static ReadmeEditor()
        {
            EditorApplication.delayCall += SelectReadmeAutomatically;
        }

        static void SelectReadmeAutomatically()
        {
            if (!SessionState.GetBool(s_ShowedReadmeSessionStateName, false))
            {
                SelectReadme();

                SessionState.SetBool(s_ShowedReadmeSessionStateName, true);
            }
        }

        [MenuItem("Window/Makaka Games/Offline Readme")]
        static Readme SelectReadme()
        {
            var ids = AssetDatabase.FindAssets("Readme t:Readme");
            if (ids.Length == 1)
            {
                var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

                Selection.objects = new UnityEngine.Object[] { readmeObject };

                return (Readme)readmeObject;
            }
            else
            {
                Debug.Log("Couldn't find a readme");
                return null;
            }
        }

        [MenuItem("Window/Makaka Games/Online Docs")]
        static void OpenURLOnlineDocs()
        {
            Application.OpenURL(urlOnlineDocs);
        }

        [MenuItem("Window/Makaka Games/Support")]
        static void OpenURLSupport()
        {
            Application.OpenURL(urlSupport);
        }

        protected override void OnHeaderGUI()
        {
            var readme = (Readme)target;
            Init();

            var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

            GUILayout.BeginHorizontal("In BigTitle");
            {
                if (readme.icon != null)
                {
                    GUILayout.Space(k_Space);
                    GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
                }
                GUILayout.Space(k_Space);
                GUILayout.BeginVertical();
                {

                    GUILayout.FlexibleSpace();
                    GUILayout.Label(readme.title, TitleStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            var readme = (Readme)target;
            Init();

            foreach (var section in readme.sections)
            {
                if (!string.IsNullOrEmpty(section.heading))
                {
                    GUILayout.Label(section.heading, HeadingStyle);
                }

                if (!string.IsNullOrEmpty(section.text))
                {
                    GUILayout.Label(section.text, BodyStyle);
                }

                GUILayout.Space(before_button_Space);

                if (!string.IsNullOrEmpty(section.linkText))
                {
                    if (GUILayout.Button(section.linkText, ButtonStyle))
                    {
                        Application.OpenURL(section.url);
                    }
                }

                GUILayout.Space(k_Space);
            }
        }

        bool m_Initialized;

        GUIStyle TitleStyle
        {
            get { return m_TitleStyle; }
        }

        [SerializeField]
        GUIStyle m_TitleStyle;

        GUIStyle HeadingStyle
        {
            get { return m_HeadingStyle; }
        }

        [SerializeField]
        GUIStyle m_HeadingStyle;

        GUIStyle BodyStyle
        {
            get { return m_BodyStyle; }
        }

        [SerializeField]
        GUIStyle m_BodyStyle;

        GUIStyle ButtonStyle
        {
            get { return m_ButtonStyle; }
        }

        [SerializeField]
        GUIStyle m_ButtonStyle;

        void Init()
        {
            if (m_Initialized)
                return;

            m_BodyStyle = new GUIStyle(EditorStyles.label);
            m_BodyStyle.wordWrap = true;
            m_BodyStyle.fontSize = 14;
            m_BodyStyle.richText = true;

            m_TitleStyle = new GUIStyle(m_BodyStyle);
            m_TitleStyle.fontSize = 26;

            m_HeadingStyle = new GUIStyle(m_BodyStyle);
            m_HeadingStyle.fontStyle = FontStyle.Bold;
            m_HeadingStyle.fontSize = 18;

            m_ButtonStyle = new GUIStyle(EditorStyles.miniButton);
            m_ButtonStyle.fontStyle = FontStyle.Bold;

            m_Initialized = true;
        }
    }
}
