using Raylib_cs;
using JEngine.util;

namespace JEngine.ui; 

public struct AutoLayout {
    public enum AlignMode {
        Left,
        Center,
        Right
    }

    // State
    private Rectangle rect;
    private AlignMode alignMode;
    private float     gap;
    public  float     CurY;

    // Properties
    public float     RemainingHeight => rect.Height - CurY;
    public Rectangle Rect            => rect;

    public AutoLayout(Rectangle rect, AlignMode alignMode = AlignMode.Left, float gap = GUI.GapTiny) {
        this.rect      = rect;
        CurY           = rect.Y;
        this.alignMode = alignMode;
        this.gap       = gap;
    }

    public void Header(FormattedString text) {
        using (new TextBlock(fontSize: GUI.HeaderFontSize))
            GUI.Label(GetAlignedRect(rect.Width, GUI.HeaderFontSize), text);
        CurY += GUI.HeaderFontSize + gap;
    }

    public void Label(FormattedString text, float? height = null) {
        height ??= GUI.FontSize;

        GUI.Label(GetAlignedRect(rect.Width, height.Value), text);
        CurY += height.Value + gap;
    }

    public bool ButtonText(FormattedString text, Color? col = null, bool selected = false, float? width = null, float? height = null) {
        height ??= GUI.ButtonHeight;

        var res = GUI.ButtonText(GetAlignedRect(width ?? rect.Width, height.Value), text, col, selected);
        CurY += height.Value + gap;
        
        return res;
    }

    public void TextInput(ref string text, string focusId, float? width = null, float? height = null) {
        height ??= GUI.ButtonHeight;

        GUI.TextInput(GetAlignedRect(width ?? rect.Width, height.Value), ref text, focusId);
        CurY += height.Value + gap;
    }

    public void ProgressBar(float pct, Color? colour = null, Color? backgroundColour = null, float? width = null, float? height = null) {
        height ??= 10;

        GUI.ProgressBar(GetAlignedRect(width ?? rect.Width, height.Value), pct, colour, backgroundColour);
        CurY += height.Value + gap;
    }

    public Rectangle GetRect(float height, float? width = null) {
        var r = GetAlignedRect(width ?? rect.Width, height);
        CurY += height + gap;

        return r;
    }

    private Rectangle GetAlignedRect(float width, float height) {
        switch (alignMode) {
            default:
            case AlignMode.Left:
                return new Rectangle(rect.X, CurY, width, height);
            case AlignMode.Center:
                return new Rectangle(rect.X + (rect.Width - width) / 2, CurY, width, height);
            case AlignMode.Right:
                return new Rectangle(rect.X + rect.Width - width, CurY, width, height);

        }
    }
}