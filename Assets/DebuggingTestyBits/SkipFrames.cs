using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SkipFrames : MonoBehaviour
{
	public List<VideoPlayer> preparedPlayers = new List<VideoPlayer>();

	public Dictionary<VideoClip, VideoPlayer> players = new Dictionary<VideoClip, VideoPlayer>();

	public VideoClip[] clips = null;
	int index = 0;

	bool allPrepared = false;

	void Start()
	{
		for (int clip = 0; clip < clips.Length; clip++)
		{
			GameObject gameObj = new GameObject($"{clip.ToString()}", typeof(VideoPlayer));

			VideoPlayer videoPlayer = gameObj.GetComponent<VideoPlayer>();

			videoPlayer.playOnAwake = false;

			videoPlayer.waitForFirstFrame = false;

			videoPlayer.clip = clips[clip];

			videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
			videoPlayer.targetCamera = Camera.main;

			videoPlayer.Prepare();

			videoPlayer.prepareCompleted += OnVideoPlayerPrepared;

			players.Add(clips[clip], videoPlayer);
		}
	}

	void OnVideoPlayerPrepared(VideoPlayer source)
	{
		if (!preparedPlayers.Contains(source))
		{
			preparedPlayers.Add(source);

			source.Play();

			source.Pause();
		}

		if (preparedPlayers.Count == clips.Length)
		{
			allPrepared = true;
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (allPrepared)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				// Disable last.
				//players[clips[index]].enabled = false;
				players[clips[index]].Stop();

				index = index < clips.Length - 1 ? index + 1 : 0;
				Debug.Log("index: " + index);

				//players[clips[index]].enabled = true;
				players[clips[index]].Play();
				//players[clips[index]].frame = (long)Random.Range(0, clips[index].frameCount);
			}
		}
    }
}
