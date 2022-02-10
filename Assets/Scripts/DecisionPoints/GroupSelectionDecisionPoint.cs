using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// Holds all logic for a group selecting from its candidates and termination of that selection process.
	/// </summary>
	public partial class GroupSelectionDecisionPoint : DecisionPoint
	{
		/// <summary>
		/// Exception thrown when the specified group selection method is not valid.
		/// </summary>
		public class InvalidGroupSelectionMethodException : Exception { }

		/// <summary>
		/// Exception thrown when the specified group termination method is not valid.
		/// </summary>
		public class InvalidGroupTerminationMethodException : Exception { }

		/// <summary>
		/// A container for the group selection method definition.
		/// </summary>
		[Serializable]
		public class GroupSelectionMethodNameContainer : MethodNameContainer { }

		/// <summary>
		/// A container for the group termination method definition.
		/// </summary>
		[Serializable]
		public class GroupSelectionTerminationMethodNameContainer : MethodNameContainer { }

		/// <summary>
		/// The container to hold the group selection method defintion for this group object.
		/// </summary>
		[Header("Selection")]
		[HelpBox("Built in Group Selection Methods:\n\n• Random - Select randomly from the specified candidates.\n• Count - Select candidates up to the number specified in the Selection Count field.\n• Select - Select based on constraints applied to the group.\n• SelectUnique - Select unique narrative objects based on constraints applied to the group.", HelpBoxMessageType.Info)]
		public GroupSelectionMethodNameContainer groupSelectionMethodName = null;

		/// <summary>
		/// Number of selections made when the group selection method being used is the built in "Count" method.
		/// </summary>
		[Tooltip("Number of selections made when using Count selection method.")]
		public int selectionCount = 1;

		/// <summary>
		/// The container to hold the group termination method defintion for this group object.
		/// </summary>
		[Header("Termination")]
		[HelpBox("Built in Group Selection Methods:\n\n• HasPlayedAll - Group selection terminates once all candidates have been selected at least once.\n• PlayCount - Group selection terminates once the number of selections made equals the value specified in the Play Count field.", HelpBoxMessageType.Info)]
		public GroupSelectionTerminationMethodNameContainer groupSelectionTerminationMethodName = null;

		/// <summary>
		/// Number of selections made when the group termination method being used is the built in "PlayCount" method.
		/// </summary>
		[Tooltip("Controls how many selections are made before termination for PlayCount condition.")]
		public int playCount = 1;

		/// <summary>
		/// A list of the narrative objects which have been selected by this decision point.
		/// </summary>
		public List<object> selectedNarrativeObjects { get; private set; } = new List<object>();

		/// <summary>
		/// Initialises the decision point.
		/// </summary>
		private new void Awake()
		{
			// Force this to be output selection type to be reflected.
			groupSelectionMethodName.methodClass = typeof(GroupSelectionDecisionPoint).AssemblyQualifiedName;

			groupSelectionMethodName.Init();

			groupSelectionTerminationMethodName.methodClass = typeof(GroupSelectionDecisionPoint).AssemblyQualifiedName;

			groupSelectionTerminationMethodName.Init();

			base.Awake();
		}

		/// <summary>
		/// The processing loop for this decision point.
		/// </summary>
		/// <param name="onSelected"></param>
		/// <param name="sequencer"></param>
		/// <returns></returns>
		public IEnumerator Process(OnSelectedCallback onSelected, Sequencer sequencer, Sequencer.SequencerLayer sequencerLayer, GroupNarrativeObject groupNarrativeObject)
		{
			// Store selection callback;
			this.onSelected = onSelected;

			// Clear selected in case you enter the same group twice.
			selectedNarrativeObjects.Clear();

			if (groupSelectionMethodName.Initialised)
			{
				if (groupSelectionTerminationMethodName.Initialised)
				{
					bool shouldTerminate = false;

					while (!shouldTerminate)
					{
						// Check if termination should occur.
						object shouldTerminateObject = groupSelectionTerminationMethodName.methodInfo.Invoke(null, new object[] { new object[] { this, null, sequencer, sequencerLayer, groupNarrativeObject } });

						// If something is returned.
						if (shouldTerminateObject != null)
						{
							if (shouldTerminateObject is bool)
							{
								// It should be a boolean.
								shouldTerminate = (bool)shouldTerminateObject;

								if (!shouldTerminate)
								{
									// Store the number of selections before the yield.
									int selectedCountBefore = selectedNarrativeObjects.Count;

									yield return StartCoroutine(groupSelectionMethodName.methodInfo.Name, new object[] { this,
									new OnSelectedCallback(OnSelected),
									sequencer,
									sequencerLayer,
									groupNarrativeObject});

									if (selectedCountBefore == selectedNarrativeObjects.Count)
									{
										Debug.LogWarning($"The GroupSelectionDecisionPoint attached to the GameObject called {gameObject.name} did not make a selection.");

										shouldTerminate = true;
									}

								}
							}
						}
						else
						{
							shouldTerminate = true;
						}
					}
				}
				else
				{
					throw new InvalidGroupTerminationMethodException();
				}
			}
			else
			{
				throw new InvalidGroupSelectionMethodException();
			}
		}

		private IEnumerator OnSelected(object selected)
		{

			// Store the selection if it is a narrative object?
			if (selected is NarrativeObject)
			{
				selectedNarrativeObjects.Add(selected as NarrativeObject);
			}

			// Callback to other objects which need to know selection has occurred.
			//onSelected?.Invoke(selected);

			yield return StartCoroutine(onSelected(selected));
		}
	}
}
