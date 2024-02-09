namespace JEngine.util; 

public static class FileUtility {
    public static IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption) {
        if (!Directory.Exists(path))
            yield break;

        foreach (var file in Directory.EnumerateFiles(path, searchPattern, searchOption)) {
            yield return file.Replace("\\", "/");
        }
    }
}