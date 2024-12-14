import subprocess
import sys
import os
import shutil

def find_git_root():
    """Find the root directory of the git repository."""
    current_dir = os.getcwd()
    while not os.path.exists(os.path.join(current_dir, '.git')):
        parent_dir = os.path.dirname(current_dir)
        if parent_dir == current_dir:
            raise Exception("Not inside a git repository")
        current_dir = parent_dir
    return current_dir

def remove_submodule_entry(submodule_path):
    """Remove the submodule entry from .gitmodules and .git/config."""
    try:
        # Remove from .gitmodules
        subprocess.run(['git', 'config', '--file', '.gitmodules', '--remove-section', f'submodule.{submodule_path}'], check=True)
        print(f"Removed submodule entry from .gitmodules: {submodule_path}")
    except subprocess.CalledProcessError:
        print(f"No entry in .gitmodules for: {submodule_path}")
    
    try:
        # Remove from .git/config
        subprocess.run(['git', 'config', '--remove-section', f'submodule.{submodule_path}'], check=True)
        print(f"Removed submodule entry from .git/config: {submodule_path}")
    except subprocess.CalledProcessError:
        print(f"No entry in .git/config for: {submodule_path}")

def remove_git_submodule(submodule_path):
    try:
        # Change directory to the git root
        git_root = find_git_root()
        os.chdir(git_root)
        
        # Check if the submodule path exists in the submodule list
        result = subprocess.run(['git', 'submodule', 'status'], capture_output=True, text=True)
        if submodule_path not in result.stdout:
            print(f"Submodule path '{submodule_path}' not found in .gitmodules or submodule list.")
            # Attempt to remove entries manually
            remove_submodule_entry(submodule_path)
        else:
            # Deinitialize the submodule
            subprocess.run(['git', 'submodule', 'deinit', '-f', submodule_path], check=True)
            
            # Remove the submodule from the git index
            subprocess.run(['git', 'rm', '--cached', submodule_path], check=True)
        
        # Remove the submodule's directory using shutil for cross-platform compatibility
        full_submodule_path = os.path.join(git_root, submodule_path)
        if os.path.isdir(full_submodule_path):
            shutil.rmtree(full_submodule_path)
            print(f"Removed submodule directory: {full_submodule_path}")
        elif os.path.isfile(full_submodule_path):
            os.remove(full_submodule_path)
            print(f"Removed submodule file: {full_submodule_path}")
        else:
            print(f"Warning: {submodule_path} does not exist as a directory or file.")
        
        # Always attempt to clean up the submodule entry from .gitmodules and .git/config
        remove_submodule_entry(submodule_path)
        
        print(f"Submodule removed successfully: {submodule_path}")
        print("Don't forget to commit and push.")
    except subprocess.CalledProcessError as e:
        print(f"Failed to remove submodule: {e}")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python remove_submodule.py <submodule_path>")
        sys.exit(1)

    submodule_path = sys.argv[1]

    remove_git_submodule(submodule_path)
