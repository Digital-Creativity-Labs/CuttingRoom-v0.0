using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CuttingRoom.Exceptions;
using CuttingRoom.VariableSystem;

namespace CuttingRoom
{
	[RequireComponent(typeof(GameObjectGuid))]
	public class Sequencer : MonoBehaviour, ISaveable
	{
		#region Debugging

		/// <summary>
		/// The path of the log file saved for the sequencer when debugging is enabled.
		/// </summary>
		private string debugLogFilePath = string.Empty;

		/// <summary>
		/// Toggle to enable or disable debugging for the sequencer.
		/// </summary>
		[Header("Debugging")]
		[SerializeField]
		private bool enableDebugging = false;

		/// <summary>
		/// Toggle to enable debug overlay.
		/// </summary>
		[SerializeField]
		private bool enableDebuggingOverlay = false;

		/// <summary>
		/// Debug variable for holding a reference to the string on screen.
		/// </summary>
		private string playingAtomicNarrativeObjectsText = string.Empty;

		#endregion

		[Header("References")]
		[SerializeField]
		private GameObjectGuid gameObjectGuid = null;

		/// <summary>
		/// The narrative space which is being processed by the sequencer.
		/// </summary>
		[Header("Settings")]
		public NarrativeSpace narrativeSpace = null;

		/// <summary>
		/// Whether processing should begin once the object receives its initialisation or whether it will be called by the user from elsewhere.
		/// </summary>
		public bool AutoStartProcessing = true;

		/// <summary>
		/// Toggle to hide the cursor at the start of the production.
		/// </summary>
		[SerializeField]
		private bool hideCursor = false;

		[Header("Save/Load")]
		[SerializeField]
		private string productionGuid = string.Empty;

		[SerializeField]
		private bool loadBeforeSequencing = false;

		[SerializeField]
		private bool saveWhenObjectSequenced = false;

		[SerializeField]
		private bool saveWhenVariableSet = false;

		/// <summary>
		/// The current time of the global playhead.
		/// </summary>
		public float PlayheadTime { get; private set; } = 0.0f;

		/// <summary>
		/// Exception thrown when the narrative space is invalid. The reason is included as the message field.
		/// </summary>
		public class InvalidNarrativeSpaceException : Exception { public InvalidNarrativeSpaceException(string message) : base(message) { } }

		/// <summary>
		/// The playback layers for this sequence.
		/// </summary>
		private List<SequencerLayer> sequencerLayers = new List<SequencerLayer>();

		/// <summary>
		/// Delegate used to indicate that the sequencer has completed playback.
		/// </summary>
		public delegate void OnPlaybackEventCallback();

		/// <summary>
		/// Invoked when playback begins.
		/// </summary>
		public event OnPlaybackEventCallback OnPlaybackStart;

		/// <summary>
		/// Invoked when playback completes within the sequencer.
		/// </summary>
		public event OnPlaybackEventCallback OnPlaybackComplete;

		/// <summary>
		/// Is true when the processing stack is complete and no further media will be sequenced.
		/// </summary>
		private bool processingComplete = false;

		/// <summary>
		/// A hashset of all sequenced narrative objects.
		/// </summary>
		private HashSet<NarrativeObject> sequencedNarrativeObjects = new HashSet<NarrativeObject>();

		public class CurrentlyPlayingNarrativeObjectInfo
		{
			public NarrativeObject narrativeObject = null;
			public SequencerLayer sequencerLayer = null;
		}

		/// <summary>
		/// The narrative objects currently being processed.
		/// </summary>
		//public List<CurrentlyPlayingNarrativeObjectInfo> currentlyProcessingNarrativeObjectInfos = new List<CurrentlyPlayingNarrativeObjectInfo>();
		private Dictionary<SequencerLayer, NarrativeObject> currentlyProcessingNarrativeObjects = new Dictionary<SequencerLayer, NarrativeObject>();

