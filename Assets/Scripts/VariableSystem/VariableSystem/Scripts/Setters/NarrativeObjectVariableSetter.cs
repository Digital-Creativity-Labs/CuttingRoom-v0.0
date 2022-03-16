using CuttingRoom;
using CuttingRoom.VariableSystem.Variables;
using UnityEngine;

public class NarrativeObjectVariableSetter : VariableSetter
{
    [SerializeField]
    private NarrativeObject value = null;

    public void Set()
    {
        Set<NarrativeObjectVariable>(value.ToString());
    }
}