using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class BoolVariable : Variable
	{
		public bool Value = false;

		[VariableEventMethod]
		public void Invert()
		{
			Set(!Value);
		}

		[VariableEventMethod]
		public void Set(bool value)
		{
			Value = value;

			RegisterVariableSet();
		}

		[VariableEventMethod]
		public void Set(string value)
		{
			bool parsedValue;

			if (bool.TryParse(value, out parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Bool parsing failed. Value: {value}");
			}
		}

		public override string GetValueAsString()
		{
			return Value.ToString();
		}

		public override void SetValueFromString(string value)
		{
			Set(value);
		}
	}
}