		private void Start()
		{
			if (enableDebugging)
			{
				string debugLogFileName = DateTime.Now.ToString("yyyyMMddTHHmmss") + ".log";
				// Log file name is the date and time. ISO 8601 compliant.
				debugLogFilePath = Path.Combine(Application.persistentDataPath, debugLogFileName);

				// Create the file path for the 
				using (StreamWriter streamWriter = File.CreateText(debugLogFilePath))
				{
					// TODO: Add startup info?
				}
			}

			if (enableDebuggingOverlay)
			{
				GameObject debuggingOverlayPrefab = Resources.Load("DebuggingOverlayCanvas") as GameObject;
			}

			if (hideCursor)
			{
				Cursor.visible = false;
			}
			else
			{
				Cursor.visible = true;
			}

			if (narrativeSpace == null)
			{
				throw new InvalidNarrativeSpaceException("Sequencer has no NarrativeSpace defined for processing.");
			}

			if (saveWhenVariableSet)
			{
				VariableStore[] variableStores = FindObjectsOfType<VariableStore>();

				for (int i = 0; i < variableStores.Length; i++)
				{
					// Set up each variable to callback to this method to trigger a save.
					variableStores[i].RegisterOnVariableSetCallback(() =>
					{
						SaveState();
					});
				}
			}

			if (AutoStartProcessing)
			{
				StartProcessing();
			}
		}

		public void StartProcessing()
		{
			if (!narrativeSpace.IsValid())
			{
				throw new InvalidRootNarrativeObjectException("The NarrativeSpace assigned to the Sequencer does not define a root narrative object.");
			}

			// If this should try to load save data before proceeding, then do so.
			if (loadBeforeSequencing)
			{
				LoadState();
			}

			// Callback for playback beginning.
			OnPlaybackStart?.Invoke();

			// TODO: The processing coroutine takes the root narrative object, when in fact, if a load has happened, it will not be this.

			StartCoroutine(ProcessingCoroutine());

			StartCoroutine(SequencingCoroutine());
		}

		private IEnumerator ProcessingCoroutine()
		{
			SequencerLayer baseSequencerLayer = new SequencerLayer(null, 0.0f);

			sequencerLayers.Add(baseSequencerLayer);

			// Processing coroutines are placed here and watched for completion.
			List<Coroutine> processingCoroutines = new List<Coroutine>();

			// If nothing has been restored for playback, then start at the root of the production.
			if (currentlyProcessingNarrativeObjects.Count == 0)
			{
				processingCoroutines.Add(StartCoroutine(narrativeSpace.rootNarrativeObject.Process(this, baseSequencerLayer)));
			}
			// Otherwise restore processing to where it was when saved.
			else
			{
				// Dictionary must be cloned to iterate it as the process call will alter the original dictionary while the foreach is still running. Cannot iterate it if you change the contents during iteration.
				Dictionary<SequencerLayer, NarrativeObject> clonedCurrentlyProcessingNarrativeObjects = new Dictionary<SequencerLayer, NarrativeObject>(currentlyProcessingNarrativeObjects);

				foreach(KeyValuePair<SequencerLayer, NarrativeObject> pair in clonedCurrentlyProcessingNarrativeObjects)
				{
					processingCoroutines.Add(StartCoroutine(pair.Value.Process(this, pair.Key)));
				}
			}

			for (int i = 0; i < processingCoroutines.Count; i++)
			{
				yield return processingCoroutines[i];
			}

			// No more processing to do, the yield has ended so there cannot be any further processing after this point.
			processingComplete = true;
		}

		public void OnStartProcessingNarrativeObject(NarrativeObject narrativeObject, SequencerLayer sequencerLayer)
		{
			if (!currentlyProcessingNarrativeObjects.ContainsKey(sequencerLayer))
			{
				currentlyProcessingNarrativeObjects.Add(sequencerLayer, null);
			}

			currentlyProcessingNarrativeObjects[sequencerLayer] = narrativeObject;
		}

		private IEnumerator SequencingCoroutine()
		{
			while (true)
			{
				PlayheadTime += Time.deltaTime;

				// If processing is complete, then start to look for the end of playback.
				// Before processing is complete it is a waste of time looking as more media might be sequenced.
				if (processingComplete)
				{
					// Assume playback is complete.
					bool playbackComplete = true;

					for (int count = 0; count < sequencerLayers.Count; count++)
					{
						if (sequencerLayers[count].GetLayerEndTime() >= PlayheadTime)
						{
							playbackComplete = false;

							break;
						}
					}

					// If playback is complete, then break out of the while loop.
					if (playbackComplete)
					{
						for (int count = 0; count < sequencerLayers.Count; count++)
						{
							// One final call to update to allow layers to shutdown.
							sequencerLayers[count].Terminate(narrativeSpace);
						}

						break;
					}
				}

				playingAtomicNarrativeObjectsText = string.Empty;

				for (int count = 0; count < sequencerLayers.Count; count++)
				{
					int layerIndex = count;

					sequencerLayers[count].Update(PlayheadTime, narrativeSpace, (SequencedAtomicNarrativeObject sequencedAtomicNarrativeObject) =>
					{
						if (enableDebugging)
						{
							using (StreamWriter streamWriter = File.AppendText(debugLogFilePath))
							{
								streamWriter.WriteLine($"Sequencer Layer: {layerIndex}\nMedia: {sequencedAtomicNarrativeObject.GetMediaName()}\nStart Time: {sequencedAtomicNarrativeObject.playheadStartTime}\nEnd Time: {sequencedAtomicNarrativeObject.playheadFinishTime}\n");
							}
						}
					});
				}

				yield return new WaitForEndOfFrame();
			}

			OnPlaybackComplete?.Invoke();
		}

