using System.CommandLine;
using System.Text.Json;

namespace pathtool;

public class Arguments
{
    public async Task<int> Process(string[] args)
    {
        var pathArgument = new Argument<string>("path", "The path to add to be added to the PATH environment variable");

        var addCommand = new Command(
            "add",
            description: "Add a path to the user or system PATH environment variable")
        {
            pathArgument
        };

        var filenameArgument = new Argument<string>("file", "The file to save the PATH environment variable to");
        var saveCommand = new Command(
            "save",
            description: "Save the current PATH environment variable to a file, enter name of file without extension")
        {
            filenameArgument
        };

        var userOption = new Option<bool>(
            "--user",
            description: "Works on the user profile path");

        var systemOption = new Option<bool>(
            "--system",
            description: "Works on the system PATH environment variable");
        var includeTargetOption = new Option<bool>("--include-target", "Include the target (user or system) in the console output");
        var colorOutputOption = new Option<bool>("--color-output", "Color the output (green for user, blue for system)");


        var checkdupCommand = new Command("checkdup", "Check for duplicated paths")
        {
            userOption,
            systemOption
        };
        checkdupCommand.SetHandler(CheckDuplicates, userOption, systemOption);

        var searchStringArgument = new Argument<string>("searchString", "The string to search for in paths");

        var findCommand = new Command("find", "Find paths containing a certain string") { searchStringArgument, userOption, systemOption };
        findCommand.SetHandler(FindPaths, searchStringArgument, userOption, systemOption);
        var deleteCommand = new Command("delete", "Delete a path") { pathArgument, userOption, systemOption };
        var gitunixCommand = new Command("gitunix", "Ensure git is in PATH and add Unix tools path if not present");

        var listCommand = new Command(
            "list",
            description: "List all paths in the user or system PATH environment variable")
        {
            userOption,
            systemOption,
            includeTargetOption,
            colorOutputOption
        };

        var rootCommand = new RootCommand
        {
            Description = "A simple tool to manage the PATH environment variable"
        };
        rootCommand.AddCommand(listCommand);
        rootCommand.AddCommand(addCommand);
        addCommand.AddOption(userOption);
        addCommand.AddOption(systemOption);
        rootCommand.AddCommand(saveCommand);
        saveCommand.AddOption(userOption);
        saveCommand.AddOption(systemOption);
        rootCommand.AddCommand(checkdupCommand);
        rootCommand.AddCommand(findCommand);
        rootCommand.AddCommand(deleteCommand);
        rootCommand.AddCommand(gitunixCommand);

        listCommand.SetHandler(ListPaths, userOption, systemOption, includeTargetOption, colorOutputOption);

        gitunixCommand.SetHandler(EnsureGitAndUnixToolsInPath);

        deleteCommand.SetHandler(DeletePath, pathArgument, userOption, systemOption);

        addCommand.SetHandler((path, user, system) =>
        {
            if (user || (!system && !user))
            {
                AddPathToEnvironment(path, EnvironmentVariableTarget.User);
            }
            if (system)
            {
                AddPathToEnvironment(path, EnvironmentVariableTarget.Machine);
            }
        }, pathArgument, userOption, systemOption);

        saveCommand.SetHandler((file, user, system) =>
        {
            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("No file provided");
                return;
            }
            if (!user && !system)
            {
                Console.WriteLine("You must specify either --user or --system");
                return;
            }
            string userPathstring = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            string systemEnvPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            if (user)
            {
                SavePaths(userPathstring, file, "user");
            }
            if (system)
            {
                SavePaths(systemEnvPath, file, "system");
            }

        }, filenameArgument, userOption, systemOption);
        // Invoke the command line handler
        return await rootCommand.InvokeAsync(args);
    }

    private static void DeletePath(string path, bool user, bool system)
    {
        if (path == ".")
        {
            path = Directory.GetCurrentDirectory();
        }
        if (user) DeletePathFromEnvironment(path, EnvironmentVariableTarget.User);
        if (system) DeletePathFromEnvironment(path, EnvironmentVariableTarget.Machine);
    }

    private static void ListPaths(bool user, bool system, bool includeTarget, bool colorOutput)
    {
        var paths = new List<(string path, string target)>();
        if ((!user && !system) || user)
        {
            string userPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            if (!string.IsNullOrEmpty(userPathString))
            {
                paths.AddRange(userPathString.Split(';').Select(p => (p, "user")));
            }
        }
        if ((!user && !system) || system)
        {
            string systemPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            if (!string.IsNullOrEmpty(systemPathString))
            {
                paths.AddRange(systemPathString.Split(';').Select(p => (p, "system")));
            }
        }
        if (paths.Count > 0)
        {
            foreach (var (path, target) in paths.OrderBy(p => p.path))
            {
                string output = includeTarget ? $"{path} ({target})" : path;
                if (colorOutput)
                {
                    Console.ForegroundColor = target switch
                    {
                        "user" => ConsoleColor.Green,
                        "system" => ConsoleColor.Blue,
                        _ => Console.ForegroundColor
                    };
                }
                Console.WriteLine(output);
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("No PATHs found");
        }
    }

    static void SavePaths(string pathString, string file, string suffix)
    {
        var pathsList = pathString.Split(';');
        string json = JsonSerializer.Serialize(pathsList, new JsonSerializerOptions { WriteIndented = true });
        string fname = InsertSuffixBeforeExtension(file, suffix);
        File.WriteAllText(fname, json);
        Console.WriteLine($"{suffix} Paths saved to {fname}");
    }


    static string InsertSuffixBeforeExtension(string filename, string suffix)
    {
        // Get the directory, file name without extension, and extension separately
        string directory = Path.GetDirectoryName(filename) ?? "";
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        // Construct the new filename
        string newFileName = $"{fileNameWithoutExtension}.{suffix}.json";

        // Combine the directory and new filename to get the full path
        return Path.Combine(directory, newFileName);
    }

    static void CheckDuplicates(bool user, bool system)
    {
        var userPaths = new HashSet<string>();
        var systemPaths = new HashSet<string>();

        if (user)
        {
            string userPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            userPaths = [.. userPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)];
        }

        if (system)
        {
            string systemPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            systemPaths = [.. systemPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)];
        }

        // Find duplicates in user and system paths
        HashSet<string> duplicatePaths = [.. userPaths.Intersect(systemPaths)];

        // Also find duplicates within user and system paths themselves
        var userDuplicates = userPaths.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        var systemDuplicates = systemPaths.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        Console.WriteLine("Duplicate Paths:");
        foreach (var path in duplicatePaths)
        {
            Console.WriteLine($"{path} (in both user and system)");
        }

        foreach (var path in userDuplicates)
        {
            Console.WriteLine($"{path} (in user)");
        }

        foreach (var path in systemDuplicates)
        {
            Console.WriteLine($"{path} (in system)");
        }

        if (!duplicatePaths.Any() && !userDuplicates.Any() && !systemDuplicates.Any())
        {
            Console.WriteLine("No duplicates found.");
        }
    }

    static void FindPaths(string searchString, bool user, bool system)
    {
        searchString = searchString.ToLower();

        string userPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        var userPaths = userPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.ToLower().Contains(searchString))
            .ToList();

        string systemPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
        var systemPaths = systemPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.ToLower().Contains(searchString))
            .ToList();


        if (userPaths.Any())
        {
            Console.WriteLine("User Paths containing the search string:");
            foreach (var path in userPaths)
            {
                Console.WriteLine(path);
            }
        }

        if (systemPaths.Any())
        {
            Console.WriteLine("System Paths containing the search string:");
            foreach (var path in systemPaths)
            {
                Console.WriteLine(path);
            }
        }

        if (!userPaths.Any() && !systemPaths.Any())
        {
            Console.WriteLine("No paths containing the search string found.");
        }
    }

    static string AddValidPath(string path)
    {
        if (path == ".")
        {
            // Resolve the current directory
            return Directory.GetCurrentDirectory();
        }

        if (Directory.Exists(path) || File.Exists(path))
        {
            // Return the full path if it is valid
            return Path.GetFullPath(path);
        }

        throw new ArgumentException($"The path '{path}' is not valid.");
    }

    static void AddPathToEnvironment(string path, EnvironmentVariableTarget target)
    {
        string envPathString = Environment.GetEnvironmentVariable("PATH", target) ?? "";
        string oppositeEnvPathString = Environment.GetEnvironmentVariable("PATH", target == EnvironmentVariableTarget.User ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User) ?? "";
        string validPath = AddValidPath(path);
        if (!string.IsNullOrEmpty(envPathString))
        {
            envPathString += ";";
        }

        if (ContainsExactPath(envPathString, validPath))
        {
            Console.WriteLine($"Path {path} already exists in {(target == EnvironmentVariableTarget.User ? "user" : "system")} PATH");
            return;
        }

        if (ContainsExactPath(oppositeEnvPathString, validPath))
        {
            Console.WriteLine($"Path {validPath} already exists in {(target == EnvironmentVariableTarget.User ? "system" : "user")} PATH");
            return;
        }

        try
        {
            Console.WriteLine($"Adding {validPath} to PATH.");
            envPathString += validPath;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        Environment.SetEnvironmentVariable("PATH", envPathString, target);
        Console.WriteLine($"Path {path} added to {(target == EnvironmentVariableTarget.User ? "user" : "system")} PATH");
    }

    static bool ContainsExactPath(string envPathString, string pathToCheck)
    {
        var envPaths = new HashSet<string>(envPathString.Split(';', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        return envPaths.Contains(pathToCheck);
    }

    static void DeletePathFromEnvironment(string path, EnvironmentVariableTarget target)
    {
        string envPathString = Environment.GetEnvironmentVariable("PATH", target) ?? "";
        var envPaths = new HashSet<string>(envPathString.Split(';', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

        if (!envPaths.Contains(path))
        {
            Console.WriteLine($"Path {path} does not exist in {(target == EnvironmentVariableTarget.User ? "user" : "system")} PATH");
            return;
        }

        // Remove the path
        envPaths.Remove(path);

        // Rebuild the environment variable string
        envPathString = string.Join(";", envPaths);

        // Set the updated environment variable
        Environment.SetEnvironmentVariable("PATH", envPathString, target);
        Console.WriteLine($"Path {path} removed from {(target == EnvironmentVariableTarget.User ? "user" : "system")} PATH");
    }

    static void EnsureGitAndUnixToolsInPath()
    {
        string gitPath = FindExecutableInPath("git.exe");
        if (string.IsNullOrEmpty(gitPath))
        {
            Console.WriteLine("Git.exe is not found in any PATH.");
            return;
        }

        Console.WriteLine($"Git found in: {gitPath}");

        // Assuming Unix tools are in 'usr/bin' under the Git installation directory
        var gitDirectory = Directory.GetParent(gitPath)?.ToString()??"";
        string unixToolsPath = Path.Combine(Path.GetDirectoryName(gitDirectory) ?? string.Empty, "usr", "bin");
        if (!Directory.Exists(unixToolsPath))
        {
            Console.WriteLine($"Unix tools directory does not exist: {unixToolsPath}");
            return;
        }

        Console.WriteLine($"Unix tools directory found: {unixToolsPath}");

        // Check if unixToolsPath is in the user PATH
        string userPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        var userPaths = new HashSet<string>(userPathString.Split(';', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

        if (!userPaths.Contains(unixToolsPath))
        {
            userPaths.Add(unixToolsPath);
            Environment.SetEnvironmentVariable("PATH", string.Join(";", userPaths), EnvironmentVariableTarget.User);
            Console.WriteLine($"Added Unix tools path to user PATH: {unixToolsPath}");
        }
        else
        {
            Console.WriteLine("Unix tools path is already in user PATH.");
        }
    }

    static string FindExecutableInPath(string executable)
    {
        var paths = Environment.GetEnvironmentVariable("PATH")!.Split(Path.PathSeparator);
        foreach (var path in paths)
        {
            var fullPath = Path.Combine(path, executable);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }
        return string.Empty;
    }


}