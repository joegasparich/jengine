using Raylib_cs;

namespace JEngine.ui;

public class Window {
    
    // Config
    public  string            Id;
    public  bool              Immediate           = false;
    public  bool              DismissOnRightClick = false;
    public  Rectangle         AbsRect;
    public  bool              ConsumesHover = true;
    private Action<Rectangle> onUi;
    
    // Properties
    public bool IsHovered => Find.UI.IsMouseOverRect(GetRect());

    public Window(Rectangle rect) {
        Id = Guid.NewGuid().ToString();
        AbsRect = rect;
    }

    public Window(string id, Rectangle rect, Action<Rectangle> onUi) {
        this.Id   = id;
        AbsRect   = rect;
        this.onUi = onUi;
    }

    public virtual void DoWindowContents() {
        if (onUi != null)
            onUi(GetRect());
    }

    public virtual void OnInput(InputEvent evt) {
        DoWindowContents();
    }

    public virtual void OnScreenResized(int width, int height) {}
    
    public virtual void OnClose() {}

    public virtual void Close() {
        Find.UI.CloseWindow(Id);
    }
    
    public float GetWidth() {
        return Math.Min(AbsRect.Width, Find.Game.ScreenWidth - AbsRect.X);
    }
    
    public float GetHeight() {
        return Math.Min(AbsRect.Height, Find.Game.ScreenHeight - AbsRect.Y);
    }

    public Rectangle GetRect() {
        return new Rectangle(0, 0, GetWidth(), GetHeight());
    }
}