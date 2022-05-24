using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
    [Serializable]
    public class NarrativeObjectNodeState
    {
        /// <summary>
        /// The guid of the narrative object being represented.
        /// </summary>
        public string narrativeObjectGuid = string.Empty;

        /// <summary>
        /// The position of the node.
        /// </summary>
        public Vector2 position = Vector2.zero;
    }
}