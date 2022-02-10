using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem
{
	public class VariableStore : MonoBehaviour
	{
		[Header("Settings")]
		/// <summary>
		/// If true, the Awake method will add the variables attached to this game object automatically.
		/// </summary>
		[Tooltip("If true, the Awake method will add the variables attached to this game object automatically.")]
		public bool autoAddVariablesOnGameObject = true;

		public List<Variable> variableList = new List<Variable>();

		private Dictionary<VariableName, List<Variable>> variables = new Dictionary<VariableName, List<Variable>>();

		public class InvalidVariableException : Exception { public InvalidVariableException(string message) : base(message) { } }

		public class VariableState
		{
			public string variableName = string.Empty;
			public string guid = string.Empty;
			public string value = string.Empty;
		}

		public string Save()
		{
			List<VariableState> states = new List<VariableState>();

			foreach (KeyValuePair<VariableName, List<Variable>> pair in variables)
			{
				for (int i = 0; i < pair.Value.Count; i++)
				{
					Variable variable = pair.Value[i];

					states.Add(new VariableState { variableName = pair.Key.variableName, guid = variable.guid, value = variable.GetValueAsString() });
				}
			}

			// Serialize to xml.
			return XmlSerialization.SerializeToXmlString(states);
		}

		public void Load(string state)
		{
			// Get the state from the saved string.
			List<VariableState> states = XmlSerialization.DeserializeFromXmlString<List<VariableState>>(state);

			foreach (KeyValuePair<VariableName, List<Variable>> variablesPair in variables)
			{
				for (int i = 0; i < states.Count; i++)
				{
					if (states[i].variableName == variablesPair.Key.variableName)
					{
						for (int j = 0; j < variablesPair.Value.Count; j++)
						{
							if (states[i].guid == variablesPair.Value[j].guid)
							{
								variablesPair.Value[j].SetValueFromString(states[i].value);
							}
						}
					}
				}
			}
		}

		public void Awake()
		{
			if (autoAddVariablesOnGameObject)
			{
				Variable[] variables = GetComponents<Variable>();

				for (int i = 0; i < variables.Length; i++)
				{
					if (!variableList.Contains(variables[i]))
					{
						variableList.Add(variables[i]);
					}
				}
			}

			// Generate a dictionary for quick look up of variables.
			for (int count = 0; count < variableList.Count; count++)
			{
				// Check that the entry in the variable list exists.
				// There can be null entries due to human error/use.
				if (variableList[count] != null)
				{
					AddVariable(variableList[count]);
				}
			}
		}

		public void RegisterOnVariableSetCallback(Action onVariableSet)
		{
			foreach (KeyValuePair<VariableName, List<Variable>> pair in variables)
			{
				for (int i = 0; i < pair.Value.Count; i++)
				{
					pair.Value[i].OnVariableSet +=
						() =>
						{
							onVariableSet?.Invoke();
						};
				}
			}
		}

		public void AddVariable(Variable variable)
		{
			if (variable.Key == null)
			{
				throw new InvalidVariableException("Variables must have a VariableName assigned to them.");
			}

			if (!variables.ContainsKey(variable.Key))
			{
				variables.Add(variable.Key, new List<Variable>());
			}

			variables[variable.Key].Add(variable);
		}

		public T GetFirstVariableOrDefault<T>(VariableName key) where T : Variable
		{
			return GetFirstVariableOrDefault<T>(key.variableName);
		}

		public T GetFirstVariableOrDefault<T>(string key) where T : Variable
		{
			IEnumerable<KeyValuePair<VariableName, List<Variable>>> matchingVariables = variables.Where((pair, index) => pair.Key.variableName == key);

			foreach (KeyValuePair<VariableName, List<Variable>> pair in matchingVariables)
			{
				// Iterate all variables tied to that variable name.
				for (int variableCount = 0; variableCount < pair.Value.Count; variableCount++)
				{
					// If the type of variable is the desired type.
					if (pair.Value[variableCount] is T)
					{
						// Add it to the return list as the desired type.
						return pair.Value[variableCount] as T;
					}
				}
			}

			return default;
		}

		public List<T> GetVariables<T>(VariableName key) where T : Variable
		{
			return GetVariables<T>(key.variableName);
		}

		public List<T> GetVariables<T>(string key) where T : Variable
		{
			List<T> values = new List<T>();

			IEnumerable<KeyValuePair<VariableName, List<Variable>>> matchingVariables = variables.Where((pair, index) => pair.Key.variableName == key);

			foreach (KeyValuePair<VariableName, List<Variable>> pair in matchingVariables)
			{
				// Iterate all variables tied to that variable name.
				for (int variableCount = 0; variableCount < pair.Value.Count; variableCount++)
				{
					// If the type of variable is the desired type.
					if (pair.Value[variableCount] is T)
					{
						// Add it to the return list as the desired type.
						values.Add(pair.Value[variableCount] as T);
					}
				}
			}

			return values;
		}

		public List<T> GetAllVariables<T>() where T : Variable
		{
			List<T> values = new List<T>();

			// For each variable key and any associated variables.
			foreach (KeyValuePair<VariableName, List<Variable>> pair in variables)
			{
				// Iterate the attached variables.
				for (int i = 0; i < pair.Value.Count; i++)
				{
					// If its the correct type.
					if (pair.Value[i] is T)
					{
						// Add it as a returned variable.
						values.Add(pair.Value[i] as T);
					}
				}
			}

			// Return all variables.
			return values;
		}
	}
}