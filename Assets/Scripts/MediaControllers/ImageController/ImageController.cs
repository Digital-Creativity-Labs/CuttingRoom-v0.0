using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
	/// <summary>
	/// A controller for sprite images on the Unity GUI.
	/// </summary>
	public class ImageController : MediaController
	{
		[SerializeField]
		private Image image = null;

		public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Preload(narrativeSpace, atomicNarrativeObject);

			if (mediaSource.mediaSourceData.objects.Count == 0)
			{
				throw new InvalidMediaException("ImageController MediaSource has no media attached.");
			}

			Sprite sprite = mediaSource.mediaSourceData.objects[0] as Sprite;

			// ?? checks whether sprite is null or not, if it is not then the assignment before it occurs, otherwise the error is thrown.
			image.sprite = sprite ?? throw new InvalidMediaException("ImageControllers MediaSource has not specified a sprite as the first entry in the media list.");

			image.gameObject.SetActive(false);

		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			image.gameObject.SetActive(true);
		}
	}
}