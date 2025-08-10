using System.Linq;
using OverridableScriptableObjects.Runtime;
using UnityEditor;
using UnityEngine;

namespace OverridableScriptableObjects.Editor
{
    /// <summary>
    ///     Custom editor for <see cref="OverridableScriptableObject" />.
    ///     This editor provides a panel with buttons to save, load, show, and delete overrides for the scriptable object.
    ///     It also displays the default inspector for the scriptable object, allowing users to edit its properties.
    ///     The override file is stored in the persistent data path of the application (see
    ///     <see cref="Application.persistentDataPath" />).
    /// </summary>
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

            var t = (OverridableScriptableObject)target;

            var savePath = t.GetOverrideFilePath();
            if (string.IsNullOrEmpty(savePath))
                savePath = OverridableScriptableObjectUtil.GetTargetFilePaths(t.name).FirstOrDefault();

            EditorGUILayout.LabelField(new GUIContent("Override Scriptable Object Path", savePath),
                new GUIContent(savePath, savePath));
            EditorGUILayout.Separator();

            using var horizontalScope = new EditorGUILayout.HorizontalScope();

            if (GUILayout.Button("Save As Override", EditorStyles.miniButtonLeft))
            {
                if (t.SaveOverride())
                    Debug.Log($"Saved override for {t.name} successfully");
                else
                    Debug.LogWarning($"Failed to save override for {t.name}");
            }

            using var disabledScope = new EditorGUI.DisabledScope(!t.ExistsOverride());

            if (GUILayout.Button("Load From Override", EditorStyles.miniButtonMid))
            {
                if (t.LoadOverride() != null)
                    Debug.Log($"Loaded override for {t.name} successfully");
                else
                    Debug.LogWarning($"Failed to load override for {t.name}");
            }

            if (GUILayout.Button("Show File in Explorer", EditorStyles.miniButtonMid))
                EditorUtility.RevealInFinder(t.GetOverrideFilePath());

            if (GUILayout.Button("Delete Override", EditorStyles.miniButtonRight))
            {
                if (!EditorUtility.DisplayDialog("Confirmation",
                        $"Are you sure you want to delete the override for '{t.name}'?", "Yes", "Cancel"))
                    return;

                if (t.DeleteOverride())
                    Debug.Log($"Deleted override for {t.name} successfully");
                else
                    Debug.LogWarning($"Failed to delete override for {t.name}");
            }
        }
    }
}