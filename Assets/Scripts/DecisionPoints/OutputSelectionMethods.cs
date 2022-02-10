using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Constraints;

namespace CuttingRoom
{
	public partial class OutputSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Selects a random output from the candidates available.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Random(object[] parameters)
		{
			MethodReferences<NarrativeObject> functionReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			NarrativeObject narrativeObject = functionReferences.candidates.Count > 0 ? functionReferences.candidates[UnityEngine.Random.Range(0, functionReferences.candidates.Count)] : null;

			yield return StartCoroutine(functionReferences.onSelected(narrativeObject));
		}

		/// <summary>
		/// Selects the first candidate specified as the next output.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator First(object[] parameters)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			NarrativeObject narrativeObject = methodReferences.candidates.Count > 0 ? methodReferences.candidates[0] : null;

			yield return StartCoroutine(methodReferences.onSelected(narrativeObject));
		}

		/// <summary>
		/// Wait for the end of the currently playing object, then select the first candidate.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator WaitThenFirst(object[] parameters)
		{
			yield return StartCoroutine(Wait(parameters));

			yield return First(parameters);
		}

		/// <summary>
		/// Select based on constraints on the object and its candidates..
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
		/// Wait for the end of the currently playing object, then select based on constraints on the object and its candidates.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator WaitThenSelect(object[] parameters)
		{
			yield return Wait(parameters);

			yield return Select(parameters);
		}

		/// <summary>
		/// Wait coroutine which looks at the layer end time and waits until that time (minus specified interval to allow selection).
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="sequenceSecondsBeforeLayerEndTime"></param>
		/// <returns></returns>
		private IEnumerator Wait(object[] parameters, float sequenceSecondsBeforeLayerEndTime = 1.0f)
		{
			MethodReferences<NarrativeObject> methodReferences = Utilities.ConvertParameters<NarrativeObject>(parameters);

			float sequencerLayerEndTime = methodReferences.sequencerLayer.GetLayerEndTime() - sequenceSecondsBeforeLayerEndTime;

			while (methodReferences.sequencer.PlayheadTime < sequencerLayerEndTime)
			{
				yield return new WaitForEndOfFrame();
			}
		}
	}
}