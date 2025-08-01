using System;
using System.IO;
using UnityEngine;

namespace OverridableScriptableObjects.Runtime
{
    public static class OverridableScriptableObjectUtil
    {
        private const string OverridesFolder = "Overrides";

        public static bool ExistsOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Exists(scriptableObject.GetType());
        }

        public static bool Exists(Type type)
        {
            return Exists(GetOverridesPath(type));
        }

        public static bool DeleteOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Delete(scriptableObject.GetType());
        }

        public static bool Delete(Type type)
        {
            var overridesPath = GetOverridesPath(type);
            if (!Exists(overridesPath))
                return false;

            File.Delete(overridesPath);
            return true;
        }

        public static bool SaveOverride<T>(
            this T scriptableObject,
            bool overrideIfExists = true,
            bool prettyPrintJson = true)
            where T : OverridableScriptableObject
        {
            return Save(scriptableObject, overrideIfExists, prettyPrintJson);
        }

        public static bool Save(
            OverridableScriptableObject scriptableObject,
            bool overrideIfExists = true,
            bool prettyPrintJson = true)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            var type = scriptableObject.GetType();

            var overridesPath = GetOverridesPath(type);
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

        public static bool LoadOverride<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return Load(scriptableObject);
        }

        public static bool Load(OverridableScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                throw new ArgumentNullException(nameof(scriptableObject));

            var type = scriptableObject.GetType();

            var overridesPath = GetOverridesPath(type);
            if (!Exists(overridesPath))
                return false;

            var json = File.ReadAllText(overridesPath);
            if (string.IsNullOrEmpty(json))
                return false;

            var dataType = GetSerializableDataType(type);
            if (JsonUtility.FromJson(json, dataType) is not ISerializableOverridableScriptableObject data)
                return false;

            data.ApplyTo(scriptableObject);
            return true;
        }

        public static string GetOverridesPath<T>(this T scriptableObject) where T : OverridableScriptableObject
        {
            return GetOverridesPath(scriptableObject.GetType());
        }

        public static string GetOverridesPath(Type type)
        {
            return Path.Combine(GetOverridesDirectoryPath(), type.Name + ".json");
        }

        private static string GetOverridesDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, OverridesFolder);
        }

        private static bool Exists(string path)
        {
            return File.Exists(path);
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