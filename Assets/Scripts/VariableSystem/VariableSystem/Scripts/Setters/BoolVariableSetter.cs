using UnityEngine;
using CuttingRoom.VariableSystem.Variables;

public class BoolVariableSetter : VariableSetter
{
    [SerializeField]
    private bool value = false;

    public void Set()
    {
        Set<BoolVariable>(value.ToString());
    }
}