		public List<SequencedAtomicNarrativeObject> GetPlayingAtomicNarrativeObjects()
		{
			List<SequencedAtomicNarrativeObject> playingAtomicNarrativeObjects = new List<SequencedAtomicNarrativeObject>();

			for (int i = 0; i < sequencerLayers.Count; i++)
			{
				if (sequencerLayers[i].playingAtomicNarrativeObject != null)
				{
					playingAtomicNarrativeObjects.Add(sequencerLayers[i].playingAtomicNarrativeObject);
				}
			}

			return playingAtomicNarrativeObjects;
		}


		public string GetDebugPlaybackInfo()
		{
			string debugPlaybackInfo = string.Empty;

			for (int count = 0; count < sequencerLayers.Count; count++)
			{
				debugPlaybackInfo += sequencerLayers[count].GetDebugPlaybackInfo();
			}

			return debugPlaybackInfo;
		}

		private void OnGUI()
		{
			if (enableDebugging)
			{
				GUI.Label(new Rect(5.0f, 5.0f, Screen.width, Screen.height), playingAtomicNarrativeObjectsText);
			}
		}

		/// <summary>
		/// The definition for a processing layer within the sequencer.
		/// </summary>
		public class SequencerLayer
		{
			private Guid guid = Guid.NewGuid();

			/// <summary>
			/// The max number of seconds ahead of the current playtime that this layer can sequence.
			/// </summary>
			private const float maximumSecondsToSequenceAheadOfTime = 120f;

			/// <summary>
			/// Cached playheadTime from Update method.
			/// </summary>
			private float playheadTime = 0.0f;

			public class StateSkeleton
			{
				public string type = string.Empty;

				public string guid = string.Empty;

				public string parentSequencerLayerGuid = string.Empty;

				public List<string> childSequencerLayerGuids = new List<string>();

				public List<string> sequencedAtomicNarrativeObjectStates = new List<string>();

				public List<string> playedAtomicNarrativeObjectStates = new List<string>();

				//public string playingNarrativeObjectGuid = string.Empty;

				//public string preloadingNarrativeObjectGuid = string.Empty;
			}

			/// <summary>
			/// The types of layer available.
			/// </summary>
			public enum Types
			{
				Default,
				Master,
				Slave,
			}

			/// <summary>
			/// The type of this layer.
			/// </summary>
			public Types type { get; private set; } = Types.Default;

			/// <summary>
			/// Atomic objects which have been sequenced but not yet processed.
			/// </summary>
			private Queue<SequencedAtomicNarrativeObject> sequencedAtomicNarrativeObjects = new Queue<SequencedAtomicNarrativeObject>();

			/// <summary>
			/// Atomic objects which have been played.
			/// </summary>
			private List<SequencedAtomicNarrativeObject> playedAtomicNarrativeObjects = new List<SequencedAtomicNarrativeObject>();

			/// <summary>
			/// If this layer is a child of a layer, this is a reference to the parent.
			/// </summary>
			private SequencerLayer parentSequencerLayer = null;

			/// <summary>
			/// If this layer has any child layers associated with it, they are stored in this list.
			/// </summary>
			private List<SequencerLayer> childSequencerLayers = new List<SequencerLayer>();

			/// <summary>
			/// The currently playing atomic narrative object.
			/// </summary>
			public SequencedAtomicNarrativeObject playingAtomicNarrativeObject { get; private set; } = null;

