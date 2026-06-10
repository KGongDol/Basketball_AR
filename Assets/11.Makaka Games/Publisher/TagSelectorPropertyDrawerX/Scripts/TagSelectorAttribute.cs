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

#if UNITY_EDITOR 

using UnityEditor;

#endif

namespace MakakaGames.Publisher.TagSelectorPropertyDrawerX
{
    public class TagSelectorAttribute : PropertyAttribute 
    { 
        public static string Untagged = "Untagged";
    }

#if UNITY_EDITOR 

    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(
            Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                property.stringValue =
                    EditorGUI.TagField(position, label, property.stringValue);
                
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }

#endif

}