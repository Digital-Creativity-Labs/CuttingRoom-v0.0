using System;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CuttingRoom
{
	/// <summary>
	/// A basic controller for audio playback within a production.
	/// </summary>
	public class AudioController : MediaController
	{
		[SerializeField]
		private AudioSource audioSource = null;

		private AudioClip GetAudioClip(MediaSource mediaSource)
		{
			if (mediaSource.mediaSourceData.objects.Count > 0)
			{
				return mediaSource.mediaSourceData.objects[0] as AudioClip;
			}

			return null;
		}

		public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Preload(narrativeSpace, atomicNarrativeObject);

			if (mediaSource.mediaSourceData.objects.Count == 0)
			{
				throw new InvalidMediaException("AudioControllers MediaSource has no media attached.");
			}

			AudioClip audioClip = GetAudioClip(mediaSource);

			// ?? checks whether audioClip is null or not, if it is not then the assignment before it occurs, otherwise the error is thrown.
			audioSource.clip = audioClip ?? throw new InvalidMediaException("AudioControllers MediaSource has not specified an AudioClip as the first entry in the media list.");

			// Set the in time for the clip.
			audioSource.time = atomicNarrativeObject.inTime;
		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			audioSource.Play();
		}

		public override string GetMediaName()
		{
			AudioClip audioClip = GetAudioClip(mediaSource);

			return audioClip != null ? audioClip.name : "No Media Assigned";
		}

#if UNITY_EDITOR

		public override async Task<float> GetDuration(MediaSource mediaSource)
		{
			AudioClip audioClip = GetAudioClip(mediaSource);

			return audioClip != null ? audioClip.length : 0.0f;
		}

		public override void RenderCreateFields(Rect rect, MediaSourceData mediaSourceData)
		{
			// Add an object.
			if (mediaSourceData.objects.Count == 0)
			{
				mediaSourceData.objects.Add(null);
			}

			AudioClip audioClip = null;

			if (mediaSourceData.objects[0] != null)
			{
				audioClip = mediaSourceData.objects[0] as AudioClip;
			}

			AudioClip selectedAudioClip = EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), false) as AudioClip;

			if (selectedAudioClip != audioClip)
			{
				mediaSourceData.objects[0] = selectedAudioClip;
			}
		}

		// Dropzone for multiple field rendering.
		private EditorLayoutUtilities.DropZone audioClipDropZone = new EditorLayoutUtilities.DropZone();

		public override void RenderCreateMultipleFields(Rect rect, Func<MediaSourceData> GetMediaSourceData)
		{
			UnityEngine.Object[] droppedObjects = audioClipDropZone.Render("Drop AudioClips here", (int)rect.width - 8, 100);

			if (droppedObjects != null)
			{
				for (int droppedObjectCount = 0; droppedObjectCount < droppedObjects.Length; droppedObjectCount++)
				{
					if (droppedObjects[droppedObjectCount] is AudioClip)
					{
						MediaSourceData mediaSourceData = GetMediaSourceData.Invoke();

						PopulateMediaSourceData(mediaSourceData, new List<UnityEngine.Object> { droppedObjects[droppedObjectCount] }, null);
					}
					else
					{
						Debug.LogError("Invalid object. Objects must be of type AudioClip.");
					}
				}
			}

			EditorGUILayout.Space();
		}

#endif
	}
}