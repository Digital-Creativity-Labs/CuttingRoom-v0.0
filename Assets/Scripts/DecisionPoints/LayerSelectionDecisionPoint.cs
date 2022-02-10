using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// A container for holding the logic required to select groups within a layer narrative object.
	/// </summary>
	public partial class LayerSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// A container for the layer selection method definition.
		/// </summary>
		[Serializable]
		public class LayerSelectionMethodNameContainer : MethodNameContainer { }

		/// <summary>
		/// The layer selection method definition for this instance of the decision point.
		/// </summary>
		[HelpBox("Built in Layer Selection Methods:\n\n• All - Selects all candidates for processing.\n• Random - Selects randomly and returns selected candidates for processing.", HelpBoxMessageType.Info)]
		public LayerSelectionMethodNameContainer layerSelectionMethodNameContainer = null;

		/// <summary>
		/// Initialisation.
		/// </summary>
		private new void Awake()
		{
			layerSelectionMethodNameContainer.methodClass = typeof(LayerSelectionDecisionPoint).AssemblyQualifiedName;

			layerSelectionMethodNameContainer.Init();

			base.Awake();
		}

		/// <summary>
		/// Processing loop.
		/// </summary>
		/// <param name="onSelected"></param>
		/// <param name="sequencer"></param>
		/// <returns></returns>
		public IEnumerator Process(OnSelectedCallback onSelected, Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer, LayerNarrativeObject layerNarrativeObject)
		{
			// If ready to select some layers...
			if (layerSelectionMethodNameContainer.Initialised)
			{
				// ...and some layers are available to be selected.
				if (candidates.Count > 0)
				{
					yield return StartCoroutine(layerSelectionMethodNameContainer.methodInfo.Name, new object[] { this, onSelected, sequencer, sequencerLayer, layerNarrativeObject });
				}
			}
		}
	}
}
