# Overridable Scriptable Objects for Unity

Easily override Unity `ScriptableObject` values at runtime in your Unity project by storing JSON files in configurable locations.

_Have you found yourself needing to change values of scriptable objects at runtime, but didn't want to modify the original asset or make a new build?
I wanted to be able to override our configurations (stored as ScriptableObjects) at runtime, without having to create a new build every time.
This package provides a simple way to create overridable ScriptableObjects that can be saved, loaded, and deleted at runtime, using Unity's built-in JSON serialization._


---

## Features
- Override ScriptableObject values at runtime without rebuilding your project.
- Flexible configuration asset for override management, including hierarchical and custom paths.
- Editor integration for saving, loading, showing, and deleting overrides.
- Supports multiple prioritized override paths.
- `IgnoreOverridableFieldAttribute` to exclude fields from override serialization.
- Works with Unity's built-in JSON serialization.

---

## Installation

Requires Unity `6000.0.53f1` or newer (may work in older versions, but untested).

1. **Install via Git, Clone or Download**
   - Add the package via Git URL: `https://github.com/RamiAhmed/overridable-scriptable-objects.git` in Unity Package Manager.
   - Or clone/download into your project's `Assets` folder.
2. **Dependencies**
   - No external dependencies. Uses Unity's built-in serialization (`JsonUtility`).
   - Relies on [incremental source generation](https://docs.unity3d.com/6000.0/Documentation/Manual/create-source-generator.html).
3. **Samples**
   - Basic sample included in the `Samples` folder.

---

## Configuration

The system uses a configuration asset (`OverridableScriptableObjectConfiguration`) to control how and where overrides are stored. Create this asset via `Create > OverridableScriptableObjects > Configuration` in the Unity menu. Place it in the `Resources` folder for runtime loading.

If no configuration asset is found, default settings are used. The configuration asset itself is overridable.

### Override Path Hierarchy

You can specify multiple override paths in the configuration. The system checks each path in order and uses the first one that exists. Supported path types:

- **PersistentDataPath**: [`Application.persistentDataPath`](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) + OverridesFolder  
  (per-device storage outside the project directory)
- **StreamingAssetsPath**: [`Application.streamingAssetsPath`](https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html) + OverridesFolder  
  (read-only data bundled with the build)
- **ExecutablePath**: [`Application.dataPath`](https://docs.unity3d.com/ScriptReference/Application-dataPath.html) + OverridesFolder  
  (local overrides near the game binary)
- **CustomPath**: [`Environment.CurrentDirectory`](https://learn.microsoft.com/en-us/dotnet/api/system.environment.currentdirectory) + CustomPath  
  (flexible override storage anywhere on disk)

The first valid path is used for all override operations. You must specify at least one path. Custom paths are relative to the current execution directory, not absolute (e.g. not `C:/`).

#### Examples
- `OverridePaths = [PersistentDataPath, StreamingAssetsPath]` and `OverridesFolder = "Overrides"` will check `Application.persistentDataPath/Overrides` first, then `Application.streamingAssetsPath/Overrides`.
- `OverridePaths = [CustomPath]` with `Path = "MyOverrides"` will use `{currentExecutionDirectory}/MyOverrides`.
- Multiple custom paths: `OverridePaths = [CustomPath, CustomPath, PersistentDataPath]` with `Path = "MyOverrides"` and `Path = "SharedOverrides"` will check `{currentExecutionDirectory}/MyOverrides`, then `{currentExecutionDirectory}/SharedOverrides`, then `Application.persistentDataPath/Overrides`.
- Organize overrides in subfolders: `Path = "Config/Local"` and `Path = "Config/Shared"` for `{currentExecutionDirectory}/Config/Local` and `{currentExecutionDirectory}/Config/Shared`.

Set the overrides folder name in the configuration. Use the inspector to configure; no code changes required.

Default configuration can be applied via the inspector's "Reset" option or by calling `ApplyDefaults()` programmatically.

---

## Usage Guide

### 1. Create an Overridable Scriptable Object

If using assembly definitions, add a reference to `OverridableScriptableObjects.Runtime`.

Inherit from `OverridableScriptableObject`:

```csharp
[CreateAssetMenu(...)]
public class MyConfig : OverridableScriptableObject
{
    public int Value;
    public string Name;
}
```

A serializable representation is auto-generated as `{ScriptableObjectTypeName}_GeneratedSerializableData`. In general you will not need to interact with this class directly, as the `OverridableScriptableObject` handles serialization and deserialization, through the `OverridableScriptableObjectUtil`.

**Note:** Only `public` and `internal` fields are serialized. Private fields are not included, even if marked `[SerializeField]`.

### 2. Editor Actions

Select your ScriptableObject asset in the Unity Editor. Use the custom inspector panel to:
- **Save As Override**: Save current values as a JSON override.
- **Load From Override**: Load values from the override file.
- **Show File in Explorer**: Open the override file location.
- **Delete Override**: Permanently delete the override file.

### 3. Runtime API

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

Call `LoadOverride()` at the start of your game/scene to apply overrides. Handle the case where no override exists, as it may return `null`.

### 4. Ignore Overridable Field Attribute

Exclude fields from override serialization by marking them with `IgnoreOverridableFieldAttribute`:

```csharp
[IgnoreOverridableField]
public int InternalValue;
```

Fields with this attribute are not included in override JSON, but remain in Unity serialization (e.g. inspector).

### 5. Example Usage

```csharp
public class GameManager : MonoBehaviour
{
    public MyConfig GameConfig;
    private void Start()
    {
        if (GameConfig.ExistsOverride())
            GameConfig = GameConfig.LoadOverride();
        Debug.Log($"Game Name: {GameConfig.Name}, Value: {GameConfig.Value}");
    }
}
```

---

## Notes
- Overrides are stored per device, outside the Unity project, in the configured path(s).
- Complex Unity types (e.g. `GameObject`, `Transform`) are not supported in overrides. Only [JSON-serializable types](https://docs.unity3d.com/ScriptReference/JsonUtility.html) are included.
- Designed for runtime overrides; editor actions are for convenience.

---

## License
MIT
