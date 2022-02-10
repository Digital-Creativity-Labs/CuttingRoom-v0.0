using System;
using UnityEngine;

namespace CuttingRoom.VariableSystem.Variables
{
	public class DateTimeNowVariable : Variable
	{
		public DateTime Value { get { return DateTime.Now; } }
	}
}
