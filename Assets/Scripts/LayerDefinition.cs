using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
	public class LayerDefinition : MonoBehaviour
	{
		/// <summary>
		/// The type of this layer.
		/// </summary>
		public Sequencer.SequencerLayer.Types layerType = Sequencer.SequencerLayer.Types.Slave;

		/// <summary>
		/// The root of the layer.
		/// </summary>
		[SerializeField]
		private NarrativeObject rootNarrativeObject = null;

		/// <summary>
		/// An delay in seconds, from the time that the layer which this definition belongs to is processed, to the start of processing this layer definition.
		/// </summary>
		public float layerStartTimeOffset = 0.0f;

		/// <summary>
		/// Whether this layers processing is triggered by a start processing event.
		/// </summary>
		public bool isTriggered = false;

		/// <summary>
		/// Whether this layers processing has been triggered.
		/// </summary>
		private bool HasBeenTriggered { get; set; } = false;

		private float? TriggeredTime { get; set; } = null;

		/// <summary>
		/// Resets the triggered status for this object.
		/// </summary>
		private void ResetHasBeenTriggered()
		{
			// If this layer is not to be triggered, then it has been triggered automatically.
			HasBeenTriggered = !isTriggered;

			TriggeredTime = null;
		}

		/// <summary>
		/// Handler for triggering this layer definition to start processing.
		/// </summary>
		public void HandleOnTriggered()
		{
			HasBeenTriggered = true;
		}

		/// <summary>
		/// Handler for triggering this layer definition to start processing.
		/// </summary>
		public void HandleOnTriggered(float triggeredTime)
		{
			HasBeenTriggered = true;
			TriggeredTime = triggeredTime;
		}

		/// <summary>
		/// Processing routine.
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public IEnumerator Process(dynamic parameters)
		{
			// Whenever processing starts, reset the triggered status for this layer definition.
			ResetHasBeenTriggered();

			Sequencer sequencer = parameters.sequencer;
			Sequencer.SequencerLayer sequencerLayer = parameters.sequencerLayer;

			if (rootNarrativeObject == null)
			{
				throw new InvalidRootNarrativeObjectException("LayerDefinition does not define a root narrative object.");
			}

			// If this layer has not been triggered then processing waits here for that to begin.
			while (!HasBeenTriggered)
			{
				yield return new WaitForEndOfFrame();
			}

			if (TriggeredTime == null)
			{
				// Just before the layer starts playing, set the point at which it "starts" sequencing from.
				// Normally this would be zero but if a layer isnt triggered immediately, then 
				// it doesnt start sequencing from 0 seconds as the playhead has been moving forward 
				// while it has been waiting to start processing.
				sequencerLayer.SetLayerEndTime(sequencer.PlayheadTime);
			}
			else
			{
				sequencerLayer.SetLayerEndTime((float)TriggeredTime);
			}

			yield return rootNarrativeObject.Process(sequencer, sequencerLayer);
		}
	}
}