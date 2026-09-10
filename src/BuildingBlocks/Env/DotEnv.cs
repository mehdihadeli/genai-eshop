namespace BuildingBlocks.Env;

public static class DotEnv
{
    public static void Load(string? filePath = null)
    {
        if (filePath is null)
        {
            foreach (var startDirectory in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                var directory = new DirectoryInfo(startDirectory);
                while (directory is not null)
                {
                    var candidate = Path.Combine(directory.FullName, ".env");
                    if (File.Exists(candidate))
                    {
                        filePath = candidate;
                        break;
                    }

                    directory = directory.Parent;
                }

                if (filePath is not null)
                    break;
            }
        }

        if (filePath is null || !File.Exists(filePath))
            return;

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (line.StartsWith("export ", StringComparison.Ordinal))
                line = line[7..].TrimStart();

            var separatorIndex = line.IndexOf('=', StringComparison.Ordinal);
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 && value[0] == value[^1] && (value[0] == '\'' || value[0] == '"'))
                value = value[1..^1];

            // Keep process/container-provided secrets ahead of local .env values.
            Environment.SetEnvironmentVariable(key, Environment.GetEnvironmentVariable(key) ?? value);
        }
    }
}
