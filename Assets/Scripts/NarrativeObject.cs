using System.Collections.Generic;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// The base narrative object class which is inherited by any object used within a narrative space.
	/// </summary>
	[RequireComponent(typeof(OutputSelectionDecisionPoint), typeof(VariableStore), typeof(GameObjectGuid))]
	public class NarrativeObject : MonoBehaviour, ISaveable
	{
		/// <summary>
		/// Delegate for processing callback events.
		/// </summary>
		public delegate void OnProcessCallback(float triggeringNarrativeObjectSequencerLayerEndTime);

		/// <summary>
		/// Called when processing begins for this object.
		/// </summary>
		public event OnProcessCallback OnProcessStart;

		/// <summary>
		/// Called when processing ends for this object.
		/// </summary>
		public event OnProcessCallback OnProcessFinish;

		/// <summary>
		/// Contains the settings for selecting the next output object once processing of this narrative object is complete.
		/// </summary>
		public OutputSelectionDecisionPoint outputSelectionDecisionPoint = null;

		/// <summary>
		/// Constraints applied to this narrative object.
		/// </summary>
		public List<Constraint> constraints = new List<Constraint>();

		/// <summary>
		/// The variable store for this narrative object.
		/// </summary>
		public VariableStore variableStore = null;

		/// <summary>
		/// Guid for this game object.
		/// Used by the save system.
		/// </summary>
		public GameObjectGuid gameObjectGuid = null;

		protected NarrativeObject selectedNarrativeObject = null;

		protected Sequencer sequencer = null;

		protected Sequencer.SequencerLayer sequencerLayer = null;

		public delegate void OnEventCallback();
		public event OnEventCallback OnPlaybackStart = null;
		public event OnEventCallback OnPlaybackFinish = null;

		public void InvokeOnPlaybackStart()
		{
			OnPlaybackStart?.Invoke();
		}

		public void InvokeOnPlaybackFinish()
		{
			OnPlaybackFinish?.Invoke();
		}

		/// <summary>
		/// Called by child classes to indicate processing is starting.
		/// </summary>
		protected void InvokeOnProcessStart(float triggeringNarrativeObjectSequencerLayerEndTime)
		{
			OnProcessStart?.Invoke(triggeringNarrativeObjectSequencerLayerEndTime);
		}

		/// <summary>
		/// Called by child classes to indicate processing is finished.
		/// </summary>
		protected void InvokeOnProcessFinish(float triggeringNarrativeObjectSequencerLayerEndTime)
		{
			OnProcessFinish?.Invoke(triggeringNarrativeObjectSequencerLayerEndTime);
		}

		/// <summary>
		/// The processing loop for the narrative object.
		/// </summary>
		/// <param name="sequencer"></param>
		/// <param name="sequencerLayer"></param>
		/// <returns></returns>
		public virtual IEnumerator Process(Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer)
		{
			// Callback to sequencer to notify that this object is currently processing.
			sequencer.OnStartProcessingNarrativeObject(this, sequencerLayer);

			// Select output from this narrative object.
			yield return outputSelectionDecisionPoint.Process(OnSelected, sequencer, sequencerLayer, this);

			// If something was selected.
			if (selectedNarrativeObject != null)
			{
				// Process the selection.
				yield return selectedNarrativeObject.Process(sequencer, sequencerLayer);
			}
		}

		private IEnumerator OnSelected(object selected)
		{
			if (selected != null)
			{
				if (selected is NarrativeObject)
				{
					selectedNarrativeObject = selected as NarrativeObject;
				}
			}

			yield return null;
		}

		public string GetGuid()
		{
			return gameObjectGuid.Guid;
		}

		public class StateSkeleton
		{
			public string variableStoreState = string.Empty;
		}

		public string Save()
		{
			StateSkeleton stateSkeleton = new StateSkeleton();

			stateSkeleton.variableStoreState = variableStore.Save();

			return XmlSerialization.SerializeToXmlString(stateSkeleton);
		}

		public void Load(string state)
		{
			StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

			variableStore.Load(stateSkeleton.variableStoreState);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Used to show a thumbnail of this media in the narrative space editor.
		/// </summary>
		/// <param name="mediaSource"></param>
		/// <returns></returns>
		public virtual Task<Texture2D> GetThumbnail(object args = null) { return null; }
#endif
	}
}