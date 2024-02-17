using Raylib_cs;

namespace JEngine.ui;

public class Window {
    
    // Config
    public  string            id;
    public  bool              immediate           = false;
    public  bool              dismissOnRightClick = false;
    public  Rectangle         absRect;
    private Action<Rectangle> onUI;
    public  bool              consumesHover = true;
    
    // Properties
    public bool IsHovered => Find.UI.IsMouseOverRect(GetRect());

    public Window(Rectangle rect) {
        id = Guid.NewGuid().ToString();
        absRect = rect;
    }

    public Window(string id, Rectangle rect, Action<Rectangle> onUi) {
        this.id = id;
        absRect = rect;
        onUI = onUi;
    }

    public virtual void DoWindowContents() {
        if (onUI != null)
            onUI(GetRect());
    }

    public virtual void OnInput(InputEvent evt) {
        DoWindowContents();
    }

    public virtual void OnScreenResized(int width, int height) {}
    
    public virtual void OnClose() {}

    public virtual void Close() {
        Find.UI.CloseWindow(id);
    }
    
    public float GetWidth() {
        return Math.Min(absRect.Width, Find.Game.ScreenWidth - absRect.X);
    }
    
    public float GetHeight() {
        return Math.Min(absRect.Height, Find.Game.ScreenHeight - absRect.Y);
    }

    public Rectangle GetRect() {
        return new Rectangle(0, 0, GetWidth(), GetHeight());
    }
}