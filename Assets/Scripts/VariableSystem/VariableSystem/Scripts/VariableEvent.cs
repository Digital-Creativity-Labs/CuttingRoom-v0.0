using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Timeline;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem
{
	public class VariableEvent : MonoBehaviour
	{
		/// <summary>
		/// This enum specifies the name of the method to call for this event.
		/// Any variable can implement these methods as parameterless void
		/// methods and they will be invoked during an event using reflection.
		/// </summary>
		public enum Method
		{
			Undefined,
			Increment,
			Decrement,
			Set,
		}

		public Variable variable = null;

		// Hide this from the inspector as it is set via the VariableEventInspector editor.
		[HideInInspector]
		public string methodName = string.Empty;

		// These lists are used for serializing variables in the editor for the selected method.
		[HideInInspector]
		public List<bool> boolParameters = new List<bool>();
		[HideInInspector]
		public List<int> intParameters = new List<int>();
		[HideInInspector]
		public List<float> floatParameters = new List<float>();
		[HideInInspector]
		public List<string> stringParameters = new List<string>();
		[HideInInspector]
		public List<UnityEngine.Object> objectParameters = new List<UnityEngine.Object>();

#if UNITY_EDITOR

		public void OnMethodNameChanged()
		{
			boolParameters.Clear();
			intParameters.Clear();
			floatParameters.Clear();
			stringParameters.Clear();
			objectParameters.Clear();
		}

#endif

		public void Invoke()
		{
			if (string.IsNullOrEmpty(methodName))
			{
				Debug.LogError("Method to be invoked is not set.");

				return;
			}

			Type variableType = variable.GetType();

			// Reflect the method to be invoked.
			MethodInfo methodInfo = variableType.GetMethod(methodName);

			if (methodInfo == null)
			{
				Debug.LogWarning($@"The method ""{methodName}"" is not implemented by the {variable.GetType().Name} variable type.");
			}
			else
			{
				// This dictionary is used to keep counts for the types of variables a method has.
				Dictionary<Type, int> parameterCounts = new Dictionary<Type, int>()
					{
						{ typeof(bool), 0 },
						{ typeof(int), 0 },
						{ typeof(float), 0 },
						{ typeof(string), 0 },
						{ typeof(UnityEngine.Object), 0 },
					};

				ParameterInfo[] methodParameters = methodInfo.GetParameters();

				object[] parameters = new object[methodParameters.Length];

				for (int parameterCount = 0; parameterCount < methodParameters.Length; parameterCount++)
				{
					Type parameterType = methodParameters[parameterCount].ParameterType;

					if (InheritsUnityEngineObjectType(parameterType))
					{
						parameterType = typeof(UnityEngine.Object);
					}

					if (parameterCounts.ContainsKey(parameterType))
					{
						int lookupIndex = parameterCounts[parameterType];

						if (parameterType == typeof(bool))
						{
							parameters[parameterCount] = boolParameters[lookupIndex];
						}
						else if (parameterType == typeof(int))
						{
							parameters[parameterCount] = intParameters[lookupIndex];
						}
						else if (parameterType == typeof(float))
						{
							parameters[parameterCount] = floatParameters[lookupIndex];
						}
						else if (parameterType == typeof(string))
						{
							parameters[parameterCount] = stringParameters[lookupIndex];
						}
						else if (parameterType == typeof(UnityEngine.Object))
						{
							parameters[parameterCount] = objectParameters[lookupIndex];
						}

						// Increment the count of that type.
						parameterCounts[parameterType]++;
					}
					else
					{
						Debug.LogError($"Invalid parameter type: {parameterType.Name}");
					}
				}

				methodInfo.Invoke(variable, parameters);
			}
		}

		public static bool InheritsUnityEngineObjectType(Type type)
		{
			return type.IsSubclassOf(typeof(UnityEngine.Object));
		}
	}
}