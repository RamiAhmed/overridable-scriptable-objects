namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Interface for the generated, serializable representation of an overridable scriptable object. 
    /// </summary>
    public interface ISerializableOverridableScriptableObject
    {
        /// <summary>
        ///     Applies the values from this serializable object to the target overridable scriptable object.
        /// </summary>
        /// <param name="target">The target overridable scriptable object to apply values to.</param>
        void ApplyTo(OverridableScriptableObject target);
        
        /// <summary>
        ///     Copies the values from another overridable scriptable object into this serializable object.
        /// </summary>
        /// <param name="source">The source overridable scriptable object to copy values from.</param>
        void CopyFrom(OverridableScriptableObject source);
    }
}