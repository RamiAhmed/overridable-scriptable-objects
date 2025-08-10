using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Utility & extension class for managing overrides of scriptable objects (<see cref="OverridableScriptableObject" />
    ///     ).
    /// </summary>
    public static class OverridableScriptableObjectUtil
    {
        private static OverridableScriptableObjectConfiguration _configuration;

        private static OverridableScriptableObjectConfiguration Configuration
        {
            get
            {
                if (_configuration != null)
                    return _configuration;

                _configuration = Resources.Load<OverridableScriptableObjectConfiguration>(
                    nameof(OverridableScriptableObjectConfiguration));

                // If no configuration is found in Resources, create a default one.
                if (_configuration == null)
                {
                    _configuration = ScriptableObject.CreateInstance<OverridableScriptableObjectConfiguration>();
                    _configuration.ApplyDefaults();
                }

                if (_configuration.ExistsOverride())
                    _configuration = _configuration.LoadOverride();

                return _configuration;
            }
        }

        /// <summary>
        ///     Checks if a scriptable object override exists for the given scriptable object.
        /// </summary>
        /// <param name="scriptableObject">Scriptable object to check</param>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <returns>True if the override file exists, otherwise false.</returns>
        public static bool ExistsOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Exists(scriptableObject.name, out _);
        }

        /// <summary>
        ///     Checks if an override file exists for the given type.
        /// </summary>
        /// <param name="fileName">Name of the scriptable object</param>
        /// <param name="filePath">
        ///     The override file path for the scriptable object with the given <paramref name="fileName" />, if
        ///     one exists.
        /// </param>
        /// <returns>True if the override file exists, otherwise false.</returns>
        public static bool Exists(string fileName, out string filePath)
        {
            filePath = GetOverrideFilePath(fileName);
            return !string.IsNullOrEmpty(filePath);
        }

        /// <summary>
        ///     Deletes the override file for the given scriptable object.
        /// </summary>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <param name="scriptableObject">Scriptable object to delete override for</param>
        /// <returns>True if the override was deleted, otherwise false.</returns>
        public static bool DeleteOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Delete(scriptableObject.name);
        }

        /// <summary>
        ///     Deletes the override file for the given type.
        /// </summary>
        /// <param name="fileName">Name of the scriptable object</param>
        /// <returns>True if the override was deleted, otherwise false.</returns>
        public static bool Delete(string fileName)
        {
            if (!Exists(fileName, out var overridesPath))
                return false;

            File.Delete(overridesPath);
            return true;
        }

        /// <summary>
        ///     Saves the override for the given scriptable object.
        /// </summary>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <param name="scriptableObject">Scriptable object to save</param>
        /// <param name="overrideIfExists">Whether to override if the file exists</param>
        /// <returns>True if the override was saved, otherwise false.</returns>
        public static bool SaveOverride<T>(
            this T scriptableObject,
            bool overrideIfExists = true)
            where T : OverridableScriptableObject
        {
            return Save(scriptableObject, overrideIfExists);
        }

        /// <summary>
        ///     Saves the override for the given scriptable object instance.
        /// </summary>
        /// <param name="scriptableObject">Scriptable object to save</param>
        /// <param name="overrideIfExists">Whether to override if the file exists</param>
        /// <returns>True if the override was saved, otherwise false.</returns>
        public static bool Save(
            OverridableScriptableObject scriptableObject,
            bool overrideIfExists = true)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            var overrideExists = Exists(scriptableObject.name, out var overridesPath);
            if (!overrideIfExists && overrideExists)
                return false;

            if (!overrideExists)
            {
                var overridesDirectoryPath = GetTargetDirectoryPaths().FirstOrDefault();
                if (string.IsNullOrEmpty(overridesDirectoryPath))
                    return false;

                if (!Directory.Exists(overridesDirectoryPath))
                    Directory.CreateDirectory(overridesDirectoryPath);

                overridesPath = GetTargetFilePaths(scriptableObject.name).First();
            }

            var dataType = GetSerializableDataType(scriptableObject.GetType());
            var data = (ISerializableOverridableScriptableObject)Activator.CreateInstance(dataType);
            data.CopyFrom(scriptableObject);

            var json = JsonUtility.ToJson(data, Configuration.PrettyPrintJson);
            if (string.IsNullOrEmpty(json))
                return false;

            File.WriteAllText(overridesPath, json);
            return true;
        }

        /// <summary>
        ///     Loads the override for the given scriptable object and applies it.
        /// </summary>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <param name="scriptableObject">Scriptable object to load override for</param>
        /// <returns>The scriptable object with override applied, or null if not found.</returns>
        public static T LoadOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return (T)Load(scriptableObject);
        }

        /// <summary>
        ///     Loads the override for the given scriptable object and applies it.
        /// </summary>
        /// <param name="scriptableObject">Scriptable object to load override for</param>
        /// <returns>The scriptable object with override applied, or null if not found.</returns>
        public static OverridableScriptableObject Load(OverridableScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            if (!Exists(scriptableObject.name, out var overridesPath))
                return null;

            var json = File.ReadAllText(overridesPath);
            if (string.IsNullOrEmpty(json))
                return null;

            var dataType = GetSerializableDataType(scriptableObject.GetType());
            if (JsonUtility.FromJson(json, dataType) is not ISerializableOverridableScriptableObject data)
                return null;

#if UNITY_EDITOR
            // In the editor we instantiate the scriptable object to avoid changing the original asset's values.
            // In a build this is not a problem since the scriptable object edits won't persist beyond that session.
            scriptableObject = Object.Instantiate(scriptableObject);
#endif

            data.ApplyTo(scriptableObject);
            return scriptableObject;
        }

        /// <summary>
        ///     Gets the path to the override file for the given scriptable object.
        /// </summary>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <param name="scriptableObject">Scriptable object to get path for</param>
        /// <returns>Path to the override file.</returns>
        public static string GetOverrideFilePath<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return GetOverrideFilePath(scriptableObject.name);
        }

        /// <summary>
        ///     Gets the path to the override file for the given type.
        /// </summary>
        /// <param name="fileName">Name of the scriptable object</param>
        /// <returns>Path to the override file.</returns>
        public static string GetOverrideFilePath(string fileName)
        {
            return GetTargetFilePaths(fileName)
                .Where(File.Exists)
                .FirstOrDefault();
        }

        public static IEnumerable<string> GetTargetFilePaths(string fileName)
        {
            var fullFileName = GetFileName(fileName);
            return GetTargetDirectoryPaths()
                .Select(directoryPath => Path.Combine(directoryPath, fullFileName));
        }

        private static IEnumerable<string> GetTargetDirectoryPaths()
        {
            if (Configuration == null)
                throw new InvalidOperationException(
                    $"{nameof(OverridableScriptableObjectConfiguration)} not initialized correctly. " +
                    "Make sure a configuration exists in Resources.");

            if (Configuration.OverridePaths == null || Configuration.OverridePaths.Length == 0)
                throw new InvalidOperationException(
                    $"{nameof(OverridableScriptableObjectConfiguration)} has no override paths configured. " +
                    "Please configure at least one override path in the configuration asset.");

            foreach (var overridePath in Configuration.OverridePaths)
                yield return overridePath.GetFullPath(Configuration.OverridesFolder);
        }

        private static string GetFileName(string fileName)
        {
            return $"{fileName}.json";
        }

        private static Type GetSerializableDataType(Type type)
        {
            var expectedTypeName = $"{type.Namespace}.{type.Name}_GeneratedSerializableData";
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var dataType = assembly.GetType(expectedTypeName);
                if (dataType != null)
                    return dataType;
            }

            throw new ArgumentException(
                $"Generated serializable data type '{expectedTypeName}' not found. Ensure that it has public or internal serializable, non-UnityEngine.Object fields.",
                nameof(type));
        }
    }
}