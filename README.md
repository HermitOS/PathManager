# Path Manager

Path Manager is a simple tool to manage the PATH environment variable. 
It provides commands to list, add, save, check duplicates, delete, restore from a JSON file, and find paths containing a certain string.

## Installation

The tool installs as a .NET Core global tool. To install it, run the following command:

```cm
dotnet tool install --global PathManager
```

## Usage

To run the tool, use the following command:

```cmd
dotnet pathmanager [command] [options]
```

## Commands

### `list`
Lists all paths in the user or system PATH environment variable.

**Usage:**
```cmd
dotnet pathmanager list --user
dotnet pathmanager list --system
dotnet pathmanager list --user --system
```

### `add`
Adds a new path to the user or system PATH environment variable.

Usage:

bash
Copy code
dotnet pathmanager add "C:\\NewPath" --user
dotnet pathmanager add "C:\\NewPath" --system

### `save`
Saves the paths to a specified JSON file.


Usage:

```cmd
Copy code
dotnet pathmanager save "paths" "C:\\Path1,C:\\Path2" --user
dotnet pathmanager save "paths" "C:\\Path1,C:\\Path2" --system
dotnet pathmanager save "paths" "C:\\Path1,C:\\Path2" --user --system
```

Write the path without any extension, it will autoamticall be saved as a .json file.
checkdup
Checks for duplicated paths and lists them, indicating whether they are in the user or system PATH environment variable.

Usage:

bash
Copy code
dotnet pathmanager checkdup --user
dotnet pathmanager checkdup --system
dotnet pathmanager checkdup --user --system
delete
Deletes a specified path from the user or system PATH environment variable.

Usage:

bash
Copy code
dotnet pathmanager delete "C:\\PathToDelete" --user
dotnet pathmanager delete "C:\\PathToDelete" --system
dotnet pathmanager delete "C:\\PathToDelete" --user --system
restore
Restores paths from a specified JSON file to the user or system PATH environment variable.

Usage:

bash
Copy code
dotnet pathmanager restore "paths.json" --user
dotnet pathmanager restore "paths.json" --system
dotnet pathmanager restore "paths.json" --user --system
find
Finds and lists paths containing a certain string in the user or system PATH environment variable.

Usage:

bash
Copy code
dotnet pathmanager find "substring" --user
dotnet pathmanager find "substring" --system
dotnet pathmanager find "substring" --user --system