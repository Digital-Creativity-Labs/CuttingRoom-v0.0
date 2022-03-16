using System;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace CuttingRoom
{
	/// <summary>
	/// The layer narrative object processes several narrative objects in parallel, allowing for composition or concurrent processing.
	/// </summary>
	[RequireComponent(typeof(LayerSelectionDecisionPoint))]
	public class LayerNarrativeObject : NarrativeObject
	{
		/// <summary>
		/// Thrown when more than one master layer is selected in a layer narrative object.
		/// </summary>
		public class MultipleMasterLayersException : Exception { public MultipleMasterLayersException(string message) : base(message) { } }

		/// <summary>
		/// Contains the settings for selecting which layers are processed from the layer narrative object.
		/// </summary>
		public LayerSelectionDecisionPoint layerSelectionDecisionPoint = null;

		/// <summary>
		/// The definitions for the layers which make up this layer narrative object.
		/// </summary>
		[SerializeField]
		private LayerDefinition[] layerDefinitions = null;

		/// <summary>
		/// The coroutines which run in parallel to process each of the selected layers within this object.
		/// </summary>
		private List<Coroutine> layerProcessingCoroutines = new List<Coroutine>();

		// Selected layers are put here.
		private List<LayerDefinition> selectedLayerDefinitions = new List<LayerDefinition>();

		/// <summary>
		/// The processing loop for the layer narrative object.
		/// </summary>
		/// <param name="sequencer"></param>
		/// <param name="sequencerLayer"></param>
		/// <returns></returns>
		public override IEnumerator Process(Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer)
		{
#if UNITY_EDITOR
			// Check for multiple masters in editor only as it is a sanity check for users.
			IEnumerable<LayerDefinition> masterLayers = layerDefinitions.Where(layerDefinition => layerDefinition.layerType == Sequencer.SequencerLayer.Types.Master);

			if (masterLayers.Count() > 1)
			{
				throw new MultipleMasterLayersException("Multiple master layers are defined. There can be only one.");
			}
#endif
			// Remove any layers selected if this object has been processed before.
			selectedLayerDefinitions.Clear();

			this.sequencer = sequencer;
			this.sequencerLayer = sequencerLayer;

			sequencer.RegisterNarrativeObjectSequenced(this);

			// Callback for processing starting.
			InvokeOnProcessStart(sequencerLayer.GetLayerEndTime());

			// Callback with selected layers.
			OnSelectedCallback onSelected = OnSelected;

			// Wait for layer selection to complete.
			yield return layerSelectionDecisionPoint.Process(onSelected, sequencer, sequencerLayer, this);

			if (selectedLayerDefinitions.Count == 0)
			{
				Debug.LogWarning($"The LayerSelectionDecisionPoint attached to the GameObject called {gameObject.name} did not make a selection.");
			}

			// Sub layers created for this object.
			List<Sequencer.SequencerLayer> subSequencerLayers = new List<Sequencer.SequencerLayer>();

			// For each selected layer.
			for (int layerDefinitionCount = 0; layerDefinitionCount < selectedLayerDefinitions.Count; layerDefinitionCount++)
			{
				subSequencerLayers.Add(sequencer.CreateSubLayer(sequencerLayer, selectedLayerDefinitions[layerDefinitionCount].layerStartTimeOffset, selectedLayerDefinitions[layerDefinitionCount].layerType));
			}

			// For each selected layer.
			for (int subSequencerLayerCount = 0; subSequencerLayerCount < subSequencerLayers.Count; subSequencerLayerCount++)
			{
				// TODO: Possibly require a cancellation token source here.
				// Start a coroutine processing the layer.
				Coroutine layerProcessing = StartCoroutine(selectedLayerDefinitions[subSequencerLayerCount].Process(new { sequencer, sequencerLayer = subSequencerLayers[subSequencerLayerCount] }));

				// Store a reference to the coroutine.
				layerProcessingCoroutines.Add(layerProcessing);
			}

			// Wait for all of the coroutines to complete. Order shouldnt matter as if a later coroutine completes 
			// before an earlier one in the list it should immediately yield and move to looking at the next one.
			for (int coroutineCount = 0; coroutineCount < layerProcessingCoroutines.Count; coroutineCount++)
			{
				yield return layerProcessingCoroutines[coroutineCount];
			}

			// Find longest layer and add that to the parent sequencer layer (to push it to the end of the timeline).
			sequencerLayer.SetLayerEndTime(sequencerLayer.GetLayerEndTime());

			// Find a master if it exists.
			Sequencer.SequencerLayer masterLayer = sequencerLayer.GetMasterLayer();

			// If there is a master, then other layers need trimmed.
			if (masterLayer != null)
			{
				// Get end time of master layer.
				float masterLayerEndTime = masterLayer.GetLayerEndTime();

				List<SequencedAtomicNarrativeObject> sequencedAtomicNarrativeObjects = new List<SequencedAtomicNarrativeObject>();

				sequencerLayer.GetSequencedAtomicNarrativeObjectsRecursively(sequencedAtomicNarrativeObjects);

				for (int sequencedCount = 0; sequencedCount < sequencedAtomicNarrativeObjects.Count; sequencedCount++)
				{
					if (sequencedAtomicNarrativeObjects[sequencedCount].playheadFinishTime > masterLayerEndTime)
					{
						sequencedAtomicNarrativeObjects[sequencedCount].SetPlayheadFinishTime(masterLayerEndTime);
					}
				}
			}

			// Do output selection and other base activities.
			yield return base.Process(sequencer, sequencerLayer);

			// Callback for processing finishing.
			InvokeOnProcessFinish(sequencerLayer.GetLayerEndTime());
		}

		private IEnumerator OnSelected(object selections)
		{
			if (selections != null)
			{
				if (selections is LayerDefinition[])
				{
					selectedLayerDefinitions.AddRange(selections as LayerDefinition[]);
				}
			}

			yield return null;
		}
	}
}