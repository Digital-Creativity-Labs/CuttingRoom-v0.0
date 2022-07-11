using UnityEngine;

namespace CuttingRoom.Editor
{
    public static class CuttingRoomEditorUtils
    {
        /// <summary>
        /// Get or create narrative space.
        /// </summary>
        /// <returns></returns>
        public static NarrativeSpace GetOrCreateNarrativeSpace()
        {
            NarrativeSpace narrativeSpace = Object.FindObjectOfType<NarrativeSpace>();

            if (narrativeSpace == null)
            {
                narrativeSpace = CuttingRoomContextMenus.CreateNarrativeSpace();
            }

            // Ensure sequencer exists.
            Sequencer sequencer = Object.FindObjectOfType<Sequencer>();

            if (sequencer == null)
            {
                sequencer = CuttingRoomContextMenus.CreateSequencer();

                // Set narrative space on the sequencer.
                sequencer.narrativeSpace = narrativeSpace;
            }

            return narrativeSpace;
        }
    }
}
