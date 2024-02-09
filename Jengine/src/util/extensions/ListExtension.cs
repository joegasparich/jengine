namespace JEngine.util; 

public static class ListExtension {
    public static T Pop<T>(this IList<T> source) {
        if(!source.Any()) 
            throw new Exception();
        
        var element = source[^1];
        source.RemoveAt(source.Count - 1);
        return element;
    }
    public static T Dequeue<T>(this IList<T> source) {
        if(!source.Any()) 
            throw new Exception();
        
        var element = source[0];
        source.RemoveAt(0);
        return element;
    }
    public static void MoveItemAtIndexToFront<T>(this IList<T> list, int index) {
        var item = list[index];
        list.RemoveAt(index);
        list.Insert(0, item);
    }
    public static void MoveItemAtIndexToBack<T>(this IList<T> list, int index) {
        var item = list[index];
        list.RemoveAt(index);
        list.Add(item);
    }
    public static bool NullOrEmpty<T>( this List<T>? list ) {
        return list == null || list.Count == 0;
    }
    public static T? RandomElement<T>(this List<T> list) {
        if (list.NullOrEmpty()) 
            return default;
        
        return list[Rand.Int(0, list.Count)];
    }
}