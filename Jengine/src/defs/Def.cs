using JEngine.util;

namespace JEngine.defs;

public class Def {
    // Config
    public required string  Class;
    public          bool    Abstract = false;
    public          string? Inherits;
    
    public required string  Id;
    public          string? Name;

    // Properties
    public Type DefType => TypeUtility.GetTypeByName(Class);
}