			/// <summary>
			/// The next atomic narrative object to be played, which is currently preparing itself for its playback to commence.
			/// </summary>
			private SequencedAtomicNarrativeObject preloadingAtomicNarrativeObject = null;

			/// <summary>
			/// The current value associated with the time this layer ends.
			/// </summary>
			private float layerEndTime = 0.0f;

			/// <summary>
			/// Check whether this layer can sequence further objects.
			/// </summary>
			/// <returns></returns>
			public bool CanSequence()
			{
				if (playheadTime + maximumSecondsToSequenceAheadOfTime > layerEndTime)
				{
					return true;
				}

				return false;
			}

			public string GetGuid()
			{
				return guid.ToString();
			}

			public string Save()
			{
				StateSkeleton stateSkeleton = new StateSkeleton();

				stateSkeleton.type = type.ToString();

				stateSkeleton.guid = GetGuid();

				// If this has a parent layer, then return its id, otherwise return empty.
				stateSkeleton.parentSequencerLayerGuid = parentSequencerLayer != null ? parentSequencerLayer.GetGuid() : string.Empty;

				for (int i = 0; i < childSequencerLayers.Count; i++)
				{
					stateSkeleton.childSequencerLayerGuids.Add(childSequencerLayers[i].GetGuid());
				}

				// Convert queue to an array to be iterated over.
				SequencedAtomicNarrativeObject[] sequencedAtomicNarrativeObjectsFromQueue = sequencedAtomicNarrativeObjects.ToArray();

				for (int i = 0; i < sequencedAtomicNarrativeObjectsFromQueue.Length; i++)
				{
					// Store the state for each of the sequenced narrative objects.
					stateSkeleton.sequencedAtomicNarrativeObjectStates.Add(sequencedAtomicNarrativeObjectsFromQueue[i].Save());
				}

				for (int i = 0; i < playedAtomicNarrativeObjects.Count; i++)
				{
					// Store the state for each of the played narrative objects.
					stateSkeleton.playedAtomicNarrativeObjectStates.Add(playedAtomicNarrativeObjects[i].Save());
				}

				return XmlSerialization.SerializeToXmlString(stateSkeleton);
			}

			public void LoadStageOne(string state)
			{
				StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

				Types parsedType = Types.Default;

				if (Enum.TryParse(stateSkeleton.type, out parsedType))
				{
					type = parsedType;
				}

				guid = new Guid(stateSkeleton.guid);

				// Restore the sequenced objects.
				for (int i = 0; i < stateSkeleton.sequencedAtomicNarrativeObjectStates.Count; i++)
				{
					SequencedAtomicNarrativeObject sequencedAtomicNarrativeObject = new SequencedAtomicNarrativeObject();

					sequencedAtomicNarrativeObject.Load(stateSkeleton.sequencedAtomicNarrativeObjectStates[i]);

					sequencedAtomicNarrativeObjects.Enqueue(sequencedAtomicNarrativeObject);
				}

				// Restore the played objects.
				for (int i = 0; i < stateSkeleton.playedAtomicNarrativeObjectStates.Count; i++)
				{
					SequencedAtomicNarrativeObject playedAtomicNarrativeObject = new SequencedAtomicNarrativeObject();

					playedAtomicNarrativeObject.Load(stateSkeleton.playedAtomicNarrativeObjectStates[i]);

					playedAtomicNarrativeObjects.Add(playedAtomicNarrativeObject);
				}
			}

			public void LoadStageTwo(string state, List<SequencerLayer> sequencerLayers)
			{
				StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

				// Find parent from guid.
				for (int i = 0; i < sequencerLayers.Count; i++)
				{
					if (sequencerLayers[i].GetGuid() == stateSkeleton.parentSequencerLayerGuid)
					{
						parentSequencerLayer = sequencerLayers[i];

						break;
					}
				}

				// Find children from guids.
				for (int i = 0; i < sequencerLayers.Count; i++)
				{
					if (stateSkeleton.childSequencerLayerGuids.Contains(sequencerLayers[i].GetGuid()))
					{
						childSequencerLayers.Add(sequencerLayers[i]);
					}
				}
			}


			/// <summary>
			/// Sets the end time of this sequencer layer.
			/// </summary>
			/// <param name="layerEndTime"></param>
			public void SetLayerEndTime(float layerEndTime)
			{
				this.layerEndTime = layerEndTime;
			}

