using System.Reflection;
using PathTool;

if (args.Length == 0)
{
    var versionString = Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;
    Console.WriteLine("Copyright 2024, Hermit AS");
    Console.WriteLine($"PathManager v{versionString}");
    Console.WriteLine("-------------");
    Console.WriteLine("Usage:");
    Console.WriteLine("  list [--user] [--system] [--include-target] [--color-output]  List all paths");
    Console.WriteLine("  add <path> [--user] [--system]                                Add a new path");
    Console.WriteLine("  delete <path> [--user] [--system]                             Delete a path");
    Console.WriteLine("  save <filename> [--user] [--system]                           Save the paths to a file");
    Console.WriteLine("  checkdup [--user] [--system]                                  Check for duplicated paths");
    Console.WriteLine("  gitunix                                                       Ensure git is in PATH and add Unix tools path if not present");

    return;
}

var arguments = new Arguments();
arguments.Process(args).Wait();
