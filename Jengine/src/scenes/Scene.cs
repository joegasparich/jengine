namespace JEngine.scenes;

public abstract class Scene : ISerialisable {
    // Config
    public string Name { get; }

    protected Scene(string name) {
        Name = name;
    }

    public virtual void Start()                 {}
    public virtual void PreUpdate()             {}
    public virtual void Update()                {}
    public virtual void PostUpdate()            {}
    public virtual void ConstantUpdate()              {}
    public virtual void Render()                {}
    public virtual void RenderLate()            {}
    public virtual void OnGUI()                 {}
    public virtual void OnInput(InputEvent evt) {}

    public virtual void Stop() {
        Find.Game.ClearEntities();
    }

    public virtual void Serialise() {}
    public virtual void PostLoad()  {}
}
