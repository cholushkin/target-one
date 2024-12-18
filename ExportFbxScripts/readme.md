# Blender to Unity FBX Export Scripts

This collection of Python scripts is designed to automate the process of exporting specific objects from a Blender .blend file to Unity as .fbx files. The scripts handle proper axis conversion for compatibility between Blender and Unity.

____________
## ```export_fbx_object.py```

```export_fbx_object.py``` script is executed within Blender's Python environment and handles the main logic for exporting Blender objects to .fbx. Usually you never need to edit this file. It performs the following tasks:

- Opens a specified Blender file.
- Finds a specific object by name and its children.
- Unhides and selects the target objects recursively.
- Exports the selected objects to the specified directory in .fbx format.
- Optionally applies Unity axis conversion during the export process.

**Usage within Blender:**

```
blender --background --python export_fbx_object.py -- <blend_file> <export_directory> <unity_axis_conversion> <objects_to_export>
```

- `<blend_file>`: Path to the .blend file.
- `<export_directory>`: Directory where the .fbx files will be saved.
- `<unity_axis_conversion>`: Boolean (true/false) to apply Unity axis conversion.
- `<objects_to_export>`: List of object names to be exported.


____________

## ```run_blender.py```

This script is executed in your system's Python environment and acts as a tool to invoke Blender and pass the necessary parameters (the Python script and its arguments) to Blender. It constructs the command line call to Blender, including the .blend file, export directory, Unity axis conversion flag, and objects to export. Usually you never need to edit this file.

Usage Example:

```
import os
from run_blender import run_blender

run_blender(blender_executable, blend_file, export_directory, "true", objects_to_export)
```


____________

## ```usr_ Prefixed Scripts```

The usr_ prefix is used for user-specific scripts that customize the list of objects you want to export. These scripts define the objects, blend files, and other parameters needed for export and pass them to the run_blender.py script. You can create different usr_ prefixed scripts for various export needs. 

Example of `usr_export_surfaces.py`: 

```
from run_blender import run_blender

# Specify objects to export
objects_to_export = ["Cube", "Plane16x16"]
blend_file = "../ArtSource/Surfaces.blend"
export_directory = "../Unity/Quantra/Assets/Core/Fbx/CompanyLogo"
blender_executable = "C:/Program Files/Blender Foundation/Blender 4.2/blender.exe"

# Call the run_blender function
run_blender(blender_executable, blend_file, export_directory, "true", objects_to_export)
You can create multiple usr_*.py scripts for different .blend files or object sets to streamline your export workflow.
```

____________

## Unity Axis Conversion
When exporting objects from Blender to Unity, proper axis alignment is crucial due to the different coordinate systems used by the two software:

Blender: Forward is along the Y-axis and up is along the Z-axis.
Unity: Forward is along the Z-axis and up is along the Y-axis.
To resolve this, the scripts provide an option to apply Unity axis conversion during export. When the unity_axis_conversion flag is set to true, the following transformations are applied:

Forward Axis: Blender's Y is mapped to Unity's Z, which is achieved by setting axis_forward='-Z'.
Up Axis: Both Blender and Unity use the same up direction (Y), so no transformation is required for the up axis.
By enabling axis conversion, your objects will be correctly oriented in Unity after export.

Example of enabling axis conversion:

```
run_blender(blender_executable, blend_file, export_directory, "true", objects_to_export) # "true" - enables axis conversion
```
