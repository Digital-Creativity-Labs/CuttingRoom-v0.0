using System;
using System.Collections.Generic;

namespace CuttingRoom
{
	[Serializable]
	public class MediaSourceData
	{
		/// <summary>
		/// The media files.
		/// </summary>
		public List<UnityEngine.Object> objects = new List<UnityEngine.Object>();

		/// <summary>
		/// String data associated with this media source.
		/// </summary>
		public List<string> strings = new List<string>();
	}
}