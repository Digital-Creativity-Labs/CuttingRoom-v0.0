using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
    /// <summary>
    /// A basic controller for Unitys built in video playback.
    /// </summary>
    public class VideoController : MediaController
    {
        /// <summary>
        /// Where this video is loaded from.
        /// </summary>
        public enum SourceLocation
        {
            VideoClip,
            Resources,
            StreamingAssets
        }

        /// <summary>
        /// Where the video clip source is loaded from for this controller.
        /// </summary>
        [SerializeField]
        private SourceLocation sourceLocation = SourceLocation.VideoClip;

        private VideoPlayer videoPlayer = null;

        private Camera videoPlayerCamera = null;

        private long startFrame = 0;

        private void SetStartFrame()
        {
            videoPlayer.frame = startFrame;
        }

        public void SetMute(bool muted)
        {
            for (ushort audioTrack = 0; audioTrack < videoPlayer.audioTrackCount; audioTrack++)
            {
                videoPlayer.SetDirectAudioMute(audioTrack, muted);
            }
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

            videoPlayer = gameObject.AddComponent<VideoPlayer>();

            // Add a camera for rendering.
            videoPlayerCamera = gameObject.AddComponent<Camera>();
            videoPlayerCamera.clearFlags = CameraClearFlags.SolidColor;
            videoPlayerCamera.backgroundColor = Color.black;

            // Hide the camera.
            videoPlayerCamera.enabled = false;

            switch (sourceLocation)
            {
                case SourceLocation.VideoClip:

                    // Ensure there is some media assigned.
                    if (mediaSource.mediaSourceData.objects.Count == 0)
                    {
                        throw new InvalidMediaException("Video media source does not define a video file for playback.");
                    }

                    // Try to get the video clip.
                    try
                    {
                        videoPlayer.clip = mediaSource.mediaSourceData.objects[0] as VideoClip;
                    }
                    catch (Exception)
                    {
                        throw new InvalidMediaException("Video media source is not a video clip. Invalid format.");
                    }

                    break;

                case SourceLocation.Resources:

                    if (mediaSource.mediaSourceData.strings.Count == 0)
                    {
                        throw new InvalidMediaException("Video media source does not define the name of the video resource file to load.");
                    }

                    try
                    {
                        videoPlayer.clip = Resources.Load(mediaSource.mediaSourceData.strings[0]) as VideoClip;
                    }
                    catch (Exception)
                    {
                        throw new InvalidMediaException("Video resource failed to load.");
                    }

                    break;

                case SourceLocation.StreamingAssets:

                    if (mediaSource.mediaSourceData.strings.Count == 0)
                    {
                        throw new InvalidMediaException("Video media source does not define the name of the video resource file to load.");
                    }

                    try
                    {
                        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, mediaSource.mediaSourceData.strings[0]);
                    }
                    catch (Exception)
                    {
                        throw new InvalidMediaException("Video resource failed to load.");
                    }

                    break;

                default:

                    throw new InvalidMediaException("No source location has been specified for the VideoController. Media cannot be loaded.");
            }

            videoPlayer.playOnAwake = false;

            videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;

            // We are rendering to the near plane (for now...)
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

            // Set the render depth of the camera. This comes from the layer that this video controller is sequenced onto.
            //videoPlayer.targetCamera.depth = renderDepth - 1;
            videoPlayer.targetCamera = videoPlayerCamera;

            // Multiply the frame rate of the video by the time we need to jump into (inTime is seconds from start).
            startFrame = (long)(atomicNarrativeObject.inTime * videoPlayer.frameRate);

            videoPlayer.Pause();

            // Preload the video player content.
            videoPlayer.Prepare();
        }


        public override void WillPlay()
        {
            // Clip will play next frame (most likely), so jump back to the correct start frame.
            SetStartFrame();
        }

        public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
        {
            videoPlayerCamera.enabled = true;

            videoPlayer.Play();
        }

        public override void Pause()
        {
            videoPlayer.Pause();
        }

        public override void Unpause()
        {
            videoPlayer.Play();
        }

        public override void Stop(NarrativeSpace narrativeSpace)
        {
            videoPlayer.Stop();
        }

        public override void Shutdown(Action onShutdownComplete)
        {
            StartCoroutine(ShutdownDelay(onShutdownComplete));
        }

        private IEnumerator ShutdownDelay(Action onShutdownComplete)
        {
            // Delay for 3 frames as this seems to prevent empty frames appearing.
            // Possibly triple buffered so three frames before clearing video buffer means no empty frames?
            // Triple buffered on capable platforms, but some are double!
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Debug.Log("Destroying video player: " + gameObject.name);

            Destroy(videoPlayer);

            base.Shutdown(onShutdownComplete);
        }
    }
}
