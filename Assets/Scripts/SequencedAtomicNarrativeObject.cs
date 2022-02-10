using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public class SequencedAtomicNarrativeObject
	{
		private static int spawnedCount = 0;

		// TODO: These are properties. Should have a capital letter at the start to follow code naming conventions.

		public AtomicNarrativeObject atomicNarrativeObject { get; private set; } = null;

		public float playheadStartTime { get; private set; } = 0.0f;

		public float playheadFinishTime { get; private set; } = 0.0f;

		/// <summary>
		/// Instantiated when the referenced ANO is played.
		/// </summary>
		private List<MediaController> mediaControllers = new List<MediaController>();

		/// <summary>
		/// Used to restore this object.
		/// </summary>
		/// <param name="atomicNarrativeObject"></param>
		public SequencedAtomicNarrativeObject() { }

		public SequencedAtomicNarrativeObject(AtomicNarrativeObject atomicNarrativeObject, float playheadStartTime)
		{
			this.atomicNarrativeObject = atomicNarrativeObject;
			this.playheadStartTime = playheadStartTime;
			playheadFinishTime = playheadStartTime + atomicNarrativeObject.duration;
		}

		public string GetGuid()
		{
			return atomicNarrativeObject.GetGuid();
		}

		public class StateSkeleton
		{
			public string guid = string.Empty;

			public float playheadStartTime = 0.0f;

			public float playheadFinishTime = 0.0f;
		}

		public string Save()
		{
			StateSkeleton stateSkeleton = new StateSkeleton { guid = atomicNarrativeObject.GetGuid(), playheadStartTime = this.playheadStartTime, playheadFinishTime = this.playheadFinishTime };

			return XmlSerialization.SerializeToXmlString(stateSkeleton);
		}

		public void Load(string state)
		{
			StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

			AtomicNarrativeObject[] atomicNarrativeObjects = GameObject.FindObjectsOfType<AtomicNarrativeObject>();

			// Find the correct atomic narrative object by its guid.
			for (int i = 0; i < atomicNarrativeObjects.Length; i++)
			{
				if (atomicNarrativeObjects[i].GetGuid() == stateSkeleton.guid)
				{
					atomicNarrativeObject = atomicNarrativeObjects[i];

					break;
				}
			}

			playheadStartTime = stateSkeleton.playheadStartTime;

			playheadFinishTime = stateSkeleton.playheadFinishTime;
		}

		public void SetPlayheadFinishTime(float playheadFinishTime)
		{
			this.playheadFinishTime = playheadFinishTime;
		}

		public void OnPreload(NarrativeSpace narrativeSpace)
		{
			MediaController mediaController = Object.Instantiate(atomicNarrativeObject.mediaSource.mediaControllerPrefab, atomicNarrativeObject.mediaParent).GetComponent<MediaController>();

			if (atomicNarrativeObject.appearsOnTop)
			{
				mediaController.transform.SetAsLastSibling();
			}
			else
			{
				// Set the newly created object as first sibling. This ensures layering is maintained.
				mediaController.transform.SetAsFirstSibling();
			}

			mediaController.Init(atomicNarrativeObject.mediaSource);

			spawnedCount++;

			mediaController.gameObject.name = spawnedCount.ToString();

			mediaController.Preload(narrativeSpace, atomicNarrativeObject);

			mediaControllers.Add(mediaController);
		}

		public void OnPlaybackWillStart()
		{
			for (int mediaControllerCount = 0; mediaControllerCount < mediaControllers.Count; mediaControllerCount++)
			{
				mediaControllers[mediaControllerCount].WillPlay();
			}
		}

		public void OnPlaybackStart(NarrativeSpace narrativeSpace)
		{
			for (int mediaControllerCount = 0; mediaControllerCount < mediaControllers.Count; mediaControllerCount++)
			{
				mediaControllers[mediaControllerCount].Play(narrativeSpace, atomicNarrativeObject);
			}

			atomicNarrativeObject.InvokeOnPlaybackStart();
		}

		public void OnPlaybackFinish(NarrativeSpace narrativeSpace)
		{
			for (int mediaControllerCount = 0; mediaControllerCount < mediaControllers.Count; mediaControllerCount++)
			{
				MediaController mediaController = mediaControllers[mediaControllerCount];

				mediaController.Stop(narrativeSpace);

				mediaController.Shutdown(() =>
				{
					// Once media has shutdown, the gameobject can be deleted.
					Object.Destroy(mediaController.gameObject);
				});
			}

			atomicNarrativeObject.InvokeOnPlaybackFinish();
		}

		public string GetMediaName()
		{
			string mediaNames = string.Empty;

			for (int mediaControllerCount = 0; mediaControllerCount < mediaControllers.Count; mediaControllerCount++)
			{
				mediaNames += mediaControllers[mediaControllerCount].GetMediaName() + "\n";
			}

			return mediaNames;
		}
	}
}