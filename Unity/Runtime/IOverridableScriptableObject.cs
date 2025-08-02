using UnityEngine;

namespace OverridableScriptableObjects.Runtime
{
    /// <summary>
    ///     Base class for scriptable objects that can be overridden.
    ///     This class should be inherited by any scriptable object that needs to support overrides.
    ///     It is used to mark scriptable objects that can have their values overridden by a JSON file in the persistent data
    ///     folder (<see cref="Application.persistentDataPath"/>).
    /// </summary>
    public abstract class OverridableScriptableObject : ScriptableObject
    {
    }
}