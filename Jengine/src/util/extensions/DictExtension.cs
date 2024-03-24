namespace JEngine.util;

public static class DictExtension {
    public static V TryRemove<K, V>(this Dictionary<K, V> source, K key) {
        if (!source.TryGetValue(key, out var value)) 
            return default;
        
        source.Remove(key);
        return value;
    }
}