using UnityEngine;

namespace OverridableScriptableObjects
{
    public class TestBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            var output = "Test";
            output = GetStringFromSourceGenerator();
            Debug.Log(output);
        }

        private static string GetStringFromSourceGenerator()
        {
            return ExampleSourceGenerated.ExampleSourceGenerated.GetTestText();
        }
    }
}