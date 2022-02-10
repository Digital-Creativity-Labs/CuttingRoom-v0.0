using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class DateTimeVariable : Variable
	{
		public int hours = 0;
		public int minutes = 0;
		public int seconds = 0;
		public int milliseconds = 0;
		public int day = 1;
		public int month = 1;
		public int year = 1;

		public DateTime Value = default;

		private void OnValidate()
		{
			hours = Mathf.Clamp(hours, 0, 23);
			minutes = Mathf.Clamp(minutes, 0, 59);
			seconds = Mathf.Clamp(seconds, 0, 59);
			milliseconds = Mathf.Clamp(milliseconds, 0, 999);
			// To use DateTime.DaysInMonth, year must be clamped to 9999.
			// Don't worry future developer, existence doesn't end in the year 10,000.
			year = Mathf.Clamp(year, 1, 9999);
			month = Mathf.Clamp(month, 1, 12);
			day = Mathf.Clamp(day, 1, DateTime.DaysInMonth(year, month));
		}

		private void Awake()
		{
			Value = new DateTime(year, month, day, hours, minutes, seconds, milliseconds);
		}

		public void Set(string value)
		{
			DateTime parsedValue = default;

			bool success = DateTime.TryParse(value, out parsedValue);

			if (success)
			{
				Set(parsedValue);
			}
		}

		public void Set(DateTime value)
		{
			Value = value;

			RegisterVariableSet();
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