			/// <summary>
			/// Increments the end time of this sequencer layer.
			/// </summary>
			/// <param name="durationToAdd"></param>
			public void AddDurationToLayerEndTime(float durationToAdd)
			{
				layerEndTime += durationToAdd;
			}

			/// <summary>
			/// Get the master child layer for this sequencer layer.
			/// </summary>
			/// <returns></returns>
			public SequencerLayer GetMasterLayer()
			{
				List<SequencerLayer> masterLayers = new List<SequencerLayer>();

				for (int childCount = 0; childCount < childSequencerLayers.Count; childCount++)
				{
					// If a master is found add it to the possibilities.
					if (childSequencerLayers[childCount].type == Types.Master)
					{
						masterLayers.Add(childSequencerLayers[childCount]);
					}
				}

				int longestIndex = -1;
				float longestDuration = 0.0f;

				// Iterate all masters found and find the longest one. This is the master layer we care about.
				for (int masterLayerCount = 0; masterLayerCount < masterLayers.Count; masterLayerCount++)
				{
					if (masterLayers[masterLayerCount].layerEndTime > longestDuration)
					{
						longestIndex = masterLayerCount;
						longestDuration = masterLayers[masterLayerCount].layerEndTime;
					}
				}

				if (longestIndex >= 0)
				{
					return masterLayers[longestIndex];
				}

				return null;
			}

			/// <summary>
			/// Get the slave child layers for a sequencer layer.
			/// </summary>
			/// <returns></returns>
			public SequencerLayer[] GetSlaveLayers()
			{
				// Store slaves here.
				List<SequencerLayer> slaveLayers = new List<SequencerLayer>();

				for (int childCount = 0; childCount < childSequencerLayers.Count; childCount++)
				{
					// If slave, add to returned list.
					if (childSequencerLayers[childCount].type == Types.Slave)
					{
						slaveLayers.Add(childSequencerLayers[childCount]);
					}
				}

				return slaveLayers.ToArray();
			}

			/// <summary>
			/// Recurse through sequencer layers and return all sequenced narrative objects.
			/// </summary>
			/// <returns></returns>
			public void GetSequencedAtomicNarrativeObjectsRecursively(List<SequencedAtomicNarrativeObject> sequencedAtomicNarrativeObjects)
			{
				for (int childCount = 0; childCount < childSequencerLayers.Count; childCount++)
				{
					// Add all sequenced in queue.
					sequencedAtomicNarrativeObjects.AddRange(childSequencerLayers[childCount].sequencedAtomicNarrativeObjects);

					// Add all played as these can be edited while being played.
					sequencedAtomicNarrativeObjects.AddRange(childSequencerLayers[childCount].playedAtomicNarrativeObjects);

					// Get the sequenced atomic narrative objects for any further layers within the children.
					childSequencerLayers[childCount].GetSequencedAtomicNarrativeObjectsRecursively(sequencedAtomicNarrativeObjects);
				}
			}

			/// <summary>
			/// Calculates and returns the end time of this layer.
			/// </summary>
			/// <returns></returns>
			public float GetLayerEndTime()
			{
				if (childSequencerLayers.Count > 0)
				{
					float longestLayerDuration = 0.0f;

					List<SequencerLayer> masterChildSequencerLayers = new List<SequencerLayer>();

					// Find all children which are masters.
					for (int childCount = 0; childCount < childSequencerLayers.Count; childCount++)
					{
						if (childSequencerLayers[childCount].type == Types.Master)
						{
							masterChildSequencerLayers.Add(childSequencerLayers[childCount]);
						}
					}

					// If there is at least one child master.
					if (masterChildSequencerLayers.Count > 0)
					{
						for (int childCount = 0; childCount < masterChildSequencerLayers.Count; childCount++)
						{
							if (masterChildSequencerLayers[childCount].GetLayerEndTime() > longestLayerDuration)
							{
								longestLayerDuration = masterChildSequencerLayers[childCount].GetLayerEndTime();
							}
						}
					}
					else
					{
						for (int childCount = 0; childCount < childSequencerLayers.Count; childCount++)
						{
							if (childSequencerLayers[childCount].GetLayerEndTime() > longestLayerDuration)
							{
								longestLayerDuration = childSequencerLayers[childCount].GetLayerEndTime();
							}
						}
					}

					// Only return the longest layer if that is the furthest item in layer.
					// This is a horrible hack for when you play a layer but sequence an atom to a layer after
					// the layer ends, at which point the longest layer is NOT the duration of the layer.
					if (longestLayerDuration > layerEndTime)
					{
						return longestLayerDuration;
					}
				}

				return layerEndTime;
			}

