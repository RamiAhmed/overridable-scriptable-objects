using System;
using UnityEditor;
using UnityEngine;

namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Configuration for the Overridable Scriptable Objects system.
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(OverridableScriptableObjectConfiguration) + ".asset",
        menuName = "OverridableScriptableObjects/Configuration")]
    public class OverridableScriptableObjectConfiguration : OverridableScriptableObject
    {
        public enum OverridePathType
        {
            PersistentDataPath,
            StreamingAssetsPath,
            ExecutablePath,
            CustomPath
        }

        /// <summary>
        ///     Name of the appendix that will be added to the type name of generated serializable data classes.
        ///     This is hardcoded in the source generator and should not be changed.
        /// </summary>
        public const string SerializeTypeNameAppendix = "_GeneratedSerializableData";

        [Tooltip(
            "If true, the JSON files will be formatted with indentation for easier readability. This will increase file size but makes it easier to read and debug.")]
        public bool PrettyPrintJson;

        [Space]
        [Tooltip(
            "Folder name where overrides are stored. This folder will be created in the override paths if it does not exist.")]
        public string OverridesFolder;

        [Tooltip("Prioritized list of paths to look for overrides in. The first path that exists will be used.")]
        public OverridePath[] OverridePaths;

        private void Reset()
        {
            ApplyDefaults();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR

            var assetPath = AssetDatabase.GetAssetPath(this);
            if (!assetPath.Contains(nameof(Resources), StringComparison.Ordinal))
                Debug.LogWarning(
                    $"{this} must be placed in a {nameof(Resources)} folder to ensure it is loaded correctly at runtime. Move it to '{nameof(Resources)}/{nameof(OverridableScriptableObjectConfiguration)}.asset', please.");

#endif
        }

        public void ApplyDefaults()
        {
            PrettyPrintJson = true;
            OverridesFolder = "Overrides";
            OverridePaths = new[]
            {
                new OverridePath
                {
                    PathType = OverridePathType.PersistentDataPath
                },
                new OverridePath
                {
                    PathType = OverridePathType.StreamingAssetsPath
                },
                new OverridePath
                {
                    PathType = OverridePathType.ExecutablePath
                }
            };
        }

        [Serializable]
        public class OverridePath
        {
            public string Path;
            public OverridePathType PathType;

            public string GetFullPath(string overridesFolder)
            {
                return PathType switch
                {
                    OverridePathType.PersistentDataPath => System.IO.Path.Combine(Application.persistentDataPath,
                        overridesFolder),
                    OverridePathType.StreamingAssetsPath => System.IO.Path.Combine(Application.streamingAssetsPath,
                        overridesFolder),
                    OverridePathType.ExecutablePath => System.IO.Path.Combine(Application.dataPath, overridesFolder),
                    OverridePathType.CustomPath => System.IO.Path.Combine(Environment.CurrentDirectory, Path),
                    _ => throw new ArgumentOutOfRangeException(nameof(PathType), PathType, "Path type not supported")
                };
            }
        }
    }
}