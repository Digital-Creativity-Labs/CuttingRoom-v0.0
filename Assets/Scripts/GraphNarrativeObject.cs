using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
    public class GraphNarrativeObject : NarrativeObject
    {
        [SerializeField]
        private NarrativeObject rootNarrativeObject = null;

        public override IEnumerator Process(Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer)
        {
            if (rootNarrativeObject == null)
            {
                throw new InvalidRootNarrativeObjectException("Graph Narrative Object has no root narrative object defined.");
            }

            // Callback for processing starting.
            InvokeOnProcessStart(sequencerLayer.GetLayerEndTime());

            // Process from the defined root.
            yield return rootNarrativeObject.Process(sequencer, sequencerLayer);

            // Process the base functionality, output selection.
            yield return base.Process(sequencer, sequencerLayer);

            // Callback for processing finishing.
            InvokeOnProcessFinish(sequencerLayer.GetLayerEndTime());
        }
    }
}