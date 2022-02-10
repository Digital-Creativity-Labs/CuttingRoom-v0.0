using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class StringVariableConstraint : Constraint
	{
		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			Contains,
			DoesNotContain,
		}

		public ComparisonType comparisonType = ComparisonType.Undefined;

		public string value = string.Empty;

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<StringVariableConstraint, StringVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(List<StringVariable> stringVariables)
		{
			for (int stringVariableCount = 0; stringVariableCount < stringVariables.Count; stringVariableCount++)
			{
				if (stringVariables[stringVariableCount].Value.Equals(value))
				{
					return true;
				}
			}

			return false;
		}

		public bool NotEqualTo(List<StringVariable> stringVariables)
		{
			return !EqualTo(stringVariables);
		}

		public bool Contains(List<StringVariable> stringVariables)
		{
			for (int stringVariableCount = 0; stringVariableCount < stringVariables.Count; stringVariableCount++)
			{
				if (stringVariables[stringVariableCount].Value.Contains(value))
				{
					return true;
				}
			}

			return false;
		}

		public bool DoesNotContain(List<StringVariable> stringVariables)
		{
			return !Contains(stringVariables);
		}
	}
}