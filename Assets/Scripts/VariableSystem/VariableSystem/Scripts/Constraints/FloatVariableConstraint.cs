using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class FloatVariableConstraint : Constraint
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
		[SerializeField]
		private float value = 0;

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			return Evaluate<FloatVariableConstraint, FloatVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
		}

		public bool EqualTo(List<FloatVariable> floatVariables)
		{
			for (int floatVariableCount = 0; floatVariableCount < floatVariables.Count; floatVariableCount++)
			{
				if (floatVariables[floatVariableCount].Value == value)
				{
					return true;
				}
			}

			return false;
		}

		public bool NotEqualTo(List<FloatVariable> floatVariables)
		{
			return !EqualTo(floatVariables);
		}

		public bool LessThan(List<FloatVariable> floatVariables)
		{
			for (int floatVariableCount = 0; floatVariableCount < floatVariables.Count; floatVariableCount++)
			{
				if (floatVariables[floatVariableCount].Value < value)
				{
					return true;
				}
			}

			return false;
		}

		public bool GreaterThan(List<FloatVariable> floatVariables)
		{
			for (int floatVariableCount = 0; floatVariableCount < floatVariables.Count; floatVariableCount++)
			{
				if (floatVariables[floatVariableCount].Value > value)
				{
					return true;
				}
			}

			return false;
		}

		public bool LessThanOrEqualTo(List<FloatVariable> floatVariables)
		{
			return LessThan(floatVariables) || EqualTo(floatVariables);
		}

		public bool GreaterThanOrEqualTo(List<FloatVariable> floatVariables)
		{
			return GreaterThan(floatVariables) || EqualTo(floatVariables);
		}
	}
}