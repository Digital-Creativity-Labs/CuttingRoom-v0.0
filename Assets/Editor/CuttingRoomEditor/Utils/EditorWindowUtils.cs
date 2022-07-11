using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorWindowUtils
{
    public static T GetWindowIfOpen<T>() where T : EditorWindow
    {
        T instance = null;

        if (EditorWindow.HasOpenInstances<T>())
        {
            instance = EditorWindow.GetWindow<T>();
        }

        return instance;
    }
}
