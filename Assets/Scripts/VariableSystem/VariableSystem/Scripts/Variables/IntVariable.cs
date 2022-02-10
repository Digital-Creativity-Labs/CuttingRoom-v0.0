using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class IntVariable : Variable
	{
		/// <summary>
		/// Value of this variable.
		/// </summary>
		public int Value = 0;

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
		public void Set(int value)
		{
			Value = value;

			RegisterVariableSet();
		}

		[VariableEventMethod]
		public void Set(string value)
		{
			int parsedValue;

			if (int.TryParse(value, out parsedValue))
			{
				Set(parsedValue);
			}
			else
			{
				Debug.LogError($"Int parsing failed. Value: {value}");
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