using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEditor;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Model;
using System;

public class FFmpegUtilities
{
	private static string snapshotDirectoryName = "snapshots";

	private static string GetFFmpegExecutablePath()
	{
		return Path.Combine(Application.streamingAssetsPath, "ffmpeg-4.2.2-win64-static", "bin");
	}

	private static bool FFmpegExists()
	{
		return Directory.Exists(GetFFmpegExecutablePath());
	}

	private static void SetFFmpegExecutablesPath()
	{
		if (!FFmpegExists())
		{
			Debug.LogError($"FFmpeg is not installed. Please unzip FFmpeg into the project at the following path: {GetFFmpegExecutablePath()}");
		}
		else
		{
			FFmpeg.ExecutablesPath = GetFFmpegExecutablePath();
		}
	}

	public static async Task<float> GetDuration(string videoFilePath)
	{
		SetFFmpegExecutablesPath();

		float duration = 0.0f;

		if (FFmpegExists())
		{
			string command = $@"-v error -select_streams v:0 -show_entries stream=duration -of default=noprint_wrappers=1:nokey=1 ""{videoFilePath}""";

			string durationString = await Probe.New().Start(command);

			bool parseSuccess = float.TryParse(durationString, out duration);

			if (!parseSuccess)
			{
				Debug.LogError($"GetDuration failed for video at path: {videoFilePath}");
			}
		}

		return duration;
	}

	public static async Task<Texture2D> GetSnapshot(string videoFilePath, float normalizedTime)
	{
		SetFFmpegExecutablesPath();

		if (FFmpegExists())
		{
			bool snapshotExists = true;

			if (!SnapshotExists(videoFilePath))
			{
				snapshotExists = await GenerateSnapshot(videoFilePath, normalizedTime);
			}

			if (snapshotExists)
			{
				Texture2D loadedSnapshot = LoadSnapshotAsTexture(videoFilePath);

				return loadedSnapshot;
			}
			else
			{
				Debug.LogError($"Snapshot could not be loaded for file: {videoFilePath}");
			}
		}

		return null;
	}

	private static string GetSnapshotDirectory()
	{
		return Path.Combine(Application.persistentDataPath, snapshotDirectoryName);
	}

	private static string GetSnapshotFilePath(string videoFileName)
	{
		return Path.Combine(GetSnapshotDirectory(), $"{GetVideoName(videoFileName)}.png");
	}

	private static string GetVideoName(string videoFilePath)
	{
		return Path.GetFileNameWithoutExtension(videoFilePath);
	}

	private static async Task<bool> GenerateSnapshot(string videoFilePath, float normalizedSnapshotTime)
	{
		SetFFmpegExecutablesPath();

		// Get the duration of the video.
		float duration = await GetDuration(videoFilePath);

		// Get timespan representing the specified normalised time we are taking the snapshot at.
		TimeSpan halfDuration = TimeSpan.FromSeconds(duration * Mathf.Clamp01(normalizedSnapshotTime));

		// Get the filename of the video having a snapshot created.
		string videoFileName = GetVideoName(videoFilePath);

		// Get the path that the snapshot is being written into.
		string snapshotDirectoryPath = GetSnapshotDirectory();

		// Ensure that the directory exists, if not create it.
		if (!Directory.Exists(snapshotDirectoryPath))
		{
			Directory.CreateDirectory(snapshotDirectoryPath);
		}

		// Create the file path to write the file to.
		string snapshotFilePath = GetSnapshotFilePath(videoFileName);

		// Attempt to get a snapshot.
		IConversionResult result = await Conversion.Snapshot(videoFilePath, snapshotFilePath, halfDuration).Start();

		return result.Success;
	}

	private static bool SnapshotExists(string videoFilePath)
	{
		return File.Exists(GetSnapshotFilePath(GetVideoName(videoFilePath)));
	}

	private static Texture2D LoadSnapshotAsTexture(string videoFilePath)
	{
		byte[] snapshotData = File.ReadAllBytes(GetSnapshotFilePath(videoFilePath));

		Texture2D loadedTexture2D = new Texture2D(1, 1);

		loadedTexture2D.LoadImage(snapshotData);

		return loadedTexture2D;
	}
}
