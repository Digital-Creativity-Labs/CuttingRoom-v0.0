using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public static class VariableStoreComponent
    {
        /// <summary>
        /// Render a variable stores editor components.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="onVariableAdded"></param>
        /// <param name="onVariableRemoved"></param>
        /// <returns></returns>
        public static VisualElement Render(string labelText, VariableStore variableStore, Action<VariableType> onVariableAdded, Action<Variable> onVariableRemoved)
        {
            VisualElement variablesSectionContainer = UIElementsUtils.GetContainerWithLabel(labelText);

            VisualElement addVariableRow = UIElementsUtils.GetRowContainer();

            EnumField variableTypeEnumField = new EnumField(VariableType.String);

            VisualElement addVariableButton = UIElementsUtils.GetSquareButton("+", () =>
            {
                onVariableAdded?.Invoke((VariableType)variableTypeEnumField.value);
            });

            addVariableRow.Add(addVariableButton);
            addVariableRow.Add(variableTypeEnumField);

            variablesSectionContainer.Add(addVariableRow);

            List<VisualElement> variableRows = GetVariableRows(variableStore, onVariableRemoved);

            foreach (VisualElement variableRow in variableRows)
            {
                variablesSectionContainer.Add(variableRow);
            }

            return variablesSectionContainer;
        }

        private static List<VisualElement> GetVariableRows(VariableStore variableStore, Action<Variable> onVariableRemoved)
        {
            List<VisualElement> variableRows = new List<VisualElement>();

            foreach (Variable variable in variableStore.variableList)
            {
                VisualElement rowContainer = UIElementsUtils.GetRowContainer();

                VisualElement removeVariableButton = UIElementsUtils.GetSquareButton("-", () =>
                {
                    onVariableRemoved?.Invoke(variable);
                });

                rowContainer.Add(removeVariableButton);

                VisualElement variableContainer = UIElementsUtils.GetContainer();

                variableContainer.Add(UIElementsUtils.GetObjectField(variable.Key, (variableName) =>
                {
                    variable.Key = variableName;
                }));

                if (variable is StringVariable)
                {
                    StringVariable stringVariable = variable as StringVariable;

                    VisualElement textField = UIElementsUtils.GetTextField(stringVariable.Value, (newValue) =>
                    {
                        stringVariable.Value = newValue;
                    });

                    variableContainer.Add(textField);
                }
                else if (variable is BoolVariable)
                {
                    BoolVariable boolVariable = variable as BoolVariable;

                    VisualElement boolField = UIElementsUtils.GetBoolField(boolVariable.Value, (newValue) =>
                    {
                        boolVariable.Value = newValue;
                    });

                    variableContainer.Add(boolField);
                }
                else if (variable is FloatVariable)
                {
                    FloatVariable floatVariable = variable as FloatVariable;

                    VisualElement floatField = UIElementsUtils.GetFloatField(floatVariable.Value, (newValue) =>
                    {
                        floatVariable.Value = newValue;
                    });

                    variableContainer.Add(floatField);
                }
                else if (variable is IntVariable)
                {
                    IntVariable intVariable = variable as IntVariable;

                    VisualElement intField = UIElementsUtils.GetIntField(intVariable.Value, (newValue) =>
                    {
                        intVariable.Value = newValue;
                    });

                    variableContainer.Add(intField);
                }

                rowContainer.Add(variableContainer);

                variableRows.Add(rowContainer);
            }

            return variableRows;
        }
    }
}
