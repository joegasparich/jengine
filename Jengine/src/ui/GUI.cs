using System.Numerics;
using System.Text;
using Raylib_cs;
using JEngine.util;

namespace JEngine.ui;

public enum AlignMode {
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

internal class ClipPlane {
    public RenderTexture2D RenderTexture = Raylib.LoadRenderTexture(Find.Game.ScreenWidth, Find.Game.ScreenHeight);
    public Rectangle       Rect;
    public Rectangle       Source;

    public void SetRect(Rectangle rect) {
        Rect = rect;
        Source    = new Rectangle(rect.X, -(rect.Height + rect.Y), rect.Width, -rect.Height);
    }

    public void CleanUp() {
        Raylib.UnloadRenderTexture(RenderTexture);
    }
}

[StaticConstructorOnLaunch]
public static class GUI {
    // Constants
    public const            int   GapTiny        = 6;
    public const            int   GapSmall       = 10;
    public const            int   Margin         = 10;
    public const            int   ButtonHeight   = 30;
    public const            int   HeaderFontSize = 16;
    private static readonly Color highlightColor = new(255, 255, 255, 150);
    public static readonly  Color UiButtonColour = new(230, 230, 230, 255);

    // Config
    public static Color     TextColour = Color.Black;
    public static AlignMode TextAlign  = AlignMode.TopLeft;
    public static int       FontSize   = 8;
    public static Font      Font;
    
    // State
    private static Stack<ClipPlane> clipPlanePool    = new();
    private static Stack<ClipPlane> activeClipPlanes = new();
    
    // Properties
    public static float UiScale => Find.UI.UiScale;

    static GUI() {
        for (var i = 0; i < 10; i++) {
            clipPlanePool.Push(new ClipPlane());
        }
    }

    private static Vector2 GetTextAlignPos(Rectangle rect, float textWidth) {
        return TextAlign switch {
            AlignMode.TopLeft => new Vector2(rect.X, rect.Y),
            AlignMode.TopCenter => new Vector2(rect.X + (rect.Width - textWidth) / 2, rect.Y),
            AlignMode.TopRight => new Vector2(rect.X + (rect.Width - textWidth), rect.Y),
            AlignMode.MiddleLeft => new Vector2(rect.X, rect.Y + (rect.Height - FontSize) / 2),
            AlignMode.MiddleCenter => new Vector2(rect.X + (rect.Width - textWidth) / 2, rect.Y + (rect.Height - FontSize) / 2),
            AlignMode.MiddleRight => new Vector2(rect.X + (rect.Width - textWidth), rect.Y + (rect.Height - FontSize) / 2),
            AlignMode.BottomLeft => new Vector2(rect.X, rect.Y + (rect.Height - FontSize)),
            AlignMode.BottomCenter => new Vector2(rect.X + (rect.Width - textWidth) / 2, rect.Y + (rect.Height - FontSize)),
            AlignMode.BottomRight => new Vector2(rect.X + (rect.Width - textWidth), rect.Y + (rect.Height - FontSize))
        };
    }

    private static float GetSpacing(Font font, float fontSize) {
        if (font.Texture.Id == Find.UI.VisibilityFont.Texture.Id)
            return -1f;
        
        return fontSize / Font.BaseSize;
    }

    public static Vector2 MeasureText(string text) {
        var scaledFontSize = FontSize * UiScale;
        return Raylib.MeasureTextEx(Font, text, scaledFontSize, GetSpacing(Font, scaledFontSize));
    }
    public static Vector2 MeasureText(FormattedString text) {
        var scaledFontSize = FontSize * UiScale;
        return Raylib.MeasureTextEx(Font, text.StripTags, scaledFontSize, GetSpacing(Font, scaledFontSize));
    }
    
    // Clipping
    public static void StartClip(Rectangle rect) {
        if (clipPlanePool.NullOrEmpty()) {
            Debug.Error("Ran out of clip planes!");
            return;
        }
        
        var plane = clipPlanePool.Pop();
        plane.SetRect(rect);
        Raylib.BeginTextureMode(plane.RenderTexture);
        activeClipPlanes.Push(plane);
    }

