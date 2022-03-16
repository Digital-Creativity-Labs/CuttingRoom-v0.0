using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
	/// <summary>
	/// The atomic narrative object is an object which holds some media to be presented as part of an overarching presentation.
	/// </summary>
	[ExecuteInEditMode]
	public class AtomicNarrativeObject : NarrativeObject
	{
		/// <summary>
		/// The parent object in the scene which has the generated media attached to it as a child.
		/// </summary>
		[SerializeField]
		public Transform mediaParent = null;

		/// <summary>
		/// The media sources associated with this object.
		/// </summary>
		[SerializeField]
		public MediaSource mediaSource = null;

		/// <summary>
		/// Controls layering within a scene. If this is true, the object is set to be on top of all other objects.
		/// In the case that there are two objects or more with this enabled, they will appear in the order they are executed.
		/// </summary>
		[SerializeField]
		public bool appearsOnTop = false;

		/// <summary>
		/// The offset from the start of a piece of media, in seconds, after which playback begins.
		/// </summary>
		public float inTime = 0.0f;
		/// <summary>
		/// Duration of playback in seconds from the specified in time.
		/// </summary>
		public float duration = 0.0f;

		/// <summary>
		/// Processing loop for an atomic narrative object.
		/// </summary>
		/// <param name="sequencer"></param>
		/// <param name="sequencerLayer"></param>
		/// <returns></returns>
		public override IEnumerator Process(Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer)
		{
			this.sequencer = sequencer;
			this.sequencerLayer = sequencerLayer;

			if (mediaSource == null)
			{
				throw new InvalidMediaException("Atomic Narrative Object has no media source attached to it.");
			}

			sequencer.RegisterNarrativeObjectSequenced(this);

			// Check whether the layer has the capacity to sequence further.
			// This is intended to prevent infinite loops sequencing infinitely
			// and draining the resources of the computer running the production.
			while (!sequencerLayer.CanSequence())
			{
				yield return new WaitForEndOfFrame();
			}

			// Callback for processing starting.
			InvokeOnProcessStart(sequencerLayer.GetLayerEndTime());

			sequencer.SequenceAtomicNarrativeObject(sequencerLayer, this);

			// Process the base functionality, output selection.
			yield return base.Process(sequencer, sequencerLayer);

			// Callback for processing finishing.
			InvokeOnProcessFinish(sequencerLayer.GetLayerEndTime());
		}

#if UNITY_EDITOR
		/// <summary>
		/// Used to show a thumbnail of this media in the narrative space editor.
		/// </summary>
		/// <param name="mediaSource"></param>
		/// <returns></returns>
		public override async Task<Texture2D> GetThumbnail(object args = null)
		{
			if (mediaSource != null)
			{
				MediaController mediaController = mediaSource.mediaControllerPrefab.GetComponent<MediaController>();

				return await mediaController.GetThumbnail(mediaSource);
			}

			return new Texture2D(0, 0);
		}
#endif
	}
}