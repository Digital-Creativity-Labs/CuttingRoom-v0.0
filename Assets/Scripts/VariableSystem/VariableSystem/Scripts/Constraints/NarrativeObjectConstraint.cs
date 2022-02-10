using UnityEngine;
using CuttingRoom.VariableSystem.Variables;
using System.Collections.Generic;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class NarrativeObjectConstraint : Constraint
	{
		public enum ConstraintType
		{
			Undefined,
			CannotBeSequencedBefore,
			CannotBeSequencedAfter,
		}

		public ConstraintType constraintType = ConstraintType.Undefined;

		public List<NarrativeObject> narrativeObjectsToConstrainAgainst = new List<NarrativeObject>();

		public override bool Evaluate(Sequencer sequencer, NarrativeSpace narrativeSpace, NarrativeObject narrativeObject)
		{
			switch (constraintType)
			{
				case ConstraintType.CannotBeSequencedBefore:

					return CannotBeSequencedBefore(sequencer, narrativeObjectsToConstrainAgainst);

				case ConstraintType.CannotBeSequencedAfter:

					return CannotBeSequencedAfter(sequencer, narrativeObjectsToConstrainAgainst);
			}

			return false;
		}

		private bool CannotBeSequencedBefore(Sequencer sequencer, List<NarrativeObject> narrativeObjectVariable)
		{
			for (int narrativeObjectVariableCount = 0; narrativeObjectVariableCount < narrativeObjectVariable.Count; narrativeObjectVariableCount++)
			{
				if (sequencer.HasNarrativeObjectBeenSequenced(narrativeObjectVariable[narrativeObjectVariableCount]))
				{
					return true;
				}
			}

			return false;
		}

		private bool CannotBeSequencedAfter(Sequencer sequencer, List<NarrativeObject> narrativeObjectVariable)
		{
			return !CannotBeSequencedBefore(sequencer, narrativeObjectVariable);
		}
	}
}