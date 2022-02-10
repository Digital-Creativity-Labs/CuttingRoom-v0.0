using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.VariableSystem
{
	[CreateAssetMenu(fileName = "VariableName.asset", menuName = "Cutting Room/Variables/Variable Name")]
	public class VariableName : ScriptableObject
	{
		public string variableName = string.Empty;
	}
}