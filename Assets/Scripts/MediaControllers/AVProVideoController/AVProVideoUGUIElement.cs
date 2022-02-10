#if AVProVideo

using System;
using System.Collections;
using UnityEngine;
using RenderHeads.Media.AVProVideo;

namespace CuttingRoom
{
	/// <summary>
	/// Control logic for an AVProVideo media player.
	/// </summary>
	class AVProVideoUGUIElement : MonoBehaviour
	{
		[SerializeField]
		private DisplayUGUI displayUGUI = null;
		private MediaPlayer mediaPlayer
		{
			get
			{
				return displayUGUI._mediaPlayer;
			}
		}

		private float startTimeMs = 0;

		/// <summary>
		/// Flag to mark this controller as ready for playback.
		/// </summary>
		private bool readyToPlay = false;

		public void Preload(string mediaFilePath, float inTime)
		{
			if (displayUGUI == null)
			{
				throw new NullReferenceException();
			}

			if (displayUGUI._mediaPlayer == null)
			{
				throw new NullReferenceException();
			}

			// If the path is absolute, then chop off the streaming assets path.
			if (mediaFilePath.Contains("StreamingAssets/"))
			{
				string[] splitPath = mediaFilePath.Split(new string[] { "StreamingAssets/" }, StringSplitOptions.RemoveEmptyEntries);

				mediaFilePath = splitPath[splitPath.Length - 1];
			}

			// Set the video clip on the video player.
			mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, mediaFilePath);

			mediaPlayer.m_AutoOpen = false;
			mediaPlayer.m_AutoStart = false;

			// Multiply the frame rate of the video by the time we need to jump into (inTime is seconds from start).
			startTimeMs = (float)TimeSpan.FromSeconds(inTime).TotalMilliseconds;
		}

		public void Play(Action onPlay)
		{
			StartCoroutine(DelayedPlay(onPlay));
		}

		private IEnumerator DelayedPlay(Action onPlay)
		{
			while (!readyToPlay)
			{
				yield return new WaitForEndOfFrame();
			}

			mediaPlayer.Play();

			onPlay?.Invoke();
		}

		public void Pause()
		{
			mediaPlayer.Pause();
		}

		public void Unpause()
		{
			mediaPlayer.Pause();
		}

		public void Stop()
		{
			mediaPlayer.Stop();
		}

		public void Shutdown(Action onShutdownComplete)
		{
			Destroy(mediaPlayer);

			onShutdownComplete?.Invoke();
		}

		public void OnEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
		{
			Debug.Log($"OnEvent: {eventType.ToString()}");

			if (eventType == MediaPlayerEvent.EventType.FirstFrameReady)
			{
				Debug.Log($"Seeking for {gameObject.name}");

				if (startTimeMs > 0)
				{
					mediaPlayer.Control.Seek(startTimeMs);
				}
				else
				{
					mediaPlayer.Pause();


					readyToPlay = true;
				}
			}
			else if (eventType == MediaPlayerEvent.EventType.FinishedSeeking)
			{
				Debug.Log($"Seeking complete for {gameObject.name}");

				mediaPlayer.Pause();

				readyToPlay = true;
			}
		}
	}
}

#endif