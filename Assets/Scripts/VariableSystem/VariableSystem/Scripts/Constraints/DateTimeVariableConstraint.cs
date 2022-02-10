using System;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

namespace CuttingRoom.VariableSystem.Constraints
{
	public class DateTimeConstraint : Constraint
	{
		public int hours = 0;
		public int minutes = 0;
		public int seconds = 0;
		public int milliseconds = 0;
		public int day = 1;
		public int month = 1;
		public int year = 1;

		private DateTime Value;

		public enum ComparisonType
		{
			Undefined,
			EqualTo,
			NotEqualTo,
			LessThan,
			GreaterThan,
			LessThanOrEqualTo,
			GreaterThanOrEqualTo,
		}

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

		public bool EqualTo(List<DateTimeVariable> dateTimeVariables)
		{
			for (int dateTimeVariableCount = 0; dateTimeVariableCount < dateTimeVariables.Count; dateTimeVariableCount++)
			{
				if (dateTimeVariables[dateTimeVariableCount].Value == Value)
				{
					return true;
				}
			}
			return false;
		}

		public bool NotEqualTo(List<DateTimeVariable> dateTimeVariables)
		{
			return !EqualTo(dateTimeVariables);
		}

		public bool LessThan(List<DateTimeVariable> dateTimeVariables)
		{
			for (int dateTimeVariableCount = 0; dateTimeVariableCount < dateTimeVariables.Count; dateTimeVariableCount++)
			{
				if (dateTimeVariables[dateTimeVariableCount].Value < Value)
				{
					return true;
				}
			}

			return false;
		}

		public bool GreaterThan(List<DateTimeVariable> dateTimeVariables)
		{
			for (int dateTimeVariableCount = 0; dateTimeVariableCount < dateTimeVariables.Count; dateTimeVariableCount++)
			{
				if (dateTimeVariables[dateTimeVariableCount].Value > Value)
				{
					return true;
				}
			}

			return false;
		}

		public bool LessThanOrEqualTo(List<DateTimeVariable> dateTimeVariables)
		{
			return LessThan(dateTimeVariables) || EqualTo(dateTimeVariables);
		}

		public bool GreaterThanOrEqualTo(List<DateTimeVariable> dateTimeVariables)
		{
			return GreaterThan(dateTimeVariables) || EqualTo(dateTimeVariables);
		}
	}
}
