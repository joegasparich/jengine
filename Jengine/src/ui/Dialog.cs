using System.Numerics;
using Raylib_cs;
using JEngine.util;

namespace JEngine.ui; 

public class Dialog : Window {
    // Constants
    public const  int    DefaultWindowHeaderHeight = 18;
    private const string WindowNPatchPath          = "assets/textures/ui/window.png";
    private const string CloseIcon                 = "assets/textures/ui/icons/close.png";
    private const int    CloseIconSize             = 16;
    
    // Config
    public           string title;
    public           bool   showCloseX = false;
    public           bool   draggable;
    public           int    headerHeight     = DefaultWindowHeaderHeight;
    private readonly bool   doBackground     = true;
    protected        Color  backgroundColour = Color.White;
    
    // State
    protected bool    headerHovered;
    protected bool    isDragging;
    private   Vector2 dragPos = Vector2.Zero;

    public Dialog(Rectangle rect) : base(rect) {}
    public Dialog(string id, Rectangle rect, Action<Rectangle> onUi, bool doBackground = true) : base(id, rect, onUi) {
        this.doBackground = doBackground;
    }

    public override void DoWindowContents() {
        if (isDragging) {
            var newPos = Find.Input.GetMousePos() - dragPos;
            absRect = absRect with { X = newPos.X, Y = newPos.Y };
        }
        
        if (doBackground)
            GUI.DrawTextureNPatch(GetRect(), Find.AssetManager.GetTexture(WindowNPatchPath), 20, backgroundColour);
        
        var headerRect = new Rectangle(0, 0, GetWidth(), headerHeight);
        if (!title.NullOrEmpty()) {
            using (new TextBlock(AlignMode.MiddleCenter))
                GUI.Label(headerRect, title);
        }
        
        headerHovered = false;
        if (GUI.HoverableArea(headerRect)) {
            headerHovered = true;
        
            if (draggable)
                Find.UI.SetCursor(MouseCursor.PointingHand);
        }

        if (showCloseX) {
            if (GUI.ButtonIcon(new Rectangle(GetWidth() - CloseIconSize - GUI.GapTiny, GUI.GapTiny, CloseIconSize, CloseIconSize), Find.AssetManager.GetTexture(CloseIcon), Color.Gray))
                Find.UI.CloseWindow(id);
        }
        
        base.DoWindowContents();
    }

    public override void OnInput(InputEvent evt) {
        base.OnInput(evt);

        if (evt.consumed) 
            return;
        
        // Dragging
        if (draggable) {
            if (headerHovered && evt.mouseDown == MouseButton.Left) {
                dragPos    = evt.mousePos - new Vector2(absRect.X, absRect.Y);
                isDragging = true;
                Find.UI.BringWindowToFront(id);
                evt.Consume();
            }
            if (isDragging && evt.mouseUp == MouseButton.Left) {
                isDragging = false;
                evt.Consume();
            }
        }
    }
}