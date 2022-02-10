using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CuttingRoom.VariableSystem.Constraints;

namespace CuttingRoom
{
	/// <summary>
	/// Base class for all decisions required for processing an object oriented narrative space.
	/// </summary>
	public class DecisionPoint : MonoBehaviour
	{
		/// <summary>
		/// The objects attached to this decision point.
		/// </summary>
		[SerializeField]
		internal List<GameObject> candidates = new List<GameObject>();

		/// <summary>
		/// The constraints which have been applied to this decision point.
		/// </summary>
		[SerializeField]
		internal List<Constraint> constraints = new List<Constraint>();

		/// <summary>
		/// Selection callback.
		/// </summary>
		protected OnSelectedCallback onSelected = null;

		/// <summary>
		/// Candidate bin for this decision point (optional).
		/// </summary>
		[SerializeField]
		protected CandidateBin candidateBin = null;

		/// <summary>
		/// Whether this decision point has a candidate bin assigned to it.
		/// </summary>
		protected bool HasCandidateBin { get { return candidateBin != null; } }

		/// <summary>
		/// Load the candidates from the candidate bin and assign them as candidates.
		/// </summary>
		private void LoadCandidateBin()
		{
			candidates.AddRange(candidateBin?.InstantiateCandidates());
		}

		/// <summary>
		/// Unity callback.
		/// </summary>
        protected void Awake()
        {
			if (HasCandidateBin)
			{
				LoadCandidateBin();
			}
        }

#if UNITY_EDITOR

        /// <summary>
        /// Used by the Narrative Space editor to link nodes together.
        /// </summary>
        /// <param name="candidate"></param>
        public void AddCandidate(GameObject candidate)
		{
			if (!candidates.Contains(candidate))
			{
				candidates.Add(candidate);
			}
		}

		public void RemoveCandidate(GameObject candidate)
		{
			if (candidates.Contains(candidate))
			{
				candidates.Remove(candidate);
			}
		}

		public bool HasCandidate(GameObject candidate)
		{
			return candidates.Contains(candidate);
		}

		/// <summary>
		/// Ensure that the candidates on this decisionpoint are valid.
		/// </summary>
		public void Validate()
		{
			int removedCount = candidates.RemoveAll((GameObject candidate) => { return candidate == null; });

			if (removedCount > 0)
			{
				Debug.LogWarning($"Removed {removedCount} null or empty candidates from decision point attached to GameObject named {gameObject.name}");
			}
		}

		/// <summary>
		/// Editor method used to return the candidates as narrative objects
		/// for the rendering of the NarrativeSpace in the Narrative Space Editor.
		/// </summary>
		/// <returns></returns>
		public NarrativeObject[] GetCandidatesAsNarrativeObjects()
		{
			List<NarrativeObject> narrativeObjects = new List<NarrativeObject>();

			for (int count = 0; count < candidates.Count; count++)
			{
				if (candidates[count] != null)
				{
					NarrativeObject narrativeObject = candidates[count].GetComponent<NarrativeObject>();

					if (narrativeObject != null)
					{
						narrativeObjects.Add(narrativeObject);
					}
				}
			}

			return narrativeObjects.ToArray();
		}

#endif
	}
}
