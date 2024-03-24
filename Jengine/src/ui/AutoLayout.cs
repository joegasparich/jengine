using Raylib_cs;
using JEngine.util;

namespace JEngine.ui; 

public class AutoLayout {
    public enum AlignMode {
        Left,
        Center,
        Right
    }

    // State
    private Rectangle _rect;
    private AlignMode _alignMode;
    private float     _gap;
    public  float     CurY;

    // Properties
    public float     RemainingHeight => _rect.Height - CurY;
    public Rectangle Rect            => _rect;

    public AutoLayout(Rectangle rect, AlignMode alignMode = AlignMode.Left, float gap = Gui.GapTiny) {
        _rect          = rect;
        CurY               = rect.Y;
        _alignMode     = alignMode;
        _gap           = gap;
    }

    public void Header(FormattedString text) {
        using (new TextBlock(Gui.HeaderFontSize))
            Gui.Label(GetAlignedRect(_rect.Width, Gui.HeaderFontSize), text);
        CurY += Gui.HeaderFontSize + _gap;
    }

    public void Label(FormattedString text, float? height = null) {
        height ??= Gui.FontSize;

        Gui.Label(GetAlignedRect(_rect.Width, height.Value), text);
        CurY += height.Value + _gap;
    }

    public bool ButtonText(FormattedString text, Color? col = null, bool selected = false, float? width = null, float? height = null) {
        height ??= Gui.ButtonHeight;

        var res = Gui.ButtonText(GetAlignedRect(width ?? _rect.Width, height.Value), text, col, selected);
        CurY += height.Value + _gap;
        
        return res;
    }

    public void TextInput(ref string text, string focusId, float? width = null, float? height = null) {
        height ??= Gui.ButtonHeight;

        Gui.TextInput(GetAlignedRect(width ?? _rect.Width, height.Value), ref text, focusId);
        CurY += height.Value + _gap;
    }

    public void ProgressBar(float pct, Color? colour = null, Color? backgroundColour = null, float? width = null, float? height = null) {
        height ??= 10;

        Gui.ProgressBar(GetAlignedRect(width ?? _rect.Width, height.Value), pct, colour, backgroundColour);
        CurY += height.Value + _gap;
    }

    public Rectangle GetRect(float height, float? width = null) {
        var r = GetAlignedRect(width ?? _rect.Width, height);
        CurY += height + _gap;

        return r;
    }

    private Rectangle GetAlignedRect(float width, float height) {
        switch (_alignMode) {
            default:
            case AlignMode.Left:
                return new Rectangle(_rect.X, CurY, width, height);
            case AlignMode.Center:
                return new Rectangle(_rect.X + (_rect.Width - width) / 2, CurY, width, height);
            case AlignMode.Right:
                return new Rectangle(_rect.X + _rect.Width - width, CurY, width, height);

        }
    }
}