			/// <summary>
			/// Default ctor used for loading from save.
			/// </summary>
			public SequencerLayer() { }

			/// <summary>
			/// Ctor.
			/// </summary>
			/// <param name="parentSequencerLayer"></param>
			/// <param name="startLayerTime"></param>
			/// <param name="type"></param>
			public SequencerLayer(SequencerLayer parentSequencerLayer, float startLayerTime, Types type = Types.Default)
			{
				this.type = type;
				this.parentSequencerLayer = parentSequencerLayer;

				// If there is a parent, then notify it that it now has a child.
				if (parentSequencerLayer != null)
				{
					parentSequencerLayer.childSequencerLayers.Add(this);
				}

				layerEndTime = startLayerTime;
			}

			/// <summary>
			/// Used to send an atomic narrative object from the narrative space to the sequencer to have the atomic narrative object presented to the viewer.
			/// </summary>
			/// <param name="atomicNarrativeObject"></param>
			public void SequenceAtomicNarrativeObject(AtomicNarrativeObject atomicNarrativeObject)
			{
				sequencedAtomicNarrativeObjects.Enqueue(new SequencedAtomicNarrativeObject(atomicNarrativeObject, layerEndTime));

				AddDurationToLayerEndTime(atomicNarrativeObject.duration);
			}

			public void Terminate(NarrativeSpace narrativeSpace)
			{
				if (playingAtomicNarrativeObject != null)
				{
					playingAtomicNarrativeObject.OnPlaybackFinish(narrativeSpace);
				}
			}

			public void Update(float playheadTime, NarrativeSpace narrativeSpace, Action<SequencedAtomicNarrativeObject> onSequencedAtomicNarrativeObjectPlayed)
			{
				// Store out the current playhead time on the layer.
				this.playheadTime = playheadTime;

				// If ANO currently playing.
				if (playingAtomicNarrativeObject != null)
				{
					// Check if currently playing ANO is complete.
					if (playingAtomicNarrativeObject.playheadFinishTime <= playheadTime)
					{
						playingAtomicNarrativeObject.OnPlaybackFinish(narrativeSpace);

						playingAtomicNarrativeObject = null;
					}
				}

				// If nothing is preloading.
				if (preloadingAtomicNarrativeObject == null)
				{
					// Check for sequenced anos.
					if (sequencedAtomicNarrativeObjects.Count > 0)
					{
						// If there is an ano to be played, start preloading it.
						preloadingAtomicNarrativeObject = sequencedAtomicNarrativeObjects.Dequeue();

						preloadingAtomicNarrativeObject.OnPreload(narrativeSpace);
					}
				}

				// If something is preloading.
				if (preloadingAtomicNarrativeObject != null)
				{
					// Check whether we have reached its start time, if so, then start playback.
					if (preloadingAtomicNarrativeObject.playheadStartTime <= playheadTime)
					{
						playingAtomicNarrativeObject = preloadingAtomicNarrativeObject;

						preloadingAtomicNarrativeObject = null;

						playingAtomicNarrativeObject.OnPlaybackStart(narrativeSpace);

						// Store the object in the played list.
						playedAtomicNarrativeObjects.Add(playingAtomicNarrativeObject);

						// If a callback is specified for on played, then call it with the name of the media.
						onSequencedAtomicNarrativeObjectPlayed?.Invoke(playingAtomicNarrativeObject);
					}
					// Quadrouple the delta to try to ensure that it will not be missed by chance.
					else if (preloadingAtomicNarrativeObject.playheadStartTime <= playheadTime + (Time.smoothDeltaTime * 4.0f))
					{
						preloadingAtomicNarrativeObject.OnPlaybackWillStart();
					}
				}
			}

			/// <summary>
			/// Returns the debug information for this sequencer layer at the time it is invoked.
			/// </summary>
			/// <returns></returns>
			public string GetDebugPlaybackInfo()
			{
				string playbackInfo = string.Empty;

				for (int played = 0; played < playedAtomicNarrativeObjects.Count; played++)
				{
					playbackInfo += playedAtomicNarrativeObjects[played].GetMediaName() + "\n";
				}

				return playbackInfo;
			}
		}

