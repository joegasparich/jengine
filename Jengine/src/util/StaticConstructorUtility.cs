namespace JEngine.util; 

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StaticConstructorOnLaunch : Attribute {}

public static class StaticConstructorUtility
{
    public static void CallConstructors()
    {
        var types = TypeUtility.GetTypesWithAttribute<StaticConstructorOnLaunch>();

        foreach (var t in types) {
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
        }
    }
}
