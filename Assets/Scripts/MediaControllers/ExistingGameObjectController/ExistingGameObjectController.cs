using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
	/// <summary>
	/// A controller for game objects that already exist in hierarchy of the main project scene.
    /// 
    /// The object can be specified by name using a media asset with the name of the hame object as its first string parameter.
    /// An example use case would be to make a UI authored in the scene appear and disapear when the media element is set for playback.
	/// </summary>
	public class ExistingGameObjectController : MediaController
    {
		[SerializeField]
		private GameObject existingObject;

		public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Preload(narrativeSpace, atomicNarrativeObject);

			if (mediaSource.mediaSourceData.strings.Count == 0)
			{
				throw new InvalidMediaException("ExistingGameObjectController MediaSource has no media attached.");
			}

			string objectName = mediaSource.mediaSourceData.strings[0];
			existingObject = transform.parent.Find(objectName).gameObject;

			if(existingObject == null)
            {
				throw new InvalidMediaException("ExistingGameObjectController MediaSource has not specified a GameObject that is present in the scene.");
			}

			existingObject.SetActive(false);
		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
        {
			existingObject.SetActive(true);
		}

        public override void Stop(NarrativeSpace narrativeSpace)
        {
			existingObject.SetActive(false);
		}
    }
}
