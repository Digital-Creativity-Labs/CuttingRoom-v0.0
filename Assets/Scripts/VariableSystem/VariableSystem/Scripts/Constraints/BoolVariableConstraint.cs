using System.Collections.Generic;
using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Constraints
{
    public class BoolVariableConstraint : Constraint
    {
        public enum ComparisonType
        {
            Undefined,
            EqualTo,
            NotEqualTo,
        }

        [SerializeField]
        private ComparisonType comparisonType = ComparisonType.Undefined;

        public bool value = false;

        public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
        {
            return Evaluate<BoolVariableConstraint, BoolVariable>(sequencer, narrativeSpace, narrativeObject, comparisonType.ToString());
        }

        public bool EqualTo(List<BoolVariable> boolVariables)
        {
			for (int boolVariableCount = 0; boolVariableCount < boolVariables.Count; boolVariableCount++)
			{
				if (boolVariables[boolVariableCount].Value == value)
				{
					return true;
				}
			}

			return false;
        }

        public bool NotEqualTo(List<BoolVariable> boolVariables)
        {
            return !EqualTo(boolVariables);
        }
    }
}