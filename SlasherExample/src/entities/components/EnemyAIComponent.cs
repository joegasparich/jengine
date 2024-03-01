using JEngine;
using JEngine.entities;
using JEngine.util;

namespace SlasherExample.entities;

public class EnemyAIComponent : InputComponent {
    private const int TargetCheckInterval = 60;

    private Entity? target;
    
    public EnemyAIComponent(Entity entity, ComponentData? data) : base(entity, data) { }

    public override void Serialise() {
        base.Serialise();
        
        Find.SaveManager.ArchiveRef("target", ref target);
    }

    public override void Update() {
        // TODO: Hash interval
        CheckForTarget();
        
        if (target == null)
            return;

        inputVector = (target.pos - entity.pos).Normalised();
    }
    
    private void CheckForTarget() {
        if (target != null) {
            if (target.destroyed) {
                target = null;
            }
        }
        
        if (target == null) {
            target = Find.Game.GetEntitiesByTag(EntityTags.Player).FirstOrDefault();
        }
    }
}