		/// <summary>
		/// Creates a sub layer within the sequencer. This is used for layer narrative objects only.
		/// </summary>
		/// <param name="parentSequencerLayer"></param>
		/// <param name="startTimeOffset"></param>
		/// <param name="layerType"></param>
		/// <returns></returns>
		public SequencerLayer CreateSubLayer(SequencerLayer parentSequencerLayer, float startTimeOffset, SequencerLayer.Types layerType)
		{
			SequencerLayer sequencerLayer = new SequencerLayer(parentSequencerLayer, parentSequencerLayer.GetLayerEndTime() + startTimeOffset, layerType);

			sequencerLayers.Add(sequencerLayer);

			return sequencerLayer;
		}

		/// <summary>
		/// Sequences an atomic narrative object for playback.
		/// </summary>
		/// <param name="sequencerLayer"></param>
		/// <param name="atomicNarrativeObject"></param>
		public void SequenceAtomicNarrativeObject(SequencerLayer sequencerLayer, AtomicNarrativeObject atomicNarrativeObject)
		{
			if (atomicNarrativeObject.duration == 0)
			{
				throw new InvalidDurationException("AtomicNarrativeObject cannot have a zero duration.");
			}

			sequencerLayer.SequenceAtomicNarrativeObject(atomicNarrativeObject);

			if (saveWhenObjectSequenced)
			{
				SaveState();
			}
		}

		/// <summary>
		/// Register that a narrative object has been sequenced at least once.
		/// </summary>
		/// <param name="narrativeObject"></param>
		public void RegisterNarrativeObjectSequenced(NarrativeObject narrativeObject)
		{
			if (!sequencedNarrativeObjects.Contains(narrativeObject))
			{
				sequencedNarrativeObjects.Add(narrativeObject);
			}
		}

		/// <summary>
		/// Test whether a narrative object has been sequenced.
		/// </summary>
		/// <param name="narrativeObject"></param>
		/// <returns></returns>
		public bool HasNarrativeObjectBeenSequenced(NarrativeObject narrativeObject)
		{
			return sequencedNarrativeObjects.Contains(narrativeObject);
		}

		private string GetSaveStateDirectoryPath()
		{
			string saveStateDirectoryPath = Path.Combine(Application.persistentDataPath, "SaveData");

			if (!Directory.Exists(saveStateDirectoryPath))
			{
				Directory.CreateDirectory(saveStateDirectoryPath);
			}

			return saveStateDirectoryPath;
		}

		private string GetSaveFilePath()
		{
			return Path.Combine(GetSaveStateDirectoryPath(), productionGuid + ".xml");
		}

		private IEnumerable<ISaveable> GetSaveables()
		{
			return FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
		}

		public class SaveableState
		{
			public string guid = string.Empty;
			public string state = string.Empty;
		}

		public void DeleteSavedState()
		{
			string saveFilePath = GetSaveFilePath();

			if (File.Exists(saveFilePath))
			{
				File.Delete(saveFilePath);
			}
		}

		public void SaveState()
		{
			IEnumerable<ISaveable> saveableObjects = GetSaveables();

			List<SaveableState> states = new List<SaveableState>();

			foreach (ISaveable saveable in saveableObjects)
			{
				states.Add(new SaveableState { guid = saveable.GetGuid(), state = saveable.Save() });
			}

			string saveFilePath = GetSaveFilePath();

			XmlSerialization.SerializeToXmlFile(saveFilePath, states);
		}

		public void LoadState()
		{
			string saveFilePath = GetSaveFilePath();

			if (File.Exists(saveFilePath))
			{
				IEnumerable<ISaveable> saveableObjects = GetSaveables();

				List<SaveableState> states = XmlSerialization.DeserializeFromXmlFile<List<SaveableState>>(saveFilePath);

				for (int i = 0; i < states.Count; i++)
				{
					foreach (ISaveable saveable in saveableObjects)
					{
						if (states[i].guid == saveable.GetGuid())
						{
							saveable.Load(states[i].state);
						}
					}
				}
			}
		}

		public string GetGuid()
		{
			return gameObjectGuid.Guid;
		}

		public class StateSkeleton
		{
			public float playheadTime = 0.0f;

			public List<string> sequencedNarrativeObjectGuids = new List<string>();

			public List<string> sequencerLayers = new List<string>();

			public class CurrentlyPlayingNarrativeObjectGuids
			{
				public string narrativeObjectGuid = string.Empty;
				public string sequencerLayerGuid = string.Empty;
			}

