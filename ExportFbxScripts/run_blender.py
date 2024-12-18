import subprocess
import os

def run_blender(blender_executable, blend_file, export_directory, unity_axis_conversion, objects_to_export):
    # Construct the command line arguments for Blender
    command = [
        blender_executable,
        "--background",  # Run Blender in background mode
        "--python", 
        os.path.abspath("export_fbx_object.py"),  # Point to the script that handles exporting
        "--",  # Separator for Blender arguments
        os.path.abspath(blend_file),  # Absolute path of the blend file
        os.path.abspath(export_directory),  # Absolute path of the export directory
        unity_axis_conversion # str, not bool
    ] + objects_to_export  # Append object names

    # Print the command for debugging
    print(f"Running command: {' '.join(command)}")

    # Run the Blender command
    subprocess.run(command)
