using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class Constraint : MonoBehaviour
	{
		/// <summary>
		/// Constraints to be subsequently applied if this constraint resolves as true.
		/// </summary>
		public List<Constraint> nestedConstraints = new List<Constraint>();

		public VariableStoreLocation variableStoreLocation = VariableStoreLocation.Undefined;
		
		// Variable name is important. This cannot be a reference to a variable as Narrative Objects may have a variable each, all with the same name but different values.
		/// <summary>
		/// The name of the variable this constraint will examine.
		/// </summary>
		public VariableName variableName = null;

		public virtual bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject) { throw new NotImplementedException("Constraint must implement Evaluate() method."); }

		protected bool Evaluate<T1, T2>(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject, string methodName) where T1 : Constraint where T2 : Variable
		{
			bool resolves = false;

			Type constraintType = typeof(T1);

			MethodInfo methodInfo = constraintType.GetMethod(methodName);

			if (methodInfo != null)
			{
				List<T2> variables = default;

				switch (variableStoreLocation)
				{
					case VariableStoreLocation.Global:

						variables = narrativeSpace.globalVariableStore.GetVariables<T2>(variableName);

						break;

					case VariableStoreLocation.NarrativeObject:

						variables = narrativeObject.variableStore.GetVariables<T2>(variableName);

						break;

					default:

						Debug.LogError("Cannot get variable.");

						return false;
				}

				if (variables != null)
				{
					List<object> parameters = new List<object>();

					// Get the parameters for the method to be invoked.
					ParameterInfo[] methodParameters = methodInfo.GetParameters();

					// For each parameter, work out if it is a supported parameter, if so add the relevant parameter to the list, else throw error as not supported.
					for (int methodParameterCount = 0; methodParameterCount < methodParameters.Length; methodParameterCount++)
					{
						Type parameterType = methodParameters[methodParameterCount].ParameterType;

						if (parameterType == typeof(Sequencer))
						{
							parameters.Add(sequencer);
						}
						else if (parameterType == typeof(NarrativeSpace))
						{
							parameters.Add(narrativeSpace);
						}
						else if (parameterType == typeof(List<T2>))
						{
							parameters.Add(variables);
						}
						else
						{
							throw new InvalidConstraintParameterException($"Variable of type: {parameterType.ToString()} is not supported as a parameter on constraint methods.");
						}
					}

					resolves = (bool)methodInfo.Invoke(this, parameters.ToArray());
				}
				else
				{
					Debug.LogError($"Variable named {variableName} was not found in VariableStore location {variableStoreLocation.ToString()}");
				}
			}

			if (resolves)
			{
				Queue<Constraint> nestedConstraintQueue = new Queue<Constraint>(nestedConstraints);

				while (resolves && nestedConstraintQueue.Count > 0)
				{
					Constraint nestedConstraint = nestedConstraintQueue.Dequeue();

					resolves = nestedConstraint.Evaluate(sequencer, narrativeSpace, narrativeObject);
				}
			}

			return resolves;
		}
	}
}