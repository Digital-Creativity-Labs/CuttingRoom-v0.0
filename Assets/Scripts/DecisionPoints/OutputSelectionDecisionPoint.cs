using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// A container for the logic required to select an output from one narrative object to the next after processing on that object has finished.
	/// </summary>
	public partial class OutputSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Subtracted from the end time of the current seqencer layer to give a processing start time when waiting.
		/// </summary>
		[SerializeField]
		private float waitEndOffset = 0.0f;

		/// <summary>
		/// A container for the output selection method definition.
		/// </summary>
		[Serializable]
		public class OutputSelectionMethodNameContainer : MethodNameContainer { }

		/// <summary>
		/// The output selection method definition for this instance of the decision point.
		/// </summary>
		[HelpBox("Built in Output Selection Methods:\n\n• First - Select the first output.\n• Random - Select a random output.\n• Select - Select based on constraints applied to this object.\n• WaitThenSelect - Select based on constraints applied to this object once the object has nearly finished playback.", HelpBoxMessageType.Info)]
		public OutputSelectionMethodNameContainer outputSelectionMethodName = null;

		/// <summary>
		/// The objects which have been selected by this decision point.
		/// </summary>
		public List<NarrativeObject> selectedNarrativeObjects { get; private set; } = new List<NarrativeObject>();

		/// <summary>
		/// Initialisation.
		/// </summary>
		private new void Awake()
		{
			// Force this to be output selection type to be reflected.
			outputSelectionMethodName.methodClass = typeof(OutputSelectionDecisionPoint).AssemblyQualifiedName;

			outputSelectionMethodName.Init();

			base.Awake();
		}

		/// <summary>
		/// Processing loop.
		/// </summary>
		/// <param name="onSelected"></param>
		/// <param name="sequencer"></param>
		/// <returns></returns>
		public IEnumerator Process(OnSelectedCallback onSelected, Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer, NarrativeObject narrativeObject)
		{
			// Store selection callback;
			this.onSelected = onSelected;

			if (outputSelectionMethodName.Initialised)
			{
				// If there is a possible selection then try, else just return null below.
				if (candidates.Count > 0)
				{
					// Store the number of selections before the yield.
					int selectedCountBefore = selectedNarrativeObjects.Count;

					// First parameter is null as all methods being invoked must be static.
					yield return StartCoroutine(outputSelectionMethodName.methodInfo.Name, new object[] { this,
						new OnSelectedCallback(OnSelected),
						sequencer,
						sequencerLayer,
						narrativeObject});

					if (selectedCountBefore == selectedNarrativeObjects.Count)
					{
						Debug.LogWarning($"The OutputSelectionDecisionPoint attached to the GameObject called {gameObject.name} did not make a selection.");
					}
				}
			}
		}

		private IEnumerator OnSelected(object selected)
		{
			if (selected is NarrativeObject)
			{
				selectedNarrativeObjects.Add(selected as NarrativeObject);
			}

			yield return StartCoroutine(onSelected(selected));
		}
	}
}