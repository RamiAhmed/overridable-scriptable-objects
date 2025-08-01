using OverridableScriptableObjects.Runtime;
using UnityEngine;

namespace OverridableScriptableObjects.Samples.Basic
{
    [CreateAssetMenu(fileName = "ScriptableObjectTest", menuName = "OverridableScriptableObjects/ScriptableObjectTest")]
    public class ScriptableObjectTest : OverridableScriptableObject
    {
        public string TestString = "ABC";
        public float TestFloat = 123.456f;
        public int TestInt = 789;
    }
}