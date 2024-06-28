namespace pathtool;

public class DuplicateChecker
{
    public IEnumerable<(string Key, int Count)> DuplicatePaths { get; private set; } = [];
    public IEnumerable<(string Key, int Count)> UserDuplicates { get; private set; } = [];
    public IEnumerable<(string Key, int Count)> SystemDuplicates { get; private set; } = [];

    public bool HasDuplicates => DuplicatePaths.Any();

    public void CheckForDuplicates(IReadOnlyCollection<string> userPaths, IReadOnlyCollection<string> systemPaths)
    {
        // Find duplicates in user and system paths
        List<string> allPaths = [];
        allPaths.AddRange(userPaths);
        allPaths.AddRange(systemPaths);
        DuplicatePaths = allPaths.GroupBy(p => p.ToLower()).Where(g => g.Count() > 1).Select(g => (g.Key, g.Count())).ToList();

        // Also find duplicates within user and system paths themselves
        UserDuplicates = userPaths.GroupBy(p => p.ToLower()).Where(g => g.Count() > 1).Select(g => (g.Key, g.Count())).ToList();
        SystemDuplicates = systemPaths.GroupBy(p => p.ToLower()).Where(g => g.Count() > 1).Select(g => (g.Key, g.Count())).ToList();

    }
}