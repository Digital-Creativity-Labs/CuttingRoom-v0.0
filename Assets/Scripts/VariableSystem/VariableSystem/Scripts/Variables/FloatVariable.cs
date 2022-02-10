using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class FloatVariable : Variable
	{
		public float Value = 0.0f;

		[VariableEventMethod]
		public void Increment()
		{
			Value++;
		}

		[VariableEventMethod]
		public void Decrement()
		{
			Value--;
		}

		[VariableEventMethod]
		public void Set(float value)
		{
			Value = value;

			RegisterVariableSet();
		}

		[VariableEventMethod]
		public void Set(string value)
		{
			float parsedValue;

			if (float.TryParse(value, out parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Float parsing failed. Value: {value}");
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