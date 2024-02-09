namespace JEngine.util; 

public static class StringExtension {
    public static bool NullOrEmpty(this string str) {
        return string.IsNullOrEmpty(str);
    }
    public static string Capitalise(this string str) {
        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);
        
        return str.ToUpper();
    }
    public static string ToSnakeCase(this string str) {
        return str.ToLower().Replace(" ", "_");
    }
}