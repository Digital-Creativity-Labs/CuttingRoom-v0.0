#if AVProVideo

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using RenderHeads.Media.AVProVideo;
using CuttingRoom.Exceptions;

#if UNITY_EDITOR
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
#endif

namespace CuttingRoom
{
	public class AVProVideoController : MediaController
	{
		private DisplayIMGUI displayIMGUI = null;
		private MediaPlayer mediaPlayer
		{
			get
			{
				return displayIMGUI._mediaPlayer;
			}
		}

		private float startTimeMs = 0;

		public void SetMute(bool muted)
		{
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
				throw new InvalidMediaException("AVProVideo media source does not define the media to be played.");
			}

			displayIMGUI = GetComponent<DisplayIMGUI>();

			if (displayIMGUI == null)
			{
				throw new NullReferenceException();
			}

			if (displayIMGUI._mediaPlayer == null)
			{
				throw new NullReferenceException();
			}

			// Set the video clip on the video player.
			mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, mediaSource.mediaSourceData.strings[0]);

			// Multiply the frame rate of the video by the time we need to jump into (inTime is seconds from start).
			startTimeMs = (float)TimeSpan.FromSeconds(atomicNarrativeObject.inTime).TotalMilliseconds;

			displayIMGUI._depth = 0;
		}

		public void OnEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
		{
			if (eventType == MediaPlayerEvent.EventType.FinishedBuffering)
			{
				mediaPlayer.Control.Seek(startTimeMs);
			}
			else if (eventType == MediaPlayerEvent.EventType.FirstFrameReady)
			{
				mediaPlayer.Pause();
			}
		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			displayIMGUI._depth = -1;

			mediaPlayer.Play();
		}

		public override void Pause()
		{
			mediaPlayer.Pause();
		}

		public override void Unpause()
		{
			//mediaPlayer.pause();
		}

		public override void Stop(NarrativeSpace narrativeSpace)
		{
			mediaPlayer.Stop();
		}

		public override void Shutdown(Action onShutdownComplete)
		{
			Debug.Log("Destroying video player: " + gameObject.name);

			Destroy(mediaPlayer);

			base.Shutdown(onShutdownComplete);
		}

#if UNITY_EDITOR
		public override async Task<float> GetDuration(MediaSource mediaSource)
		{
			return await FFmpegUtilities.GetDuration(Path.Combine(Application.streamingAssetsPath, mediaSource.mediaSourceData.strings[0]));
		}
#endif
	}
}

#endif