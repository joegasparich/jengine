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
    private readonly bool   _doBackground     = true;
    protected        Color  _backgroundColour = Color.White;
    
    // State
    protected bool    _headerHovered;
    protected bool    _isDragging;
    private   Vector2 _dragPos = Vector2.Zero;

    public Dialog(Rectangle rect) : base(rect) {}
    public Dialog(string id, Rectangle rect, Action<Rectangle> onUi, bool doBackground = true) : base(id, rect, onUi) {
        _doBackground = doBackground;
    }

    public override void DoWindowContents() {
        if (_isDragging) {
            var newPos = Find.Input.GetMousePos() - _dragPos;
            AbsRect = AbsRect with { X = newPos.X, Y = newPos.Y };
        }
        
        if (_doBackground)
            Gui.DrawTextureNPatch(GetRect(), Find.AssetManager.GetTexture(WindowNPatchPath), 20, _backgroundColour);
        
        var headerRect = new Rectangle(0, 0, GetWidth(), HeaderHeight);
        if (!Title.NullOrEmpty()) {
            using (new TextBlock(AlignMode.MiddleCenter))
                Gui.Label(headerRect, Title);
        }
        
        _headerHovered = false;
        if (Gui.HoverableArea(headerRect)) {
            _headerHovered = true;
        
            if (Draggable)
                Find.UI.SetCursor(MouseCursor.PointingHand);
        }

        if (ShowCloseX) {
            if (Gui.ButtonIcon(new Rectangle(GetWidth() - CloseIconSize - Gui.GapTiny, Gui.GapTiny, CloseIconSize, CloseIconSize), Find.AssetManager.GetTexture(CloseIcon), Color.Gray))
                Find.UI.CloseWindow(Id);
        }
        
        base.DoWindowContents();
    }

    public override void OnInput(InputEvent evt) {
        base.OnInput(evt);

        if (evt.Consumed) 
            return;
        
        // Dragging
        if (Draggable) {
            if (_headerHovered && evt.MouseDown == MouseButton.Left) {
                _dragPos    = evt.MousePos - new Vector2(AbsRect.X, AbsRect.Y);
                _isDragging = true;
                Find.UI.BringWindowToFront(Id);
                evt.Consume();
            }
            if (_isDragging && evt.MouseUp == MouseButton.Left) {
                _isDragging = false;
                evt.Consume();
            }
        }
    }
}