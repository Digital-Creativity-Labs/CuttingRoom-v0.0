using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;

#if UNITY_EDITOR
using UnityEditor;
using System.Threading.Tasks;
#endif

namespace CuttingRoom
{
	/// <summary>
	/// The base class for any media controller used within a narrative space.
	/// </summary>
	public class MediaController : MonoBehaviour
	{
		/// <summary>
		/// The media source for this controller.
		/// </summary>
		protected MediaSource mediaSource = null;

		/// <summary>
		/// Set vital variables for media controllers.
		/// </summary>
		public virtual void Init(MediaSource mediaSource) { this.mediaSource = mediaSource; }

		/// <summary>
		/// Called to preload media before playback begins.
		/// </summary>
		/// <param name="narrativeSpace"></param>
		/// <param name="atomicNarrativeObject"></param>
		public virtual void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			if (atomicNarrativeObject.mediaSource == null)
			{
				throw new InvalidMediaException("MediaController does not specify a MediaSource.");
			}
		}

		/// <summary>
		/// Called when playback will occur next frame (predicted based on average delta time so may be two frames later).
		/// </summary>
		public virtual void WillPlay() { }

		/// <summary>
		/// Play assigned media from the start.
		/// </summary>
		/// <param name="media"></param>
		/// <param name="inTime"></param>
		/// <param name="duration"></param>
		public virtual void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject) { }

		/// <summary>
		/// Pause media playback.
		/// </summary>
		public virtual void Pause() { }

		/// <summary>
		/// Unpause media playback.
		/// </summary>
		public virtual void Unpause() { }

		/// <summary>
		/// Stop media playback.
		/// </summary>
		public virtual void Stop(NarrativeSpace narrativeSpace) { }

		/// <summary>
		/// Close media playback. This should destroy any instantiated assets.
		/// </summary>
		public virtual void Shutdown(Action onShutdownComplete) { onShutdownComplete?.Invoke(); }

		/// <summary>
		/// Get the name of the media related to this 
		/// </summary>
		/// <returns></returns>
		public virtual string GetMediaName() { return string.Empty; }

#if UNITY_EDITOR

		/// <summary>
		/// Used to set default duration in editor.
		/// </summary>
		/// <returns></returns>
		public virtual Task<float> GetDuration(MediaSource mediaSource) { return null; }

		/// <summary>
		/// Used to show a thumbnail of this media in the narrative space editor.
		/// </summary>
		/// <param name="mediaSource"></param>
		/// <returns></returns>
		public virtual Task<Texture2D> GetThumbnail(MediaSource mediaSource) { return null; }

		public virtual void RenderCreateFields(Rect rect, MediaSourceData mediaSourceData) { EditorGUILayout.LabelField("Generate Media Sources Editor Unsupported."); }

		public virtual void RenderCreateMultipleFields(Rect rect, Func<MediaSourceData> GetMediaSourceData) { }

		protected void PopulateMediaSourceData(MediaSourceData mediaSourceData, List<UnityEngine.Object> objects, List<string> strings)
		{
			mediaSourceData.objects = objects;

			mediaSourceData.strings = strings;
		}
#endif
	}
}