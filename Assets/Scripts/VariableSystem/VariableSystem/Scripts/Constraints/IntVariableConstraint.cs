using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class IntVariableConstraint : Constraint
	{
		// TODO: This should ideally be a bitmask with equals, less, greater.
		//		 Pain in the butt to implement and not worth it unless it becomes easier!
		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			LessThan,
			GreaterThan,
			LessThanOrEqualTo,
			GreaterThanOrEqualTo,
		}

		[SerializeField]
		private ComparisonType comparisonType = ComparisonType.Undefined;

		/// <summary>
		/// Value of this constraint.
		/// </summary>
		public int value = 0;

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<IntVariableConstraint, IntVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(List<IntVariable> intVariables)
		{
			for (int intVariableCount = 0; intVariableCount < intVariables.Count; intVariableCount++)
			{
				if (intVariables[intVariableCount].Value == value)
				{
					return true;
				}
			}

			return false;
		}

		public bool NotEqualTo(List<IntVariable> intVariables)
		{
			return !EqualTo(intVariables);
		}

		public bool LessThan(List<IntVariable> intVariables)
		{
			for (int intVariableCount = 0; intVariableCount < intVariables.Count; intVariableCount++)
			{
				if (intVariables[intVariableCount].Value < value)
				{
					return true;
				}
			}

			return false;
		}

		public bool GreaterThan(List<IntVariable> intVariables)
		{
			for (int intVariableCount = 0; intVariableCount < intVariables.Count; intVariableCount++)
			{
				if (intVariables[intVariableCount].Value > value)
				{
					return true;
				}
			}

			return false;
		}

		public bool LessThanOrEqualTo(List<IntVariable> intVariables)
		{
			return LessThan(intVariables) || EqualTo(intVariables);
		}

		public bool GreaterThanOrEqualTo(List<IntVariable> intVariables)
		{
			return GreaterThan(intVariables) || EqualTo(intVariables);
		}
	}
}