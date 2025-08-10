using System;

namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Skips the field when serializing the overridable scriptable object to JSON.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IgnoreOverridableFieldAttribute : Attribute
    {
    }
}