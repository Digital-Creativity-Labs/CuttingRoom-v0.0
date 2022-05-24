using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
    [Serializable]
    public class CuttingRoomEditorGraphViewState : ScriptableObject
    {
        /// <summary>
        /// The states of the narrative object nodes making up the graph.
        /// </summary>
        public List<NarrativeObjectNodeState> narrativeObjectNodeStates = new List<NarrativeObjectNodeState>();

        /// <summary>
        /// The states of the view containers in the graph.
        /// </summary>
        public List<ViewContainerState> viewContainerStates = new List<ViewContainerState>();

        /// <summary>
        /// The guids of the view containers making up the view stack.
        /// </summary>
        public List<string> viewContainerStackGuids = new List<string>();
    }
}