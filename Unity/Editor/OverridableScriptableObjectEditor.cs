using OverridableScriptableObjects.Runtime;
using UnityEditor;
using UnityEngine;

namespace OverridableScriptableObjects.Editor
{
    [CustomEditor(typeof(OverridableScriptableObject), true)]
    public class OverridableScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawActionsPanel();
            EditorGUILayout.Separator();
            base.OnInspectorGUI();
        }

        private void DrawActionsPanel()
        {
            using var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Overridable Scriptable Object Actions", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            using var horizontalScope = new EditorGUILayout.HorizontalScope();
            var t = (OverridableScriptableObject)target;

            if (GUILayout.Button("Save As Override", EditorStyles.miniButtonLeft))
            {
                if (t.SaveOverride())
                    Debug.Log($"Saved override for {t.name}");
                else
                    Debug.LogWarning($"Failed to save override for {t.name}");
            }

            using var disabledScope = new EditorGUI.DisabledScope(!t.ExistsOverride());

            if (GUILayout.Button("Load From Override", EditorStyles.miniButtonMid))
            {
                if (t.LoadOverride())
                    Debug.Log($"Loaded override for {t.name}");
                else
                    Debug.LogWarning($"Failed to load override for {t.name}");
            }

            if (GUILayout.Button("Show File in Explorer", EditorStyles.miniButtonMid))
                EditorUtility.RevealInFinder(t.GetOverridesPath());

            if (GUILayout.Button("Delete Override", EditorStyles.miniButtonRight))
            {
                if (!EditorUtility.DisplayDialog("Confirmation",
                        $"Are you sure you want to delete the override for '{t.name}'?", "Yes", "Cancel"))
                    return;

                if (t.DeleteOverride())
                    Debug.Log($"Deleted override for {t.name}");
                else
                    Debug.LogWarning($"Failed to delete override for {t.name}");
            }
        }
    }
}