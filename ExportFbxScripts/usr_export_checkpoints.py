import os
from run_blender import run_blender  # Import the run_blender function

# Specify the list of object names to export
objects_to_export = ["CheckpointA"]  # Modify this list as per your needs
blend_file = "../ArtSources/checkpoints.blend"  # Path to your .blend file
export_directory = "../Unity/TargetOne/Assets/Core/Fbx/Checkpoints"  # Directory to save the FBX files
blender_executable = "C:\\Program Files\\Blender Foundation\\Blender 4.2\\blender.exe"  # Update this path if necessary

# Call the run_blender function with the specified parameters
run_blender(blender_executable, blend_file, export_directory, "true", objects_to_export)
