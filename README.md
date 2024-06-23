# Path Manager

Path Manager is a simple tool to manage the PATH environment variable. 
It provides commands to list, add, save, check duplicates, delete, restore from a JSON file, and find paths containing a certain string.

## Installation

The tool installs as a .NET Core global tool. To install it, run the following command:

```cmd
dotnet tool install --global pathmanagertool  --version 1.0.0-beta.1
```

## Usage

To run the tool, use the following command:

```cmd
pathm [command] [options]
```

The most common options are `--user` and `--system`, the target options, which specify whether the command should be applied to the user or system PATH environment variable.

## Commands

### `list`
Lists all paths in the user or system PATH environment variable.
If you specify both or none, all will be listed.

**Usage:**
```cmd
pathm list --user
pathm list --system
pathm list --user --system
pathm list --color-output --include-target
```
--color-output: Colorizes the output.
--include-target: Includes the target of the symbolic link in the output.

### `add`
Adds a new path to the user or system PATH environment variable.

Usage:

```cmd
pathm add "C:\\NewPath" --user
pathm add "C:\\NewPath" --system
pathm add "C:\\NewPath"
```

You can also use `.`, which will add the current directory to the PATH.

If not any target are specified, it will add to the user PATH.

Note that if you target the system, your cmd prompt must be running as an administrator.

If the path already exists, in either user or system, it will not be added.

### `save`
Saves the paths to a specified JSON file.


Usage:

```cmd
pathm save "paths" "C:\\Path1,C:\\Path2" --user
pathm save "paths" "C:\\Path1,C:\\Path2" --system
pathm save "paths" "C:\\Path1,C:\\Path2" --user --system
```

Write the path without any extension, it will automatically be saved as a .json file.

### `checkdup`

Checks for duplicated paths and lists them, indicating whether they are in the user or system PATH environment variable.

Usage:

```cmd
pathm checkdup --user
pathm checkdup --system
pathm checkdup --user --system
```

### `delete`

Deletes a specified path from the user or system PATH environment variable.

Usage:

```cmd
pathm delete "C:\\PathToDelete" --user
pathm delete "C:\\PathToDelete" --system
pathm delete "C:\\PathToDelete" --user --system
```


### `restore`
Restores paths from a specified JSON file to the user or system PATH environment variable.

Usage:

```cmd
pathm restore "paths.json" --user
pathm restore "paths.json" --system
pathm restore "paths.json" --user --system
```

### `find`
Finds and lists paths containing a certain string in the user or system PATH environment variable.

Usage:

```cmd
pathm find "substring" --user
pathm find "substring" --system
pathm find "substring" --user --system
```

### `gitunix`

Adds the Git Unix tools to the PATH environment variable, given that git is installed.

Usage:

```cmd
pathm gitunix 
```
