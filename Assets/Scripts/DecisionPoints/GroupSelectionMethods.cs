using CuttingRoom.VariableSystem.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public partial class GroupSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Selects a random candidate from the group.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Random(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			yield return StartCoroutine(methodReferences.onSelected(methodReferences.candidates.Count > 0 ? methodReferences.candidates[UnityEngine.Random.Range(0, methodReferences.candidates.Count)] : null));
		}

		/// <summary>
		/// Selects a number of candidates from the group, specified in the selectionCount field.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Count(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			GroupSelectionDecisionPoint groupSelectionDecisionPoint = methodReferences.decisionPoint as GroupSelectionDecisionPoint;

			if (groupSelectionDecisionPoint.selectedNarrativeObjects.Count < groupSelectionDecisionPoint.selectionCount)
			{
				yield return Random(parameters);
			}

			yield return null;
		}

		/// <summary>
		/// Select based on constraints on the object.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Select(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			List<NarrativeObject> selectedNarrativeObjects = Solver.Solve(methodReferences.sequencer, methodReferences.narrativeSpace, methodReferences.candidates, methodReferences.decisionPoint.constraints);

			NarrativeObject narrativeObject = selectedNarrativeObjects.Count > 0 ? selectedNarrativeObjects[0] : null;

			yield return StartCoroutine(methodReferences.onSelected(narrativeObject));
		}

		/// <summary>
		/// Select a unique narrative object based on constraints.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator SelectUnique(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			List<NarrativeObject> selectedNarrativeObjects = Solver.Solve(methodReferences.sequencer, methodReferences.narrativeSpace, methodReferences.candidates, methodReferences.decisionPoint.constraints);

			GroupSelectionDecisionPoint groupSelectionDecisionPoint = methodReferences.decisionPoint as GroupSelectionDecisionPoint;

			// For each selected object, make sure it hasnt already been selected before.
			for (int selectedNarrativeObjectCount = selectedNarrativeObjects.Count - 1; selectedNarrativeObjectCount >= 0; selectedNarrativeObjectCount--)
			{
				// If the group selection decision point has already selected the narrative object, then its not a valid selection.
				if (groupSelectionDecisionPoint.selectedNarrativeObjects.Contains(selectedNarrativeObjects[selectedNarrativeObjectCount]))
				{
					// Remove the previously selected narrative object.
					selectedNarrativeObjects.RemoveAt(selectedNarrativeObjectCount);
				}
			}

			NarrativeObject narrativeObject = selectedNarrativeObjects.Count > 0 ? selectedNarrativeObjects[0] : null;

			yield return StartCoroutine(methodReferences.onSelected(narrativeObject));
		}
	}
}
