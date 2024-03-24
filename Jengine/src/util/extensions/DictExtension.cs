namespace JEngine.util;

public static class DictExtension {
    public static TV TryRemove<TK, TV>(this Dictionary<TK, TV> source, TK key) {
        if (!source.TryGetValue(key, out var value)) 
            return default;
        
        source.Remove(key);
        return value;
    }
}