using OverridableScriptableObjects.Runtime;
using UnityEngine;

namespace OverridableScriptableObjects.Samples.Basic
{
    public class ScriptableObjectBehaviourTest : MonoBehaviour
    {
        public ScriptableObjectTest ScriptableObject;

        private void Start()
        {
            var instance = ScriptableObject.LoadOverride();
            Debug.Log(
                $"ScriptableObjectTest values: {instance.TestString}, {instance.TestInt}, {instance.TestFloat}");
        }
    }
}