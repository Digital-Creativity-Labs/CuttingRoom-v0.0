using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class ViewContainer : IViewContainer
    {
        public string narrativeObjectGuid = string.Empty;

        public List<string> narrativeObjectNodeGuids = new List<string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="narrativeObjectGuid"></param>
        public ViewContainer(string narrativeObjectGuid)
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
            if (ContainsNode(guid))
            {
                narrativeObjectNodeGuids.Remove(guid);
            }
        }

        /// <summary>
        /// Whether this view container contains a node with the specified guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool ContainsNode(string guid)
        {
            return narrativeObjectNodeGuids.Contains(guid);
        }
    }
}
