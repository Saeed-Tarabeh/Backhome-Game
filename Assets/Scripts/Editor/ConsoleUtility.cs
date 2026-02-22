#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ConsoleUtility
{
    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(null, null);
    }
}
#endif