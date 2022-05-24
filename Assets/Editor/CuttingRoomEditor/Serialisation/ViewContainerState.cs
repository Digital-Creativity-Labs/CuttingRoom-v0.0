using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
	[Serializable]
	public class ViewContainerState
	{
		/// <summary>
		/// The guid of the narrative object which this container represents.
		/// </summary>
		public string narrativeObjectGuid = string.Empty;

		/// <summary>
		/// The guid of the nodes inside this view container.
		/// </summary>
		public List<string> narrativeObjectNodeGuids = new List<string>();
	}
}