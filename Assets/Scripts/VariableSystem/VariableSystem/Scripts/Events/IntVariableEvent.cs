using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Events
{
	public class IntVariableEvent : VariableEvent
	{
		public enum Method
		{
			Undefined,
			Set,
			Increment,
			Decrement,
		}

		[SerializeField]
		private Method method = Method.Undefined;

		[SerializeField]
		private int value = 0;

		protected override void Invoke()
		{
			List<IntVariable> intVariables = GetVariables<IntVariable>();

			for (int i = 0; i < intVariables.Count; i++)
			{
				switch (method)
				{
					case Method.Set:

						intVariables[i].Set(value);

						break;

					case Method.Increment:

						intVariables[i].Increment();

						break;

					case Method.Decrement:

						intVariables[i].Decrement();

						break;
				}
			}
		}
	}
}
