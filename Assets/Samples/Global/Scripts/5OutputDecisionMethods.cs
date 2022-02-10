using System;
using System.Collections;

namespace CuttingRoom
{
	public partial class OutputSelectionDecisionPoint : DecisionPoint
	{
		public IEnumerator SelectBasedOnSeconds(object[] parameters)
		{
			// This converts the object parameters into the correct types for use in output selection.
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			// Find out if the current second hand on the clock is on an odd or even number.
			// This is just a simple example of some logic you could base an output selection on.
			bool currentSecondIsEven = DateTime.Now.Second % 2 == 0;

			if (currentSecondIsEven)
			{
				yield return StartCoroutine(methodReferences.onSelected(methodReferences.candidates[0]));
			}
			else
			{
				yield return StartCoroutine(methodReferences.onSelected(methodReferences.candidates[1]));
			}

			yield return null;
		}
	}
}