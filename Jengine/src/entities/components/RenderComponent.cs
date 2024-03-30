using System.Numerics;
using Jengine.util;
using JEngine.util;
using Raylib_cs;

namespace JEngine.entities;

public class RenderComponentData : ComponentData {
    public Graphic? Graphic;
    public bool sortZ = true;
}

public class RenderComponent : Component {
    public static Type DataType => typeof(RenderComponentData);

    // State
    public  Vector2        Offset = Vector2.Zero;
    public  Color?         OverrideColour;
    public  Graphic        BaseGraphic;
    private Graphic        bakedGraphic;
    private List<Graphic>  attachments = new();

    // Properties
    public     RenderComponentData Data     => (RenderComponentData)data;
    public ref Graphic?            Graphics => ref bakedGraphic;

    private bool ShouldRender => true;

    public RenderComponent(Entity entity, RenderComponentData? data) : base(entity, data) {
        BaseGraphic  = data.Graphic;
        bakedGraphic = BaseGraphic;
    }

    public override void Setup(bool fromSave) {
        base.Setup(fromSave);
        
        Debug.Assert(!BaseGraphic.Texture.Empty(), "Sprite missing from render component");

        BakeAttachments();
    }

    public override void Draw() {
        if (!ShouldRender)
            return;

        bakedGraphic.Draw(
            pos: Entity.Transform.GlobalPosition + Offset,
            rotation: -Entity.Transform.GlobalRotation, // TODO: Figure out why its negative
            scale: Entity.Transform.GlobalScale,
            depth: Data.sortZ ? Find.Renderer.GetDepth(Entity.Transform.GlobalPosition.Y) : (int)Depth.Below,
            overrideColour: OverrideColour,
            pickId: Entity.Selectable ? Entity.Id : null
        );

        OverrideColour = null;
    }

    public void AddAttachment(string spritePath) {
        var attachment = BaseGraphic;
        attachment.SetSprite(spritePath);
        attachments.Add(attachment);

        BakeAttachments();
    }

    private void BakeAttachments()
    {
        if (attachments.NullOrEmpty()) 
            return;

        // Bake the attachments into the texture
        // Currently assumes that the attachment will have the exact same dimensions as the main sprite
        var tex = new RenderTex(BaseGraphic.Texture.Width, BaseGraphic.Texture.Height);

        Raylib.BeginTextureMode(tex);
        Raylib.ClearBackground(Colour.Transparent);
        Raylib.DrawTexturePro(
            BaseGraphic.Texture,
            new Rectangle(0, 0, BaseGraphic.Texture.Width, -BaseGraphic.Texture.Height),
            new Rectangle(0, 0, BaseGraphic.Texture.Width, BaseGraphic.Texture.Height),
            new Vector2(0, 0),
            0,
            Color.White
        );
        foreach (var att in attachments) {
            Raylib.DrawTexturePro(
                att.Texture,
                new Rectangle(0, 0, BaseGraphic.Texture.Width, -BaseGraphic.Texture.Height),
                new Rectangle(0, 0, BaseGraphic.Texture.Width, BaseGraphic.Texture.Height),
                new Vector2(0, 0),
                0,
                Color.White
            );
        }
        Raylib.EndTextureMode();

        bakedGraphic.Texture = tex;
    }
}