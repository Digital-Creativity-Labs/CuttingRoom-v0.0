using UnityEngine;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;

public class TestScript : MonoBehaviour
{
	public VariableEvent stringSetEvent = null;

	public void Start()
	{
		stringSetEvent.Invoke();
	}
}
