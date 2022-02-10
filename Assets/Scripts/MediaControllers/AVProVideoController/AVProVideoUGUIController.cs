#if AVProVideo

using System;
using UnityEngine;
using CuttingRoom.Exceptions;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
#endif

namespace CuttingRoom
{
	/// <summary>
	/// A controller for AVProVideo on the Unity GUI.
	/// </summary>
	public class AVProVideoUGUIController : MediaController
	{
		[SerializeField]
		private AVProVideoUGUIElement avProVideoUGUIElement = null;

		/// <summary>
		/// The file path for the media being played.
		/// </summary>
		private string mediaFilePath = string.Empty;

		private void SetRectTransformAnchors(float left, float right)
		{
			RectTransform rectTransform = GetComponent<RectTransform>();

			// Set left position off screen.
			rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);

			// Set right position off the left of the screen.
			rectTransform.offsetMax = new Vector2(right, rectTransform.offsetMax.y);
		}

		/// <summary>
		/// Perform initial setup for video.
		/// </summary>
		/// <param name="media"></param>
		/// <param name="inTime">The start point of the media in seconds from the start of the clip.</param>
		/// <param name="duration"></param>
		public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Preload(narrativeSpace, atomicNarrativeObject);

			// Ensure there is some media assigned.
			if (mediaSource.mediaSourceData.strings.Count == 0)
			{
				throw new InvalidMediaException("AVProVideoUGUI media source does not define the media to be played.");
			}

			mediaFilePath = mediaSource.mediaSourceData.strings[0];

			// Setup the media player.
			avProVideoUGUIElement.Preload(mediaSource.mediaSourceData.strings[0], atomicNarrativeObject.inTime);

			// Hide the rect transform off the screen while preloading.
			SetRectTransformAnchors(-100000.0f, -100000.0f);
		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			Debug.Log($"Starting playback for {gameObject.name}");

			avProVideoUGUIElement.Play(() =>
			{
				SetRectTransformAnchors(0.0f, 0.0f);
			});
		}

		public override void Pause()
		{
			avProVideoUGUIElement.Pause();
		}

		public override void Unpause()
		{
			avProVideoUGUIElement.Pause();
		}

		public override void Stop(NarrativeSpace narrativeSpace)
		{
			avProVideoUGUIElement.Stop();
		}

		public override void Shutdown(Action onShutdownComplete)
		{
			Debug.Log("Destroying video player: " + gameObject.name);

			avProVideoUGUIElement.Shutdown(() =>
			{
				base.Shutdown(onShutdownComplete);
			});
		}

		public override string GetMediaName()
		{
			return mediaFilePath;
		}

#if UNITY_EDITOR

		public string GetVideoFilePath(MediaSource mediaSource)
		{
			if (mediaSource != null && mediaSource.mediaSourceData.strings.Count > 0)
			{
				return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets", "StreamingAssets", mediaSource.mediaSourceData.strings[0]);
			}

			return string.Empty;
		}

		public override async Task<float> GetDuration(MediaSource mediaSource)
		{
			string videoFilePath = GetVideoFilePath(mediaSource);

			if (!string.IsNullOrEmpty(videoFilePath))
			{
				return await FFmpegUtilities.GetDuration(videoFilePath);
			}

			return 0.0f;
		}

		public override async Task<Texture2D> GetThumbnail(MediaSource mediaSource)
		{
			string videoFilePath = GetVideoFilePath(mediaSource);

			if (!string.IsNullOrEmpty(videoFilePath))
			{
				return await FFmpegUtilities.GetSnapshot(videoFilePath, 0.5f);
			}

			return new Texture2D(0, 0);
		}

		public override void RenderCreateFields(Rect rect, MediaSourceData mediaSourceData)
		{
			// Ensure that the mediaSourceData contains the required variables.
			if (mediaSourceData.strings.Count == 0)
			{
				mediaSourceData.strings.Add(string.Empty);
			}

			EditorGUILayout.LabelField("Video File:");

			UnityEngine.Object previouslySelectedObject = null;

			if (!string.IsNullOrEmpty(mediaSourceData.strings[0]))
			{
				previouslySelectedObject = AssetDatabase.LoadAssetAtPath(mediaSourceData.strings[0], typeof(UnityEngine.Object)) as UnityEngine.Object;
			}

			UnityEngine.Object selectedObject = EditorGUILayout.ObjectField(previouslySelectedObject, typeof(UnityEngine.Object), false);

			string selectedAssetPath = AssetDatabase.GetAssetPath(selectedObject);

			if (selectedAssetPath != mediaSourceData.strings[0])
			{
				PopulateMediaSourceData(mediaSourceData, null, new List<string>() { selectedAssetPath });
			}
		}

		// Dropzone for multiple field rendering.
		private EditorLayoutUtilities.DropZone videoDropZone = new EditorLayoutUtilities.DropZone();

		public override void RenderCreateMultipleFields(Rect rect, Func<MediaSourceData> GetMediaSourceData)
		{
			UnityEngine.Object[] droppedObjects = videoDropZone.Render("Drop videos here", (int)rect.width - 8, 100);

			if (droppedObjects != null)
			{
				for (int droppedObjectCount = 0; droppedObjectCount < droppedObjects.Length; droppedObjectCount++)
				{
					string selectedAssetPath = AssetDatabase.GetAssetPath(droppedObjects[droppedObjectCount]);

					MediaSourceData mediaSourceData = GetMediaSourceData.Invoke();

					PopulateMediaSourceData(mediaSourceData, null, new List<string> { selectedAssetPath });
				}
			}

			EditorGUILayout.Space();
		}
#endif
	}
}

#endif