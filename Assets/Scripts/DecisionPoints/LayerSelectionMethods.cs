using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CuttingRoom
{
	public partial class LayerSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Selects all candidates for processing.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator All(object[] parameters)
		{
			MethodReferences<LayerDefinition> methodReferences = Utilities.ConvertParameters<LayerDefinition>(parameters);

			yield return StartCoroutine(methodReferences.onSelected(methodReferences.candidates.ToArray()));
		}

		/// <summary>
		/// Selects a random sample of candidates for processing.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Random(object[] parameters)
		{
			MethodReferences<LayerDefinition> methodReferences = Utilities.ConvertParameters<LayerDefinition>(parameters);

			// Get the number of layers to select from the available possibilities.
			int numberToSelect = UnityEngine.Random.Range(1, methodReferences.candidates.Count);

			// Return values added here.
			List<LayerDefinition> selectedLayerDefinitions = new List<LayerDefinition>();

			// If some selection should be made, then do some selection.
			if (numberToSelect > 0)
			{
				// Dupe the list of possibilities.
				List<LayerDefinition> layerDefinitions = new List<LayerDefinition>(methodReferences.candidates);

				// Sort the list of possibilities by random values, take the number required and add them to the list for return values.
				selectedLayerDefinitions.AddRange(layerDefinitions.OrderBy(x => UnityEngine.Random.Range(int.MinValue, int.MaxValue)).Take(numberToSelect));
			}

			// Convert the selected layers to an array and call selection callback.
			yield return StartCoroutine(methodReferences.onSelected(selectedLayerDefinitions.ToArray()));
		}
	}
}
