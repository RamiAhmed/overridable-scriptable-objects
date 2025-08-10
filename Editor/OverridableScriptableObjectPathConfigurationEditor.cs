using OverridableScriptableObjects.Runtime;
using UnityEditor;
using UnityEngine;

namespace OverridableScriptableObjects.Editor
{
    [CustomPropertyDrawer(typeof(OverridableScriptableObjectConfiguration.OverridePath))]
    public class OverridableScriptableObjectPathConfigurationEditor : PropertyDrawer
    {
        private SerializedProperty _path;
        private SerializedProperty _type;

        private void Initialize(SerializedProperty property)
        {
            _type = property.FindPropertyRelative(nameof(OverridableScriptableObjectConfiguration.OverridePath
                .PathType));
            _path = property.FindPropertyRelative(
                nameof(OverridableScriptableObjectConfiguration.OverridePath.Path));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            using var propertyScope = new EditorGUI.PropertyScope(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, _type, label);

            if (ShowPathField())
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, _path);
            }

            var target = (OverridableScriptableObjectConfiguration)property.serializedObject.targetObject;
            var t = (OverridableScriptableObjectConfiguration.OverridePath)property.boxedValue;
            var fullPath = t?.GetFullPath(target!.OverridesFolder) ?? "Not set";
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position = EditorGUI.PrefixLabel(position,
                new GUIContent("Full Path",
                    "Note: The path displayed here in the editor may not reflect the actual path in the file system in a build."));
            EditorGUI.SelectableLabel(position, fullPath);
        }

        private bool ShowPathField()
        {
            return _type.enumValueIndex == (int)OverridableScriptableObjectConfiguration.OverridePathType.CustomPath;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);

            var defaultHeight = base.GetPropertyHeight(property, label);
            if (ShowPathField())
                return defaultHeight * 3f + EditorGUIUtility.standardVerticalSpacing;

            return defaultHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}