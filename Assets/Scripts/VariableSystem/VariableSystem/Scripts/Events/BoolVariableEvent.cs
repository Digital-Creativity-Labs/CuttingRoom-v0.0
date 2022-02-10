using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Events
{
	public class BoolVariableEvent : VariableEvent
	{
		public enum Method
		{
			Undefined,
			Set,
			Invert,
		}

		[SerializeField]
		private Method method = Method.Undefined;

		[SerializeField]
		private bool value = false;

		protected override void Invoke()
		{
			List<BoolVariable> boolVariables = GetVariables<BoolVariable>();

			for (int i = 0; i < boolVariables.Count; i++)
			{
				switch (method)
				{
					case Method.Set:

						boolVariables[i].Set(value);

						break;

					case Method.Invert:

						boolVariables[i].Invert();

						break;
				}
			}
		}
	}
}
