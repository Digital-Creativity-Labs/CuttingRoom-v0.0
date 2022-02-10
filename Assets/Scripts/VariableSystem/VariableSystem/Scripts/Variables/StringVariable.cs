using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class StringVariable : Variable
	{
		public string Value = string.Empty;

		[VariableEventMethod]
		public void Set(string value)
		{
			Value = value;

			RegisterVariableSet();
		}

		public override string GetValueAsString()
		{
			return Value;
		}

		public override void SetValueFromString(string value)
		{
			Set(value);
		}
	}
}