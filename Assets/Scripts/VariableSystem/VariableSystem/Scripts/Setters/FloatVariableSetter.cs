using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

public class FloatVariableSetter : VariableSetter
{
    [SerializeField]
    private float value = 0;

    public void Set()
    {
        Set<FloatVariable>(value.ToString());
    }
}