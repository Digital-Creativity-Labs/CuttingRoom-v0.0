using System;
using UnityEditor;
using CuttingRoom.VariableSystem;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
	[CustomEditor(typeof(VariableEvent))]
	public class VariableEventInspector : UnityEditor.Editor
	{
		private const int widthForLabels = 224;

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			VariableEvent variableEvent = target as VariableEvent;

			if (variableEvent.variable != null)
			{
				List<string> variableEventMethods = new List<string>();

				// Get the type of the variable event.
				Type variableType = variableEvent.variable.GetType();

				// Get the methods in the type which are marked with the VariableEventMethod attribute.
				IEnumerable<MethodInfo> methodInfos = variableType.GetMethods().Where(methodInfo => methodInfo.GetCustomAttributes<VariableEventMethodAttribute>().Count() > 0);

				// For each method found, add its name to the possibilities.
				foreach (MethodInfo methodInfo in methodInfos)
				{
					variableEventMethods.Add(methodInfo.Name);
				}

				EditorGUILayout.BeginHorizontal();

				// Width keeps the inspector looking tidy.
				EditorGUILayout.LabelField("Method", GUILayout.Width(widthForLabels));

				// Get the index of the currently set value for the method on the variable event.
				int previousSelection = !string.IsNullOrEmpty(variableEvent.methodName) ? variableEventMethods.IndexOf(variableEvent.methodName) : -1;

				// Do a popup and find the index of whatever is selected.
				int currentSelection = EditorGUILayout.Popup(previousSelection, variableEventMethods.ToArray());

				// If selection has changed after rendering the popup, something new was selected. Otherwise it hasnt changed.
				if (currentSelection != previousSelection)
				{
					// Save the name of the method to the instance of variable event.s
					variableEvent.methodName = variableEventMethods[currentSelection];

					// Reset the lists of parameters as the method has changed.
					variableEvent.OnMethodNameChanged();
				}

				EditorGUILayout.EndHorizontal();

				if (currentSelection >= 0)
				{
					MethodInfo selectedMethodInfo = methodInfos.ElementAt(currentSelection);

					// This dictionary is used to keep counts for the types of variables a method has.
					Dictionary<Type, int> parameterCounts = new Dictionary<Type, int>()
					{
						{ typeof(bool), 0 },
						{ typeof(int), 0 },
						{ typeof(float), 0 },
						{ typeof(string), 0 },
						{ typeof(UnityEngine.Object), 0 },
					};

					ParameterInfo[] parameterInfos = selectedMethodInfo.GetParameters();

					// Whether one of the parameter values has changed. If so, then set dirty to serialize.
					bool setDirty = false;

					foreach (ParameterInfo parameterInfo in parameterInfos)
					{
						// Get the type of the parameter being inspected.
						Type parameterType = parameterInfo.ParameterType;

						// The type to look up in the parameter count dictionary.
						// This is required as objects which inherit UnityEngine.Object class won't have that type,
						// but should be counted in the dictionary as that type (and later we need to know their derived type).
						Type parameterLookupType = default;

						if (VariableEvent.InheritsUnityEngineObjectType(parameterType))
						{
							parameterLookupType = typeof(UnityEngine.Object);
						}
						else
						{
							parameterLookupType = parameterType;
						}

						int parameterTypeCount = 0;

						// Get the count of variables for the type required.
						if (parameterCounts.ContainsKey(parameterLookupType))
						{
							// Increase number required of that type by 1.
							parameterCounts[parameterLookupType]++;

							// Get the number required.
							parameterTypeCount = parameterCounts[parameterLookupType];
						}

						// Create variables and show editor gui of correct type.
						if (parameterType == typeof(bool))
						{
							if (variableEvent.boolParameters.Count < parameterTypeCount)
							{
								variableEvent.boolParameters.Add(false);
							}

							bool previousValue = variableEvent.boolParameters[parameterTypeCount - 1];

							EditorGUILayout.BeginHorizontal();

							bool currentValue = EditorGUILayout.Toggle(parameterInfo.Name, previousValue);

							EditorGUILayout.EndHorizontal();

							if (previousValue != currentValue)
							{
								variableEvent.boolParameters[parameterTypeCount - 1] = currentValue;

								setDirty = true;
							}
						}
						else if (parameterType == typeof(int))
						{
							if (variableEvent.intParameters.Count < parameterTypeCount)
							{
								variableEvent.intParameters.Add(0);
							}

							int previousValue = variableEvent.intParameters[parameterTypeCount - 1];

							EditorGUILayout.BeginHorizontal();

							int currentValue = EditorGUILayout.IntField(parameterInfo.Name, previousValue);

							EditorGUILayout.EndHorizontal();

							if (previousValue != currentValue)
							{
								variableEvent.intParameters[parameterTypeCount - 1] = currentValue;

								setDirty = true;
							}
						}
						else if (parameterType == typeof(float))
						{
							if (variableEvent.floatParameters.Count < parameterTypeCount)
							{
								variableEvent.floatParameters.Add(0.0f);
							}

							float previousValue = variableEvent.floatParameters[parameterTypeCount - 1];

							EditorGUILayout.BeginHorizontal();

							float currentValue = EditorGUILayout.FloatField(parameterInfo.Name, previousValue);

							EditorGUILayout.EndHorizontal();

							if (previousValue != currentValue)
							{
								variableEvent.floatParameters[parameterTypeCount - 1] = currentValue;

								setDirty = true;
							}
						}
						else if (parameterType == typeof(string))
						{
							if (variableEvent.stringParameters.Count < parameterTypeCount)
							{
								variableEvent.stringParameters.Add(string.Empty);
							}

							string previousValue = variableEvent.stringParameters[parameterTypeCount - 1];

							EditorGUILayout.BeginHorizontal();

							string currentValue = EditorGUILayout.TextField(parameterInfo.Name, previousValue);

							EditorGUILayout.EndHorizontal();

							if (previousValue != currentValue)
							{
								variableEvent.stringParameters[parameterTypeCount - 1] = currentValue;

								setDirty = true;
							}
						}
						else if (parameterType == typeof(UnityEngine.Object) || VariableEvent.InheritsUnityEngineObjectType(parameterType))
						{
							if (variableEvent.objectParameters.Count < parameterTypeCount)
							{
								variableEvent.objectParameters.Add(null);
							}

							EditorGUILayout.BeginHorizontal();

							UnityEngine.Object previousValue = variableEvent.objectParameters[parameterTypeCount - 1];

							EditorGUILayout.LabelField(parameterInfo.Name, GUILayout.Width(widthForLabels));

							UnityEngine.Object currentValue = EditorGUILayout.ObjectField(previousValue, parameterType, true);

							EditorGUILayout.EndHorizontal();

							if (previousValue != currentValue)
							{
								variableEvent.objectParameters[parameterTypeCount - 1] = currentValue;

								setDirty = true;
							}
						}
						else
						{
							Debug.LogError($"Variable Events do not support the type {parameterType.Name} as a parameter.");
						}
					}

					if (setDirty)
					{
						EditorUtility.SetDirty(variableEvent);
					}
				}
			}
		}
	}
}