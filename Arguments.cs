using System.CommandLine;
using System.Text.Json;

namespace pathtool;

public class Arguments
{
    public async Task<int> Process(string[] args)
    {
        var listCommand = new Command(
            "list",
            description: "List all paths in the user or system PATH environment variable");

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

        var checkdupCommand = new Command("checkdup", "Check for duplicated paths")
        {
            userOption,
            systemOption
        };
        checkdupCommand.SetHandler(CheckDuplicates, userOption, systemOption);

        var searchStringArgument = new Argument<string>("searchString", "The string to search for in paths");

        var findCommand = new Command("find", "Find paths containing a certain string") { searchStringArgument, userOption, systemOption };
        findCommand.SetHandler(FindPaths, searchStringArgument, userOption, systemOption);


        var rootCommand = new RootCommand
        {
            Description = "A simple tool to manage the PATH environment variable"
        };
        rootCommand.AddCommand(listCommand);
        listCommand.AddOption(userOption);
        listCommand.AddOption(systemOption);
        rootCommand.AddCommand(addCommand);
        addCommand.AddOption(userOption);
        addCommand.AddOption(systemOption);
        rootCommand.AddCommand(saveCommand);
        saveCommand.AddOption(userOption);
        saveCommand.AddOption(systemOption);
        rootCommand.AddCommand(checkdupCommand);
        rootCommand.AddCommand(findCommand);

        listCommand.SetHandler((bool user, bool system) =>
        {
            var paths = new List<string>();
            if (user)
            {
                string userPathstring = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                if (!string.IsNullOrEmpty(userPathstring))
                {
                    paths.AddRange(userPathstring.Split(';'));
                }
            }
            if (system)
            {
                string systemEnvPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
                if (!string.IsNullOrEmpty(systemEnvPath))
                {
                    paths.AddRange(systemEnvPath.Split(';'));
                }
            }
            if (paths.Count > 0)
                foreach (string path in paths.Order())
                {
                    Console.WriteLine(path);
                }

            else
            {
                Console.WriteLine("No PATHs found");
            }


        }, userOption, systemOption);


        addCommand.SetHandler((string path, bool user, bool system) =>
        {
            if (user)
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

    static void SavePaths(string pathString, string file, string suffix)
    {
        var pathsList = pathString.Split(';');
        string json = JsonSerializer.Serialize(pathsList, new JsonSerializerOptions { WriteIndented = true });
        var fname = InsertSuffixBeforeExtension(file, suffix);
        File.WriteAllText(fname, json);
        Console.WriteLine($"{suffix} Paths saved to {fname}");
    }


    static string InsertSuffixBeforeExtension(string filename, string suffix)
    {
        // Get the directory, file name without extension, and extension separately
        string directory = Path.GetDirectoryName(filename);
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
        var userDuplicates = userPaths.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key);
        var systemDuplicates = systemPaths.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key);

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
        var userPaths = new List<string>();
        var systemPaths = new List<string>();


        string userPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        userPaths = userPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.ToLower().Contains(searchString))
            .ToList();

        string systemPathString = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
        systemPaths = systemPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)
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

        if (!string.IsNullOrEmpty(envPathString))
        {
            envPathString += ";";
        }

        if (ContainsExactPath(envPathString,path))
        {
            Console.WriteLine($"Path {path} already exists in {(target == EnvironmentVariableTarget.User ? "user" : "system")} PATH");
            return;
        }

        if (ContainsExactPath(oppositeEnvPathString, path))
        {
            Console.WriteLine($"Path {path} already exists in {(target == EnvironmentVariableTarget.User ? "system" : "user")} PATH");
            return;
        }

        try
        {
            string validPath = AddValidPath(path);
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


}