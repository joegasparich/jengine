namespace JEngine.util; 

public static class TextUtility {
    public static string FormatAsCash(float val) {
        return $"${val:F2}";
    }
}