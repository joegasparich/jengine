using System.Numerics;
using System.Runtime.CompilerServices;
using JEngine.util;
using Raylib_cs;

namespace JEngine; 

public static class Debug {
    public static void Log(object message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var finalSlash = Math.Max(file.LastIndexOf('\\'), file.LastIndexOf('/'));
        var fileName   = file.Substring(finalSlash + 1);
            
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("{0,-10}{1,-120}{2} {3}", "[info]", message, fileName, $"{caller}:{lineNumber}");
    }

    public static void Warn(object message, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var finalSlash = Math.Max(file.LastIndexOf('\\'), file.LastIndexOf('/'));
        var fileName   = file.Substring(finalSlash + 1);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("{0,-10}{1,-120}{2} {3}", "[warn]", message, fileName, $"{caller}:{lineNumber}");
    }
    
    public static void Error(object message, Exception? e = null, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? file = null, [CallerMemberName] string? caller = null) {
        var finalSlash = Math.Max(file.LastIndexOf('\\'), file.LastIndexOf('/'));
        var fileName   = file.Substring(finalSlash + 1);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("{0,-10}{1,-120}{2} {3}", "[error]", message, fileName, $"{caller}:{lineNumber}");
        if (e != null) 
            Console.WriteLine(e);
    }

    public static void Assert(bool condition, string message = "") {
        if (!condition)
            throw new Exception(message);
    }
}
