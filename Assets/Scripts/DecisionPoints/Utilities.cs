using CuttingRoom.VariableSystem.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	public delegate IEnumerator OnSelectedCallback(object selected);

	public struct MethodReferences<T>
	{
		public List<T> candidates;
		public DecisionPoint decisionPoint;
		public OnSelectedCallback onSelected;
		public Sequencer sequencer;
		public Sequencer.SequencerLayer sequencerLayer;
		public NarrativeSpace narrativeSpace;
		public NarrativeObject invokingNarrativeObject;
	}

	public static class Utilities
	{
		public static MethodReferences<T> ConvertParameters<T>(object[] parameters) where T : Object
		{
			MethodReferences<T> functionReferences = new MethodReferences<T>();

			// Get the decision point as a decision point object.
			functionReferences.decisionPoint = Convert<DecisionPoint>(parameters[0]);

			// Convert the candidates of the decision point to their derived type.
			functionReferences.candidates = new List<T>();

			// Iterate the candidate gameobjects and pull the component specified.
			for (int count = 0; count < functionReferences.decisionPoint.candidates.Count; count++)
			{
				T component = functionReferences.decisionPoint.candidates[count].GetComponent<T>();

				if (component != null)
				{
					functionReferences.candidates.Add(component);
				}
			}

			functionReferences.onSelected = Convert<OnSelectedCallback>(parameters[1]);
			functionReferences.sequencer = Convert<Sequencer>(parameters[2]);
			functionReferences.sequencerLayer = Convert<Sequencer.SequencerLayer>(parameters[3]);
			functionReferences.invokingNarrativeObject = Convert<NarrativeObject>(parameters[4]);
			functionReferences.narrativeSpace = functionReferences.sequencer.narrativeSpace;

			return functionReferences;
		}

		private static T Convert<T>(object obj)
		{
			if (obj is T)
			{
				return (T)obj;
			}

			try
			{
				return (T)System.Convert.ChangeType(obj, typeof(T));
			}
			catch (System.InvalidCastException)
			{
				return default;
			}
		}
	}
}
