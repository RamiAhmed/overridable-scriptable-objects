# Overridable Scriptable Objects for Unity

Easily override ScriptableObject values at runtime in your Unity project by storing JSON files in the persistent data folder.

Have you found yourself needing to change values of ScriptableObjects at runtime, but didn't want to modify the original asset and make a new build?
I wanted to be able to override our configurations (stored as ScriptableObjects) at runtime, without having to create a new build every time.
This package provides a simple way to create overridable ScriptableObjects that can be saved, loaded, and deleted at runtime, using JSON serialization.

## Installation

The minimum required Unity version is `6000.0.53f1` (may work in older versions, but untested).

1. **Install via Git, Clone or Download**  
You can install the package by referencing the Git URL (**recommended** for package manager):

   ``https://github.com/RamiAhmed/overridable-scriptable-objects.git``

   Add this URL to your Unity Package Manager by going to `Window > Package Manager`, clicking the `+` button, and selecting `Add package from git URL...`.

   Alternatively, clone this repository or download the source code into your Unity project's `Assets` folder.

2. **Dependencies**  
   No external dependencies required. Works with Unity's built-in serialization (`JsonUtility`).

3. **Samples**  
   Sample usage is provided in the `Samples` folder, which may be separately imported. 
It's a very basic sample meant to get you started quickly.

## Usage Guide

### 1. Create an Overridable Scriptable Object

Inherit from `OverridableScriptableObject` for any ScriptableObject you want to be overridable.

```csharp
[CreateAssetMenu(...)]
public class MyConfig : OverridableScriptableObject
{
    public int Value;
    public string Name;
}
```

A serializable representation of your ScriptableObject will be automatically created, and named `{ScriptableObjectTypeName}_GeneratedSerializableData`, e.g. `MyConfig_GeneratedSerializableData`. 
In general you will not need to interact with this class directly, as the `OverridableScriptableObject` handles serialization and deserialization, through the `OverridableScriptableObjectUtil`.

**Note**: Only `public` and `internal` fields are serialized. Private fields will not be included in the override, not even if they are marked with `[SerializeField]`.

Scriptable object overrides are stored in the persistent data path, which is outside the Unity project folder. This allows for runtime modifications without affecting the original asset, including for builds.
The exact path is:  
`{Application.persistentDataPath}/Overrides/{YourScriptableObjectName}.json`.

### 2. Editor Actions

In the Unity Editor, select your ScriptableObject asset.  
Use the custom inspector panel to:

- **Save As Override**: Save current values as a JSON override to persistent data folder.
- **Load From Override**: Load values from the override file in the persistent data folder.
- **Show File in Explorer**: Open the override file location in your default file explorer.
- **Delete Override**: Permanently delete the override file from your file system.

### 3. Runtime API

You can manage overrides via code:

```csharp
var config = ...; // your OverridableScriptableObject instance

// Save override
config.SaveOverride();

// Load override
config = config.LoadOverride();

// Check if override exists
bool exists = config.ExistsOverride();

// Delete override
config.DeleteOverride();
```
The idea is to call `LoadOverride()` at the start of your game or scene to apply any overrides, and before any usage of the scriptable object.

### 4. Example Usage

A typical workflow would look something like this:
- You made a build of your game that you want to do some kind of testing on.
- You want to change some configuration values for the build without having to rebuild the whole game or make permanent changes to your project.
- So you load up the editor, find the overridable scriptable object that you want to change, and click the "Save As Override" button in the inspector.
- This will create a JSON file in the persistent data path with the current values. 
  - You can also create this file manually, it's just easier to do it through the editor.
- Open the JSON file in your favorite text editor, modify the values as needed, and save it.
- Now when you run your game, it will load the override values from the JSON file instead of the original ScriptableObject asset in the project (assuming your code calls `LoadOverride()` prior to any usage).

```csharp
public class GameManager : MonoBehaviour
{
    public MyConfig GameConfig;
    
    void Start()
    {
        // Load override at runtime
        GameConfig = GameConfig.LoadOverride();

        // Use the config values
        Debug.Log($"Game Name: {GameConfig.Name}, Value: {GameConfig.Value}");
    }
}
```

## Notes

- Overrides are stored per device, outside the Unity project, in the persistent data path.
- Useful for runtime configuration overrides or user settings.
- Complex Unity types (like `GameObject`, `Transform`, etc.) are not supported in overrides. Only [JSON-serializable types](https://docs.unity3d.com/ScriptReference/JsonUtility.html) are supported.


## License

MIT
