namespace OverridableScriptableObjects.Runtime
{
    public interface ISerializableOverridableScriptableObject
    {
        void ApplyTo(OverridableScriptableObject target);
        void CopyFrom(OverridableScriptableObject source);
    }
}