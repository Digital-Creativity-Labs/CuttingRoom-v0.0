using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

public class StringVariableSetter : VariableSetter
{
    [SerializeField]
    private string value = string.Empty;

    public void Set()
    {
        Set<StringVariable>(value);
    }
}