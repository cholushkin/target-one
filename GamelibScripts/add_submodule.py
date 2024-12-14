import subprocess
import sys
import os

def find_git_root():
    """Find the root directory of the git repository."""
    current_dir = os.getcwd()
    while not os.path.exists(os.path.join(current_dir, '.git')):
        parent_dir = os.path.dirname(current_dir)
        if parent_dir == current_dir:
            raise Exception("Not inside a git repository")
        current_dir = parent_dir
    return current_dir

def add_git_submodule(repo_url, submodule_path):
    try:
        # Change directory to the git root
        git_root = find_git_root()
        os.chdir(git_root)
        
        # Add the submodule
        subprocess.run(['git', 'submodule', 'add', repo_url, submodule_path], check=True)
        print(f"Submodule added successfully: {submodule_path}")
        print("Don't forget to commit and push.")
        
        # Update the submodule after adding
        update_submodule(submodule_path)
    except subprocess.CalledProcessError as e:
        print(f"Failed to add submodule: {e}")
    except Exception as e:
        print(f"Error: {e}")

def update_submodule(submodule_path):
    try:
        subprocess.run(['git', 'submodule', 'update', '--init', submodule_path], check=True)
        print(f"Submodule updated successfully: {submodule_path}")
    except subprocess.CalledProcessError as e:
        print(f"Failed to update submodule: {e}")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python add_submodule.py <repository_url> <submodule_path>")
        sys.exit(1)

    repository_url = sys.argv[1]
    submodule_path = sys.argv[2]

    add_git_submodule(repository_url, submodule_path)
