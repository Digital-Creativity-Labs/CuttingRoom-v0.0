using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// A container for definition of a piece of source media to be used within a production.
	/// </summary>
	[CreateAssetMenu(fileName = "MediaSource.asset", menuName = "Cutting Room/Media Source")]
	public class MediaSource : ScriptableObject
	{
		/// <summary>
		/// Controller for media. This component controls how the media is actually played back.
		/// </summary>
		public GameObject mediaControllerPrefab = null;

		/// <summary>
		/// The data for this media source.
		/// </summary>
		public MediaSourceData mediaSourceData = null;
	}
}
