namespace JEngine.util; 

public static class TickUtility {
    public const int TicksPerSecond = 60;
    public const int TickRareInterval = 300; // 5 seconds
    
    public static bool IsHashTick(int id, int interval) {
        return (Find.Game.Ticks + id) % interval == 0;
    }
    public static bool IsHashTick(string id, int interval) {
        return IsHashTick(id.GetHashCode(), interval);
    }
}