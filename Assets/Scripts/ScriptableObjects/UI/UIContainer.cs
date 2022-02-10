using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	[CreateAssetMenu(fileName = "UIContainer.asset", menuName = "Cutting Room/UI/Container")]
	public class UIContainer : ScriptableObject
	{
		/// <summary>
		/// Prefab for this container.
		/// </summary>
		public GameObject prefab = null;
	}
}
