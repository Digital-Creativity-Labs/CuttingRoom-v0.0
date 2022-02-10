using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public static class PropertyDrawerUtilities
	{
		/*public static void MethodPicker(Rect position, SerializedProperty property, GUIContent label, Type methodType, string serializedPropertyName)
		{
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Get the methods which are defined in the static OutputSelectionFunction class.
			MethodInfo[] methodInfos = methodType.GetMethods(BindingFlags.Public | BindingFlags.Static);

			// List of names to be displayed on inspector.
			List<string> methodNames = new List<string>();

			// Iterate the reflected methods and add their names to the possibilities for display.
			for (int count = 0; count < methodInfos.Length; count++)
			{
				methodNames.Add(methodInfos[count].Name);
			}

			int selectedIndex = -1;

			SerializedProperty methodNameSerializedProperty = property.FindPropertyRelative(serializedPropertyName);

			string selectedMethodName = methodNameSerializedProperty.stringValue;

			// If the current method which has been selected is in the list of possibilities, start with that entry selected.
			if (methodNames.Contains(selectedMethodName))
			{
				selectedIndex = methodNames.IndexOf(selectedMethodName);
			}

			// Get any new selection from the inspector.
			int newSelectedIndex = EditorGUI.Popup(position, selectedIndex, methodNames.ToArray());

			// If nothing is selected we want to force something.
			if (newSelectedIndex < 0)
			{
				newSelectedIndex = 0;
			}

			// If the selection has changed, apply that to the target script.
			if (newSelectedIndex != selectedIndex)
			{
				methodNameSerializedProperty.stringValue = methodNames[newSelectedIndex];
			}

			EditorGUI.EndProperty();

		}*/
	}
}