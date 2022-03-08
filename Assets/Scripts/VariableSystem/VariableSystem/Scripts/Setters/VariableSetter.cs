using System.Collections.Generic;
using UnityEngine;
using CuttingRoom;
using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;

public class VariableSetter : MonoBehaviour
{
    [SerializeField]
    protected VariableName variableName = null;

    [SerializeField]
    protected VariableStoreLocation variableStoreLocation = VariableStoreLocation.Undefined;

    private NarrativeSpace narrativeSpace = null;

    private void Awake()
    {
        narrativeSpace = FindObjectOfType<NarrativeSpace>();
    }

    protected void Set<T>(string value) where T : Variable
    {
        List<T> variables = new List<T>();

        switch (variableStoreLocation)
        {
            case VariableStoreLocation.Global:

                variables = narrativeSpace.globalVariableStore.GetVariables<T>(variableName);

                break;

            case VariableStoreLocation.NarrativeObject:

                NarrativeObject[] narrativeObjects = gameObject.GetComponents<NarrativeObject>();

                foreach (NarrativeObject narrativeObject in narrativeObjects)
                {
                    variables.AddRange(narrativeObject.variableStore.GetVariables<T>(variableName));
                }

                break;
        }

        if (variables != null)
        {
            foreach (T variable in variables)
            {
                variable.SetValueFromString(value);
            }
        }
    }
}
