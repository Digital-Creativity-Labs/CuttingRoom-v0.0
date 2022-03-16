using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
    public class LayerTrigger : MonoBehaviour
    {
        /// <summary>
        /// Events which cause this trigger to activate.
        /// </summary>
        public enum TriggeringEvent
        {
            Undefined,
            OnProcessingStart,
            OnProcessingFinish,
            OnPlaybackStart,
            OnPlaybackFinish,
        }

        /// <summary>
        /// The layer definition which has its processing started by this trigger.
        /// </summary>
        [SerializeField]
        private LayerDefinition triggeredLayerDefinition = null;

        /// <summary>
        /// The object which activates this trigger.
        /// </summary>
        [SerializeField]
        private NarrativeObject triggeringNarrativeObject = null;

        /// <summary>
        /// The event which activates this trigger.
        /// </summary>
        [SerializeField]
        private TriggeringEvent triggeringEvent = TriggeringEvent.Undefined;

        private void Start()
        {
            // Set up callbacks for triggering event selected.
            switch (triggeringEvent)
            {
                case TriggeringEvent.OnProcessingStart:

                    triggeringNarrativeObject.OnProcessStart += HandleOnProcessTriggered;

                    break;

                case TriggeringEvent.OnProcessingFinish:

                    triggeringNarrativeObject.OnProcessFinish += HandleOnProcessTriggered;

                    break;

                case TriggeringEvent.OnPlaybackStart:

                    triggeringNarrativeObject.OnPlaybackStart += HandleOnPlaybackTriggered;

                    break;

                case TriggeringEvent.OnPlaybackFinish:

                    triggeringNarrativeObject.OnPlaybackFinish += HandleOnPlaybackTriggered;

                    break;

                default:

                    Debug.LogError($"Unhandled Triggering Event: {triggeringEvent.ToString()}");

                    break;
            }
        }

        /// <summary>
        /// Invoked from the selected triggering event.
        /// </summary>
        private void HandleOnPlaybackTriggered()
        {
            triggeredLayerDefinition.HandleOnTriggered();
        }
        
        /// <summary>
         /// Invoked from the selected triggering event.
         /// </summary>
        private void HandleOnProcessTriggered(float triggeringNarrativeObjectSequencerLayerEndTime)
        {
            triggeredLayerDefinition.HandleOnTriggered(triggeringNarrativeObjectSequencerLayerEndTime);
        }
    }
}
