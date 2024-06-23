# Path Manager

Path Manager is a simple tool to manage the PATH environment variable. 
It provides commands to list, add, save, check duplicates, delete, restore from a JSON file, and find paths containing a certain string.

## Installation

The tool installs as a .NET Core global tool. To install it, run the following command:

```cm
dotnet tool install --global pathm
```

## Usage

To run the tool, use the following command:

```cmd
dotnet pathm [command] [options]
```

The most common options are `--user` and `--system`, the target options, which specify whether the command should be applied to the user or system PATH environment variable.

## Commands

### `list`
Lists all paths in the user or system PATH environment variable.
If you specify both or none, all will be listed.

**Usage:**
```cmd
dotnet pathm list --user
dotnet pathm list --system
dotnet pathm list --user --system
dotnet pathm list --color-output --include-target
```
--color-output: Colorizes the output.
--include-target: Includes the target of the symbolic link in the output.

### `add`
Adds a new path to the user or system PATH environment variable.

Usage:

```cmd
dotnet pathm add "C:\\NewPath" --user
dotnet pathm add "C:\\NewPath" --system
dotnet pathm add "C:\\NewPath"
```

You can also use `.`, which will add the current directory to the PATH.

If not any target are specified, it will add to the user PATH.

Note that if you target the system, your cmd prompt must be running as an administrator.

If the path already exists, in either user or system, it will not be added.

### `save`
Saves the paths to a specified JSON file.


Usage:

```cmd
dotnet pathm save "paths" "C:\\Path1,C:\\Path2" --user
dotnet pathm save "paths" "C:\\Path1,C:\\Path2" --system
dotnet pathm save "paths" "C:\\Path1,C:\\Path2" --user --system
```

Write the path without any extension, it will automatically be saved as a .json file.

### `checkdup`

Checks for duplicated paths and lists them, indicating whether they are in the user or system PATH environment variable.

Usage:

```cmd
dotnet pathm checkdup --user
dotnet pathm checkdup --system
dotnet pathm checkdup --user --system
```

### `delete`

Deletes a specified path from the user or system PATH environment variable.

Usage:

```cmd
dotnet pathm delete "C:\\PathToDelete" --user
dotnet pathm delete "C:\\PathToDelete" --system
dotnet pathm delete "C:\\PathToDelete" --user --system
```


### `restore`
Restores paths from a specified JSON file to the user or system PATH environment variable.

Usage:

```cmd
dotnet pathm restore "paths.json" --user
dotnet pathm restore "paths.json" --system
dotnet pathm restore "paths.json" --user --system
```

### `find`
Finds and lists paths containing a certain string in the user or system PATH environment variable.

Usage:

```cmd
dotnet pathm find "substring" --user
dotnet pathm find "substring" --system
dotnet pathm find "substring" --user --system
```

### `gitunix`

Adds the Git Unix tools to the PATH environment variable, given that git is installed.

Usage:

```cmd
dotnet pathm gitunix 
```
