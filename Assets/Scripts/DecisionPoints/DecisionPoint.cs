using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CuttingRoom.VariableSystem.Constraints;
using System.Linq;

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
		/// The candidates for this decision point.
		/// </summary>
		public List<GameObject> Candidates { get { return candidates; } }

		/// <summary>
		/// The constraints which have been applied to this decision point.
		/// </summary>
		public List<Constraint> constraints = new List<Constraint>();

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

		public event Action OnCandidatesChanged;

		private List<GameObject> cachedCandidates = new List<GameObject>();

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

		public void OnValidate()
		{
			// Find candidates which are in the cached list but not in the actual inspector list (so they have been removed in inspector).
			IEnumerable<GameObject> removedCandidates = cachedCandidates.Except(candidates);

			// Find candidates which are in the actual inspector list but not in the cached list (so they have been added in inspector).
			IEnumerable<GameObject> addedCandidates = candidates.Except(cachedCandidates);

			if (removedCandidates.Count() > 0 || addedCandidates.Count() > 0)
			{
				OnCandidatesChanged?.Invoke();

				CacheCandidates();
			}
		}

		private void CacheCandidates()
		{
			// Cache the new connected guids list.
			cachedCandidates.Clear();
			cachedCandidates.AddRange(candidates);
		}

		public void AddConstraint(Constraint constraint)
		{
			if (!constraints.Contains(constraint))
			{
				constraints.Add(constraint);
			}
		}

		public void RemoveConstraint(Constraint constraint)
		{
			if (constraints.Contains(constraint))
			{
				constraints.Remove(constraint);
			}
		}

#endif
	}
}