    public static void EndClip() {
        Raylib.EndTextureMode();
        var plane = activeClipPlanes.Pop();
        
        Raylib.DrawTexturePro(
            plane.RenderTexture.Texture,
            plane.Source,
            plane.Rect,
            new Vector2(0, 0),
            0,
            Color.White
        );
        
        clipPlanePool.Push(plane);
    }
    
    // Draw functions
    public static void DrawRect(Rectangle rect, Color col) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawRectangle(absRect.X.RoundToInt(), absRect.Y.RoundToInt(), absRect.Width.RoundToInt(), absRect.Height.RoundToInt(), col);
    }
    
    public static void DrawBorder(Rectangle rect, int thickness, Color col) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawRectangleLinesEx(absRect, thickness, col);
    }

    public static void DrawTexture(Rectangle rect, Texture2D texture, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawTexturePro(
            texture,
            new Rectangle(0, 0, texture.Width, texture.Height),
            TextureUtility.MaintainAspectRatio(absRect, texture),
            new Vector2(0, 0),
            0,
            col ?? Color.White
        );
    }
    
    public static void DrawSubTexture(Rectangle rect, Texture2D texture, Rectangle source, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var absRect = Find.UI.GetAbsRect(rect);
        Raylib.DrawTexturePro(
            texture,
            new Rectangle(
                texture.Width * source.X,
                texture.Height * source.Y,
                texture.Width * source.Width,
                texture.Height * source.Height
            ),
            TextureUtility.MaintainAspectRatio(absRect, texture, source),
            new Vector2(0, 0),
            0,
            col ?? Color.White
        );
    }

    public static void DrawTextureNPatch(Rectangle rect, Texture2D texture, int cornerSize, Color? col = null) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var nPatchInfo = new NPatchInfo {
            Source = new Rectangle(0, 0, texture.Width, texture.Height),
            Left = cornerSize,
            Top = cornerSize,
            Right = cornerSize,
            Bottom = cornerSize,
            Layout = NPatchLayout.NinePatch
        };

        Raylib.DrawTextureNPatch(
            texture,
            nPatchInfo,
            Find.UI.GetAbsRect(rect),
            new Vector2(0, 0),
            0,
            col ?? Color.White
        );
    }
    
    public static void Label(Rectangle rect, FormattedString text) {
        if (Find.UI.CurrentEvent != UIEvent.Draw) 
            return;

        var scaledFontSize = FontSize * UiScale;
        var absRect        = Find.UI.GetAbsRect(rect);

        if (MeasureText(text).X > absRect.Width)
            text = WrapText(text, absRect.Width);

        var textWidth = MeasureText(text).X;
        var drawPos   = GetTextAlignPos(absRect, textWidth.FloorToInt());
        
        // TODO: Measure performance and maybe just use DrawTextEx as an overload if this is too slow
        DrawFormattedText(Font, text, drawPos.Floor(), scaledFontSize, GetSpacing(Font, scaledFontSize), TextColour);
    }

    public static FormattedString WrapText(FormattedString input, float maxWidth) {
        if (input.Length == 0 || maxWidth <= 0)
            return string.Empty;

        var words       = input.Split(' '); // Split the input string into words
        var wrappedText = new FormattedString();
        var currentLine = new FormattedString();

        foreach (var word in words) {
            if (MeasureText(currentLine + word).X <= maxWidth) {
                // Add the word to the current line if adding it doesn't exceed the maximum width
                currentLine.taggedString += word.taggedString + " ";
            } else {
                wrappedText.taggedString += currentLine.taggedString.Trim() + "\n"; // Insert newline
                currentLine =  word + " ";
            }
        }

        wrappedText += currentLine.Trim(); // Add the last line
        return wrappedText;
    }

    private static void DrawCaret(Rectangle rect, float textWidth) {
        var pos = GetTextAlignPos(rect, textWidth);
        DrawRect(new Rectangle(pos.X + textWidth + 2, pos.Y, 1, FontSize), TextColour);
    }
    
    // Input functions
    public static bool ClickableArea(Rectangle rect) {
        if (Find.UI.CurrentEvent != UIEvent.Input) 
            return false;

        return Find.Input.GetCurrentEvent().MouseDown == MouseButton.Left && Find.UI.IsMouseOverRect(rect);
    }
    
    public static bool HoverableArea(Rectangle rect) {
        return Find.UI.IsMouseOverRect(rect);
    }

    public static void DrawHighlight(Rectangle rect) {
        DrawRect(rect, highlightColor);
    }
    
    // Widgets
    public static void Header(Rectangle rect, FormattedString text) {
        using (new TextBlock(alignMode: AlignMode.TopCenter, fontSize: HeaderFontSize))
            Label(rect, text);
    }

    public static bool ButtonEmpty(Rectangle rect, Color? col = null, bool selected = false) {
        DrawRect(rect, col ?? UiButtonColour);
        HighlightMouseover(rect);
        
        if (selected)
            DrawBorder(rect, 2, Color.Black);
        
        if (HoverableArea(rect))
            Find.UI.SetCursor(MouseCursor.PointingHand);

        return ClickableArea(rect);
    }

    public static bool ButtonText(Rectangle rect, FormattedString text, Color? col = null, bool selected = false) {
        DrawRect(rect, col ?? UiButtonColour);
        HighlightMouseover(rect);
        
        if (selected) 
            DrawBorder(rect, 2, Color.Black);
        
        Label(rect, text);

        if (HoverableArea(rect))
            Find.UI.SetCursor(MouseCursor.PointingHand);

        return ClickableArea(rect);
    }

    public static bool ButtonIcon(Rectangle rect, Texture2D icon, Color? colour = null) {
        DrawTexture(rect, icon, colour);

        if (HoverableArea(rect))
            Find.UI.SetCursor(MouseCursor.PointingHand);

        return ClickableArea(rect);
    }
    
    public static void TextInput(Rectangle rect, ref string text, string focusId) {
        DrawRect(rect, Color.White);
        DrawBorder(rect, 1, Color.Black);
        Label(rect.ContractedBy(GapTiny), text);

        if (HoverableArea(rect))
            Find.UI.SetCursor(MouseCursor.IBeam);
        if (ClickableArea(rect))
            Find.UI.SetFocus(focusId);

        if (Find.UI.IsFocused(focusId)) {
            if (Find.Game.Ticks % 120 < 60)
                DrawCaret(rect.ContractedBy(GapTiny), MeasureText(text).X);

            if (Find.UI.CurrentEvent == UIEvent.Input && Find.Input.GetCurrentEvent().Type == InputEventType.Key) {
                var evt = Find.Input.GetCurrentEvent();
                if (evt.Consumed) 
                    return;
                
                evt.Consume();

                if (evt.KeyDown.HasValue && evt.KeyDown.Value.IsAlphanumeric()) {
                    var character = ((char)evt.KeyDown.Value).ToString().ToLower();
                    text += character;
                }
                if (Find.Input.IsKeyHeld(KeyboardKey.Backspace) && text.Length > 0 && Find.Game.Frames % 5 == 0)
                    text = text.Substring(0, text.Length - 1);
            }
        }
    }

    public static void DropDown(Rectangle rect, FormattedString text, ref bool open, List<(FormattedString, Action)> opts) {
        if (ButtonText(rect, text))
            open = !open;
        if (!open) 
            return;

        var i = 0;
        foreach (var opt in opts) {
            if (ButtonText(rect.OffsetBy(0, (i + 1) * rect.Height), opt.Item1)) {
                opt.Item2();
                open = false;
            }
            i++;
        }
    }

    public static void ProgressBar(Rectangle rect, float pct, Color? colour = null, Color? backgroundColour = null) {
        DrawRect(rect, backgroundColour ?? Colour.Grey);
        DrawRect(new Rectangle(rect.X, rect.Y, rect.Width * pct, rect.Height), colour ?? Colour.Primary);
    }

    public static void HighlightMouseover(Rectangle rect) {
        if (HoverableArea(rect))
            DrawHighlight(rect);
    }

    public static void ToolTip(Rectangle rect, string text, string id) {
        if (HoverableArea(rect)) {
            var textMeasurements = MeasureText(text);
            var dimensions       = new Vector2(textMeasurements.X + GapSmall * 2, textMeasurements.Y + GapSmall * 2);
            
            Find.UI.DoImmediateWindow(
                $"{id}-tooltip",
                new Rectangle(
                    Find.Input.GetMousePos().X,
                    Find.Input.GetMousePos().Y - dimensions.Y,
                    dimensions.X,
                    dimensions.Y),
                inRect => {
                    DrawRect(inRect, Color.DarkGray);
                    DrawBorder(inRect, 2, Color.White);
                    using (new TextBlock(alignMode: AlignMode.MiddleCenter, colour: Color.White))
                        Label(new Rectangle(0, 0, inRect.Width, inRect.Height), text);
                }, consumesHover: false);
        }
    }
    
    private const int TextLineSpacing = 15; // Match Raylib's default line spacing 
    // Based off Raylib's DrawTextEx function
    private static void DrawFormattedText(Font font, FormattedString text, Vector2 position, float fontSize, float spacing, Color tint) {
        var textOffsetY = 0;    // Offset between lines (on linebreak '\n')
        var textOffsetX = 0.0f; // Offset X to next character to draw

        if (font.Texture.Id == 0)
            font = Raylib.GetFontDefault(); // Security check in case of not valid font
        
        foreach (var (tagName, attribute, contents) in text.Resolve()) {
            var col = tagName == "c" ? Colour.IntToColour(Int32.Parse(attribute)) : tint;

            unsafe {
                var bytes = Encoding.UTF8.GetBytes(contents);
                fixed (byte* buffer = bytes) {
                    var str = (sbyte*)buffer;
                    
                    var size = Raylib.TextLength(str); 
            
                    var scaleFactor = fontSize / font.BaseSize; // Character quad scaling factor
            
                    for (var i = 0; i < size;) {
                        // Get next codepoint from byte string and glyph index in font
                        var codepointByteCount = 0;
                        var codepoint          = Raylib.GetCodepoint(&str[i], &codepointByteCount);
                        var index              = Raylib.GetGlyphIndex(font, codepoint);
                        
                        if (codepoint == '\n') {
                            // NOTE: Line spacing is a global variable, use SetTextLineSpacing() to setup
                            textOffsetY += TextLineSpacing;
                            textOffsetX =  0.0f;
                        } else {
                            if (codepoint != ' ' && codepoint != '\t')
                                Raylib.DrawTextCodepoint(font, codepoint, position + new Vector2(textOffsetX, textOffsetY), fontSize, col);
            
                            if (font.Glyphs[index].AdvanceX == 0)
                                textOffsetX += font.Recs[index].Width * scaleFactor + spacing;
                            else
                                textOffsetX += font.Glyphs[index].AdvanceX * scaleFactor + spacing;
                        }
            
                        i += codepointByteCount; // Move text bytes counter to next codepoint
                    }
                }
            }
        }
        
    }
}

public struct TextBlock : IDisposable
{
    private Font      oldFont;
    private int       oldFontSize;
    private Color     oldColor;
    private AlignMode oldAlignMode;

    public TextBlock(Font? font = null, int? fontSize = null, Color? colour = null, AlignMode? alignMode = null) {
        oldFont      = GUI.Font;
        oldFontSize  = GUI.FontSize;
        oldColor     = GUI.TextColour;
        oldAlignMode = GUI.TextAlign;
        
        if (font != null)
            GUI.Font = font.Value;
        else
            GUI.Font = Find.UI.DefaultFont;

        if (fontSize != null)
            GUI.FontSize = fontSize.Value;
        else
            GUI.FontSize = GUI.Font.BaseSize;

        if (colour != null)
            GUI.TextColour = colour.Value;
        else
            GUI.TextColour = Color.Black;

        if (alignMode != null)
            GUI.TextAlign = alignMode.Value;
        else
            GUI.TextAlign = AlignMode.TopLeft;
    }

    public void Dispose()
    {
        GUI.TextAlign  = oldAlignMode;
        GUI.TextColour = oldColor;
        GUI.FontSize   = oldFontSize;
        GUI.Font       = oldFont;
    }
}