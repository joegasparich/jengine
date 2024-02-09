using System.Reflection;

namespace JEngine.util; 

public static class TypeUtility {
    public static IEnumerable<Type> GetTypesWithAttribute<Att>() where Att : Attribute {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (Type type in assembly.GetTypes()) {
                var attribs = type.GetCustomAttributes(typeof(Att), false);
                if (attribs != null && attribs.Length > 0)
                    yield return type;
            }
        }
    }
    
    /// <summary>
    /// Gets a all Type instances matching the specified class name with just non-namespace qualified class name.
    /// </summary>
    /// <param name="className">Name of the class sought.</param>
    /// <returns>Types that have the class name specified. They may not be in the same namespace.</returns>
    /// https://stackoverflow.com/questions/9273629/avoid-giving-namespace-name-in-type-gettype
    public static Type GetTypeByName(string className) {
        // First check if the full name is provided
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
            var tt = assembly.GetType(className);
            if (tt != null) {
                return tt;
            }
        }

        // Otherwise search for matching class names
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
            var assemblyTypes = a.GetTypes();
            for (var j = 0; j < assemblyTypes.Length; j++)
                if (assemblyTypes[j].Name == className)
                    return assemblyTypes[j];
        }

        return null;
    }
    
    public static IEnumerable<Type> GetSubclassesOfType<T>() where T : class
    {
        return Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
    }
}