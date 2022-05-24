using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class EdgeState
    {
        /// <summary>
        /// The GraphView edge object.
        /// </summary>
        public Edge Edge { get; set; } = null;

        /// <summary>
        /// The node which has this edge connected from its output port.
        /// </summary>
        public NarrativeObjectNode OutputNarrativeObjectNode { get; set; } = null;

        /// <summary>
        /// The node which has this edge connected to its input port.
        /// </summary>
        public NarrativeObjectNode InputNarrativeObjectNode { get; set; } = null;
    }
}