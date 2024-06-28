namespace PathTool
{
    public interface IPathEnvironment
    {
        string GetPath(EnvironmentVariableTarget target);
        void SetPath(EnvironmentVariableTarget target, string path);
        string GetUserPath();
        string GetSystemPath();
        List<string> GetAllUserPaths();
        List<string> GetAllSystemPaths();
    }

    public class PathEnvironment : IPathEnvironment
    {
        const string EnvPath = "PATH";
        public string GetPath(EnvironmentVariableTarget target)
        {
            return Environment.GetEnvironmentVariable(EnvPath, target) ?? "";
        }

        public void SetPath(EnvironmentVariableTarget target, string path)
        {
            Environment.SetEnvironmentVariable(EnvPath, path, target);
        }

        public string GetUserPath() => GetPath(EnvironmentVariableTarget.User);
        public string GetSystemPath() => GetPath(EnvironmentVariableTarget.Machine);

        public List<string> GetAllUserPaths()
        {
            string userPathString = GetUserPath();
            return [.. userPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)];
        }

        public List<string> GetAllSystemPaths()
        {
            string systemPathString = GetSystemPath();
            return [.. systemPathString.Split(';', StringSplitOptions.RemoveEmptyEntries)];
        }


    }
}
