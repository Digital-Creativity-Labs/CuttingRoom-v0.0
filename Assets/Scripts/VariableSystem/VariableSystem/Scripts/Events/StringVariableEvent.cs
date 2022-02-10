using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Events
{
	public class StringVariableEvent : VariableEvent
	{
		public enum Method
		{
			Undefined,
			Set,
		}

		[SerializeField]
		private Method method = Method.Undefined;

		[SerializeField]
		private string value = string.Empty;

		protected override void Invoke()
		{
			List<StringVariable> stringVariables = GetVariables<StringVariable>();

			for (int i = 0; i < stringVariables.Count; i++)
			{
				switch (method)
				{
					case Method.Set:

						stringVariables[i].Set(value);

						break;
				}
			}
		}
	}
}
