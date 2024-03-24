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
    public           string Title;
    public           bool   ShowCloseX = false;
    public           bool   Draggable;
    public           int    HeaderHeight     = DefaultWindowHeaderHeight;
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
            AbsRect = AbsRect with { X = newPos.X, Y = newPos.Y };
        }
        
        if (doBackground)
            Gui.DrawTextureNPatch(GetRect(), Find.AssetManager.GetTexture(WindowNPatchPath), 20, backgroundColour);
        
        var headerRect = new Rectangle(0, 0, GetWidth(), HeaderHeight);
        if (!Title.NullOrEmpty()) {
            using (new TextBlock(AlignMode.MiddleCenter))
                Gui.Label(headerRect, Title);
        }
        
        headerHovered = false;
        if (Gui.HoverableArea(headerRect)) {
            headerHovered = true;
        
            if (Draggable)
                Find.Ui.SetCursor(MouseCursor.PointingHand);
        }

        if (ShowCloseX) {
            if (Gui.ButtonIcon(new Rectangle(GetWidth() - CloseIconSize - Gui.GapTiny, Gui.GapTiny, CloseIconSize, CloseIconSize), Find.AssetManager.GetTexture(CloseIcon), Color.Gray))
                Find.Ui.CloseWindow(Id);
        }
        
        base.DoWindowContents();
    }

    public override void OnInput(InputEvent evt) {
        base.OnInput(evt);

        if (evt.Consumed) 
            return;
        
        // Dragging
        if (Draggable) {
            if (headerHovered && evt.MouseDown == MouseButton.Left) {
                dragPos    = evt.MousePos - new Vector2(AbsRect.X, AbsRect.Y);
                isDragging = true;
                Find.Ui.BringWindowToFront(Id);
                evt.Consume();
            }
            if (isDragging && evt.MouseUp == MouseButton.Left) {
                isDragging = false;
                evt.Consume();
            }
        }
    }
}