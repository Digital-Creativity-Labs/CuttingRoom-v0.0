using CuttingRoom.VariableSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// The container which holds all of the narrative objects which make up a production.
	/// </summary>
	public class NarrativeSpace : MonoBehaviour, ISaveable
	{
		/// <summary>
		/// Exception which is thrown if a user tries to set an interaction result on an interaction which does not exist.
		/// </summary>
		public class InvalidInteractionException : Exception { public InvalidInteractionException(string message) : base(message) { } }

		/// <summary>
		/// The root narrative object for the narrative space.
		/// Processing starts here.
		/// </summary>
		public NarrativeObject rootNarrativeObject = null;

		/// <summary>
		/// Global variable store for the narrative space.
		/// </summary>
		public VariableStore globalVariableStore = null;

		/// <summary>
		/// Checks whether the narrative space is valid and can be processed.
		/// </summary>
		/// <returns></returns>
		public bool IsValid()
		{
			if (rootNarrativeObject == null)
			{
				return false;
			}

			return true;
		}

		public class StateSkeleton
		{
			public string globalVariableStoreState = string.Empty;
		}

		public string GetGuid()
		{
			return "NarrativeSpace";
		}

		public string Save()
		{
			StateSkeleton stateSkeleton = new StateSkeleton();

			stateSkeleton.globalVariableStoreState = globalVariableStore.Save();

			return XmlSerialization.SerializeToXmlString(stateSkeleton);
		}

		public void Load(string state)
		{
			StateSkeleton stateSkeleton = XmlSerialization.DeserializeFromXmlString<StateSkeleton>(state);

			globalVariableStore.Load(stateSkeleton.globalVariableStoreState);
		}
	}
}