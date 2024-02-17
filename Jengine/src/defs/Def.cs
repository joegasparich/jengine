namespace JEngine.defs;

public class Def {
    // Config
    public string  @class;
    public bool    @abstract = false;
    public string? inherits;
    
    public string  id;
    public string? name;

    // Properties
    public Type DefType => Type.GetType("JEngine.defs." + @class);

    public DefRef GetRef() {
        return new DefRef(id);
    }
}