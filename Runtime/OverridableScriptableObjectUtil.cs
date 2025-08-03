using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Utility & extension class for managing overrides of scriptable objects (<see cref="OverridableScriptableObject"/>). 
    /// </summary>
    public static class OverridableScriptableObjectUtil
    {
        private const string OverridesFolder = "Overrides";

        /// <summary>
        ///     Checks if a scriptable object override exists for the given scriptable object.
        /// </summary>
        /// <param name="scriptableObject">Scriptable object to check</param>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <returns>True if the override file exists, otherwise false.</returns>
        public static bool ExistsOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Exists(scriptableObject.GetType());
        }

        /// <summary>
        ///     Checks if an override file exists for the given type.
        /// </summary>
        /// <param name="type">Type of the scriptable object</param>
        /// <returns>True if the override file exists, otherwise false.</returns>
        public static bool Exists(Type type)
        {
            return Exists(GetOverrideFilePath(type));
        }

        /// <summary>
        ///     Deletes the override file for the given scriptable object.
        /// </summary>
        /// <typeparam name="T">Type of scriptable object</typeparam>
        /// <param name="scriptableObject">Scriptable object to delete override for</param>
        /// <returns>True if the override was deleted, otherwise false.</returns>
        public static bool DeleteOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Delete(scriptableObject.GetType());
        }

        /// <summary>
        ///     Deletes the override file for the given type.
        /// </summary>
        /// <param name="type">Type of the scriptable object</param>
        /// <returns>True if the override was deleted, otherwise false.</returns>
        public static bool Delete(Type type)
        {
            var overridesPath = GetOverrideFilePath(type);
            if (!Exists(overridesPath))
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
        /// <param name="prettyPrintJson">Whether to pretty print the JSON</param>
        /// <returns>True if the override was saved, otherwise false.</returns>
        public static bool SaveOverride<T>(
            this T scriptableObject,
            bool overrideIfExists = true,
            bool prettyPrintJson = true)
            where T : OverridableScriptableObject
        {
            return Save(scriptableObject, overrideIfExists, prettyPrintJson);
        }

        /// <summary>
        ///     Saves the override for the given scriptable object instance.
        /// </summary>
        /// <param name="scriptableObject">Scriptable object to save</param>
        /// <param name="overrideIfExists">Whether to override if the file exists</param>
        /// <param name="prettyPrintJson">Whether to pretty print the JSON</param>
        /// <returns>True if the override was saved, otherwise false.</returns>
        public static bool Save(
            OverridableScriptableObject scriptableObject,
            bool overrideIfExists = true,
            bool prettyPrintJson = true)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            var type = scriptableObject.GetType();

            var overridesPath = GetOverrideFilePath(type);
            if (!overrideIfExists && Exists(overridesPath))
                return false;

            var overridesDirectoryPath = GetOverridesDirectoryPath();
            if (!Directory.Exists(overridesDirectoryPath))
                Directory.CreateDirectory(overridesDirectoryPath);

            var dataType = GetSerializableDataType(type);
            var data = (ISerializableOverridableScriptableObject)Activator.CreateInstance(dataType);
            data.CopyFrom(scriptableObject);

            var json = JsonUtility.ToJson(data, prettyPrintJson);
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

            var type = scriptableObject.GetType();

            var overridesPath = GetOverrideFilePath(type);
            if (!Exists(overridesPath))
                return null;

            var json = File.ReadAllText(overridesPath);
            if (string.IsNullOrEmpty(json))
                return null;

            var dataType = GetSerializableDataType(type);
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
            return GetOverrideFilePath(scriptableObject.GetType());
        }

        /// <summary>
        ///     Gets the path to the override file for the given type.
        /// </summary>
        /// <param name="type">Type of the scriptable object</param>
        /// <returns>Path to the override file.</returns>
        public static string GetOverrideFilePath(Type type)
        {
            return Path.Combine(GetOverridesDirectoryPath(), type.Name + ".json");
        }

        private static bool Exists(string path)
        {
            return File.Exists(path);
        }

        private static string GetOverridesDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, OverridesFolder);
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
                $"Generated serializable data type '{expectedTypeName}' not found. Ensure that SerializableObjectGenerator has run successfully.",
                nameof(type));
        }
    }
}