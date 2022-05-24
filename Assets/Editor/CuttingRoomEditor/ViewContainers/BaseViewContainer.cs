using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class BaseViewContainer : IViewContainer
    {
        public string narrativeObjectGuid = string.Empty;

        public List<string> narrativeObjectNodeGuids = new List<string>();

        public BaseViewContainer(string narrativeObjectGuid)
        {
            this.narrativeObjectGuid = narrativeObjectGuid;
        }

        /// <summary>
        /// Add a node to this view container.
        /// </summary>
        /// <param name="guid"></param>
        public void AddNode(string guid)
        {
            if (!narrativeObjectNodeGuids.Contains(guid))
            {
                narrativeObjectNodeGuids.Add(guid);
            }
        }

        /// <summary>
        /// Remove a node from this container.
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveNode(string guid)
        {
            if (narrativeObjectNodeGuids.Contains(guid))
            {
                narrativeObjectNodeGuids.Remove(guid);
            }
        }

        public bool ContainsNode(string guid)
        {
            return narrativeObjectNodeGuids.Contains(guid);
        }
    }
}