			public List<CurrentlyPlayingNarrativeObjectGuids> currentlyProcessingNarrativeObjectGuids = new List<CurrentlyPlayingNarrativeObjectGuids>();
		}

		public string Save()
		{
			/*StateSkeleton stateSkeleton = new StateSkeleton();

			// Store playhead time.
			stateSkeleton.playheadTime = PlayheadTime;

			// Save out sequenced narrative objects.
			foreach (NarrativeObject narrativeObject in sequencedNarrativeObjects)
			{
				stateSkeleton.sequencedNarrativeObjectGuids.Add(narrativeObject.GetGuid());
			}

			// Save out sequencer layers.
			for (int i = 0; i < sequencerLayers.Count; i++)
			{
				stateSkeleton.sequencerLayers.Add(sequencerLayers[i].Save());
			}

			// Save the guids for the objects currently processing.
			foreach(KeyValuePair<SequencerLayer, NarrativeObject> pair in currentlyProcessingNarrativeObjects)
			{
				stateSkeleton.currentlyProcessingNarrativeObjectGuids.Add(new StateSkeleton.CurrentlyPlayingNarrativeObjectGuids { narrativeObjectGuid = pair.Value.GetGuid(), sequencerLayerGuid = pair.Key.GetGuid() } );
			}

			return XmlSerialization.SerializeToXmlString(stateSkeleton);*/

			return string.Empty;
		}

		public void Load(string state)
		{
			/*StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

			// Load playhead time.
			PlayheadTime = stateSkeleton.playheadTime;

			NarrativeObject[] narrativeObjects = FindObjectsOfType<NarrativeObject>();

			// Restore the hashset of sequenced narrative objects.
			for (int i = 0; i < stateSkeleton.sequencedNarrativeObjectGuids.Count; i++)
			{
				for (int j = 0; j < narrativeObjects.Length; j++)
				{
					if (narrativeObjects[j].GetGuid() == stateSkeleton.sequencedNarrativeObjectGuids[i])
					{
						sequencedNarrativeObjects.Add(narrativeObjects[j]);

						break;
					}
				}
			}

			// Clear any existing sequencer layers as they are about to be restored.
			sequencerLayers.Clear();

			List<Tuple<SequencerLayer, string>> sequencerLayersAndStates = new List<Tuple<SequencerLayer, string>>();

			for (int i = 0; i < stateSkeleton.sequencerLayers.Count; i++)
			{
				SequencerLayer sequencerLayer = new SequencerLayer();

				sequencerLayer.LoadStageOne(stateSkeleton.sequencerLayers[i]);

				// Store the sequencer layer with its state to allow stage two of loading.
				sequencerLayersAndStates.Add(new Tuple<SequencerLayer, string>(sequencerLayer, stateSkeleton.sequencerLayers[i]));

				// Add to list of sequencer layers, used in stage two of loading.
				sequencerLayers.Add(sequencerLayer);
			}

			for (int i = 0; i < sequencerLayersAndStates.Count; i++)
			{
				sequencerLayersAndStates[i].Item1.LoadStageTwo(sequencerLayersAndStates[i].Item2, sequencerLayers);
			}

			// Restore processing narrative objects.
			// For each guid set stored.
			for (int i = 0; i < stateSkeleton.currentlyProcessingNarrativeObjectGuids.Count; i++)
			{
				// Iterate and find the matching narrative object.
				for (int j = 0; j < narrativeObjects.Length; j++)
				{
					if (narrativeObjects[j].GetGuid() == stateSkeleton.currentlyProcessingNarrativeObjectGuids[i].narrativeObjectGuid)
					{
						// Iterate and find the matching sequencer layer.
						for (int k = 0; k < sequencerLayers.Count; k++)
						{
							if (sequencerLayers[k].GetGuid() == stateSkeleton.currentlyProcessingNarrativeObjectGuids[i].sequencerLayerGuid)
							{
								// If both are found, add the value to the currently sequencing dictionary.
								if (!currentlyProcessingNarrativeObjects.ContainsKey(sequencerLayers[k]))
								{
									currentlyProcessingNarrativeObjects.Add(sequencerLayers[k], null);
								}

								currentlyProcessingNarrativeObjects[sequencerLayers[k]] = narrativeObjects[j];
							}
						}
					}
				}
			}*/
		}
	}
}