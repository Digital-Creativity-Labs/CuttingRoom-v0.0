using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Events
{
	public class VariableEvent : MonoBehaviour
	{
		public enum TriggerEvent
		{
			Undefined,
			OnPlaybackStart,
			OnPlaybackFinish,
		}

		[SerializeField]
		private TriggerEvent triggerEvent = TriggerEvent.Undefined;

		[SerializeField]
		private NarrativeObject narrativeObject = null;

		[Space]

		[SerializeField]
		private VariableStoreLocation variableStoreLocation = VariableStoreLocation.Undefined;

		[SerializeField]
		private VariableName variableName = null;

		private NarrativeSpace narrativeSpace = null;

		private void Awake()
		{
			narrativeSpace = FindObjectOfType<NarrativeSpace>();
		}

		private void Start()
		{
			switch (triggerEvent)
			{
				case TriggerEvent.OnPlaybackStart:

					narrativeObject.OnPlaybackStart += () =>
					{
						Invoke();
					};

					break;

				case TriggerEvent.OnPlaybackFinish:

					narrativeObject.OnPlaybackFinish += () =>
					{
						Invoke();
					};

					break;
			}
		}

		protected List<T> GetVariables<T>() where T : Variable
		{
			switch (variableStoreLocation)
			{
				case VariableStoreLocation.Global:

					return narrativeSpace.globalVariableStore.GetVariables<T>(variableName);

				case VariableStoreLocation.NarrativeObject:

					return narrativeObject.variableStore.GetVariables<T>(variableName);

				default:

					Debug.LogError("VariableEvent could not find the variable associated with it.");

					return null;
			}
		}

		protected virtual void Invoke() { }
	}
}
