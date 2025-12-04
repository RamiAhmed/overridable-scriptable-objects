# Changelog

All notable changes to this project will be documented in this file.

## [1.0.2] - 04-12-2025

- Added support for partial JSON overrides. Override files no longer need to contain all fields - only fields present in the JSON will be overridden, while other fields retain their original values from the ScriptableObject asset.

## [1.0.1] - 10-08-2025

- Added configuration asset (`OverridableScriptableObjectConfiguration`) for configuration management.
- Added hierarchical override path support, including custom paths.
- Introduced `IgnoreOverridableFieldAttribute` to optionally exclude fields from override serialization.

## [1.0.0] - 03-08-2025

- First public release.
- Core functionality for overridable ScriptableObjects.
- Editor integration for saving, loading, and deleting overrides.
