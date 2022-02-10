using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using CuttingRoom;

namespace CuttingRoom.Editor
{
	public static class CuttingRoomUtilities
	{
		public static async void SetAllSceneAtomsDurationToMediaLength()
		{
			// Get the atoms in the current scene/scenes?
			AtomicNarrativeObject[] atomicNarrativeObjects = UnityEngine.Object.FindObjectsOfType<AtomicNarrativeObject>();

			// Generate a list of Atoms against their media controllers.
			// The media controller must be fetched beforehand as it cannot be done from a thread/parallel operation.
			List<Tuple<AtomicNarrativeObject, MediaController>> getMediaSourceDurationData = new List<Tuple<AtomicNarrativeObject, MediaController>>();

			void SetAtomicNarrativeObjectDuration(AtomicNarrativeObject atomicNarrativeObject, float duration)
			{
				// Flag object as dirty as duration has changed.
				atomicNarrativeObject.duration = duration;

				EditorUtility.SetDirty(atomicNarrativeObject);
			}

			for (int atomicNarrativeObjectCount = 0; atomicNarrativeObjectCount < atomicNarrativeObjects.Length; atomicNarrativeObjectCount++)
			{
				AtomicNarrativeObject atomicNarrativeObject = atomicNarrativeObjects[atomicNarrativeObjectCount];

				if (atomicNarrativeObject.mediaSource != null)
				{
					// Get the media controller.
					MediaController mediaController = atomicNarrativeObject.mediaSource.mediaControllerPrefab.GetComponent<MediaController>();

					getMediaSourceDurationData.Add(new Tuple<AtomicNarrativeObject, MediaController>(atomicNarrativeObject, mediaController));
				}
				else
				{
					// Set duration to zero.
					SetAtomicNarrativeObjectDuration(atomicNarrativeObject, 0.0f);
				}
			}

			List<Task<Tuple<AtomicNarrativeObject, float>>> generatedTasks = new List<Task<Tuple<AtomicNarrativeObject, float>>>();

			Parallel.ForEach(getMediaSourceDurationData,
				(data) =>
				{
					Task<Tuple<AtomicNarrativeObject, float>> task = GetMediaSourceDuration(data.Item1, data.Item2);

					generatedTasks.Add(task);
				});

			await Task.WhenAll(generatedTasks.ToArray());

			for (int taskCount = 0; taskCount < generatedTasks.Count; taskCount++)
			{
				// Get the task.
				Task<Tuple<AtomicNarrativeObject, float>> task = generatedTasks[taskCount];

				// If task completed successfully.
				if (task.IsCompleted)
				{
					// Get the result.
					Tuple<AtomicNarrativeObject, float> result = task.Result;

					// Set duration to the returned value.
					SetAtomicNarrativeObjectDuration(result.Item1, result.Item2);
				}
			}
		}

		public static async Task<Tuple<AtomicNarrativeObject, float>> GetMediaSourceDuration(AtomicNarrativeObject atomicNarrativeObject, MediaController mediaController)
		{
			float duration = await mediaController.GetDuration(atomicNarrativeObject.mediaSource);

			return new Tuple<AtomicNarrativeObject, float>(atomicNarrativeObject, duration);
		}

		/*public static async Task<Tuple<AtomicNarrativeObject, float>> SetAtomDurationToMediaSourceLength(AtomicNarrativeObject atomicNarrativeObject)
		{
			if (atomicNarrativeObject != null)
			{
				if (atomicNarrativeObject.mediaSource != null)
				{
					MediaController mediaController = atomicNarrativeObject.mediaSource.mediaControllerPrefab.GetComponent<MediaController>();

					float duration = await mediaController.GetDuration(atomicNarrativeObject.mediaSource);

					atomicNarrativeObject.duration = duration;

					EditorUtility.SetDirty(atomicNarrativeObject);
				}
				else
				{
					atomicNarrativeObject.duration = 0.0f;

					EditorUtility.SetDirty(atomicNarrativeObject);
				}
			}
		}

		private static async Task<float> GetMediaSourceDuration(MediaController mediaController, AtomicNarrativeObject atomicNarrativeObject)
		{
			float duration = await mediaController.GetDuration(atomicNarrativeObject.mediaSource);

			return duration;
		}*/
	}
}
