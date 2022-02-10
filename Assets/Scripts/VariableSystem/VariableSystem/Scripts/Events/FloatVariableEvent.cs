using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Events
{
	public class FloatVariableEvent : VariableEvent
	{
		public enum Method
		{
			Undefined,
			Set,
		}

		[SerializeField]
		private Method method = Method.Undefined;

		[SerializeField]
		private float value = 0.0f;

		protected override void Invoke()
		{
			List<FloatVariable> floatVariables = GetVariables<FloatVariable>();

			for (int i = 0; i < floatVariables.Count; i++)
			{
				switch (method)
				{
					case Method.Set:

						floatVariables[i].Set(value);

						break;
				}
			}
		}
	}
}
