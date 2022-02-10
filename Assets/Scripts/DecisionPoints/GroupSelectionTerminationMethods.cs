using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public partial class GroupSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Controls termination of the decision point. If all candidates of the group have been selected at least once, termination occurs, otherwise processing continues.
		/// </summary>
		/// <param name="decisionPoint"></param>
		/// <returns></returns>
		public static bool HasPlayedAll(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			GroupSelectionDecisionPoint groupSelectionDecisionPoint = methodReferences.decisionPoint as GroupSelectionDecisionPoint;

			for (int count = 0; count < methodReferences.decisionPoint.candidates.Count; count++)
			{
				NarrativeObject narrativeObject = methodReferences.decisionPoint.candidates[count].GetComponent<NarrativeObject>();

				if (narrativeObject != null)
				{
					if (!groupSelectionDecisionPoint.selectedNarrativeObjects.Contains(narrativeObject))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Controls termination of the decision point. Once the number of selections equals or exceeds the number specified in the playCount field, termination occurs, otherwise processing continues.
		/// </summary>
		/// <param name="decisionPoint"></param>
		/// <returns></returns>
		public static bool PlayCount(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			// Get derived decision point.
			GroupSelectionDecisionPoint groupSelectionDecisionPoint = methodReferences.decisionPoint as GroupSelectionDecisionPoint;

			if (groupSelectionDecisionPoint.selectedNarrativeObjects.Count < groupSelectionDecisionPoint.playCount)
			{
				return false;
			}

			return true;
		}
	}
}
