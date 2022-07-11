using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using CuttingRoom.VariableSystem.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class EditorInspector : VisualElement
    {
        /// <summary>
        /// Invoked whenever a constraint is added to a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectAddedConstraint;

        /// <summary>
        /// Invoked whenever a constraint is removed from a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectRemovedConstraint;

        /// <summary>
        /// Invoked whenever a variable is added to a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectAddedVariable;

        /// <summary>
        /// Invoked whenever a variable is removed from a narrative object.
        /// </summary>
        public event Action OnNarrativeObjectRemovedVariable;

        /// <summary>
        /// The style sheet for this visual element.
        /// </summary>
        public StyleSheet StyleSheet { get; set; } = null;

        /// <summary>
        /// Scroll view for the whole window to ensure content isn't squashed to fit.
        /// </summary>
        public ScrollView scrollView = new ScrollView(ScrollViewMode.Vertical);

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorInspector()
        {
            StyleSheet = Resources.Load<StyleSheet>("EditorInspector");

            styleSheets.Add(StyleSheet);

            name = "inspector";

            Add(scrollView);
        }

        /// <summary>
        /// Show global settings.
        /// </summary>
        public void UpdateContentForGlobal(NarrativeSpace narrativeSpace)
        {
            scrollView.Clear();

            VisualElement container = new VisualElement();

            VisualElement variablesSectionContainer = VariableStoreComponent.Render("Global Variables", narrativeSpace.variableStore,
                (variableType) =>
                {
                    AddVariable(narrativeSpace.variableStore, variableType);
                },
                (variable) =>
                {
                    RemoveVariable(narrativeSpace.variableStore, variable);
                });

            variablesSectionContainer.styleSheets.Add(StyleSheet);
            variablesSectionContainer.AddToClassList("inspector-section-container");

            container.Add(variablesSectionContainer);

            scrollView.Add(container);
        }

        /// <summary>
        /// Render the settings for a narrative object node on the sidebar.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        public void UpdateContentForNarrativeObjectNode(NarrativeObjectNode narrativeObjectNode)
        {
            scrollView.Clear();

            VisualElement container = new VisualElement();

            VisualElement settingsSectionContainer = UIElementsUtils.GetContainerWithLabel("Settings");

            List<VisualElement> settingsRows = narrativeObjectNode.GetEditableFieldRows();

            foreach (VisualElement visualElementRow in settingsRows)
            {
                settingsSectionContainer.Add(visualElementRow);
            }

            settingsSectionContainer.styleSheets.Add(StyleSheet);
            settingsSectionContainer.AddToClassList("inspector-section-container");

            VisualElement variablesSectionContainer = VariableStoreComponent.Render("Tags", narrativeObjectNode.NarrativeObject.variableStore,
                (variableType) =>
                {
                    AddVariable(narrativeObjectNode.NarrativeObject.variableStore, variableType);
                },
                (variable) =>
                {
                    RemoveVariable(narrativeObjectNode.NarrativeObject.variableStore, variable);
                });

            variablesSectionContainer.styleSheets.Add(StyleSheet);
            variablesSectionContainer.AddToClassList("inspector-section-container");

            // Output constraints.
            Dictionary<Constraint, VisualElement> outputDecisionPointConstraintRows = narrativeObjectNode.GetOutputDecisionPointConstraintRows();

            VisualElement outputConstraintsSection = GetConstraintSection(outputDecisionPointConstraintRows, "Output Constraints",
                (constraintType) =>
                {
                    AddConstraint(narrativeObjectNode.NarrativeObject.outputSelectionDecisionPoint, constraintType);
                },
                (removedConstraint) =>
                {
                    RemoveConstraint(narrativeObjectNode.NarrativeObject.outputSelectionDecisionPoint, removedConstraint);
                });

            outputConstraintsSection.styleSheets.Add(StyleSheet);
            outputConstraintsSection.AddToClassList("inspector-section-container");

            container.Add(settingsSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(variablesSectionContainer);
            container.Add(UIElementsUtils.GetHorizontalDivider());
            container.Add(outputConstraintsSection);

            scrollView.Add(container);
        }

        /// <summary>
        /// Render the settings for an edge on the sidebar.
        /// </summary>
        /// <param name="outputNarrativeObjectNode"></param>
        /// <param name="inputNarrativeObjectNode"></param>
        public void UpdateContentForEdge(NarrativeObjectNode outputNarrativeObjectNode, NarrativeObjectNode inputNarrativeObjectNode)
        {
            scrollView.Clear();

            VisualElement container = new VisualElement();

            // Candidate constraints.
            Dictionary<Constraint, VisualElement> candidateContraintRows = inputNarrativeObjectNode.GetCandidateConstraintRows();

            VisualElement candidateConstraintsSection = GetConstraintSection(candidateContraintRows, "Applied To This Output",
                (constraintType) =>
                {
                    AddConstraint(inputNarrativeObjectNode.NarrativeObject, constraintType);
                },
                (removedConstraint) =>
                {
                    RemoveConstraint(inputNarrativeObjectNode.NarrativeObject, removedConstraint);
                });

            candidateConstraintsSection.styleSheets.Add(StyleSheet);
            candidateConstraintsSection.AddToClassList("inspector-section-container");

            container.Add(candidateConstraintsSection);

            scrollView.Add(container);
        }

        /// <summary>
        /// Add a variable with the specified type to the specified narrative object.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="variableType"></param>
        private void AddVariable(VariableStore variableStore, VariableType variableType)
        {
            Variable variable = null;

            switch (variableType)
            {
                case VariableType.String:

                    variable = variableStore.gameObject.AddComponent<StringVariable>();

                    break;

                case VariableType.Bool:

                    variable = variableStore.gameObject.AddComponent<BoolVariable>();

                    break;

                case VariableType.Float:

                    variable = variableStore.gameObject.AddComponent<FloatVariable>();

                    break;

                case VariableType.Int:

                    variable = variableStore.gameObject.AddComponent<IntVariable>();

                    break;
            }

            variableStore.variableList.Add(variable);

            OnNarrativeObjectAddedVariable?.Invoke();
        }

        /// <summary>
        /// Remove a variable from a narrative object.
        /// </summary>
        /// <param name="variableStore"></param>
        /// <param name="variable"></param>
        private void RemoveVariable(VariableStore variableStore, Variable variable)
        {
            if (variableStore.variableList.Contains(variable))
            {
                variableStore.variableList.Remove(variable);

                UnityEngine.Object.DestroyImmediate(variable);

                OnNarrativeObjectRemovedVariable?.Invoke();
            }
        }

        /// <summary>
        /// Add a constraint to a narrative object.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        /// <param name="constraintType"></param>
        private void AddConstraint(NarrativeObject narrativeObject, VariableType constraintType)
        {
            Constraint constraint = null;

            switch (constraintType)
            {
                case VariableType.String:

                    constraint = narrativeObject.gameObject.AddComponent<StringVariableConstraint>();

                    break;

                case VariableType.Bool:

                    constraint = narrativeObject.gameObject.AddComponent<BoolVariableConstraint>();

                    break;

                case VariableType.Int:

                    constraint = narrativeObject.gameObject.AddComponent<IntVariableConstraint>();

                    break;

                case VariableType.Float:

                    constraint = narrativeObject.gameObject.AddComponent<FloatVariableConstraint>();

                    break;
            }

            // Add the constraint.
            narrativeObject.constraints.Add(constraint);

            OnNarrativeObjectAddedConstraint?.Invoke();
        }

        /// <summary>
        /// Remove a constraint from a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraint"></param>
        private void RemoveConstraint(NarrativeObject narrativeObject, Constraint constraint)
        {
            narrativeObject.RemoveConstraint(constraint);

            UnityEngine.Object.DestroyImmediate(constraint);

            OnNarrativeObjectRemovedConstraint?.Invoke();
        }

        /// <summary>
        /// Add a constraint to a decision point.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        /// <param name="constraintType"></param>
        private void AddConstraint(DecisionPoint decisionPoint, VariableType constraintType)
        {
            Constraint constraint = null;

            switch (constraintType)
            {
                case VariableType.String:

                    constraint = decisionPoint.gameObject.AddComponent<StringVariableConstraint>();

                    break;

                case VariableType.Bool:

                    constraint = decisionPoint.gameObject.AddComponent<BoolVariableConstraint>();

                    break;

                case VariableType.Int:

                    constraint = decisionPoint.gameObject.AddComponent<IntVariableConstraint>();

                    break;

                case VariableType.Float:

                    constraint = decisionPoint.gameObject.AddComponent<FloatVariableConstraint>();

                    break;
            }

            // Add the constraint.
            decisionPoint.constraints.Add(constraint);

            OnNarrativeObjectAddedConstraint?.Invoke();
        }

        /// <summary>
        /// Remove a constraint from a narrative object.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <param name="constraint"></param>
        private void RemoveConstraint(DecisionPoint decisionPoint, Constraint constraint)
        {
            decisionPoint.RemoveConstraint(constraint);

            UnityEngine.Object.DestroyImmediate(constraint);

            OnNarrativeObjectRemovedConstraint?.Invoke();
        }

        /// <summary>
        /// Generate a sidebar section for the constraints specified in the passed rows.
        /// </summary>
        /// <param name="constraintVisualElements"></param>
        /// <param name="labelText"></param>
        /// <param name="onClickAddConstraint"></param>
        /// <returns></returns>
        private VisualElement GetConstraintSection(Dictionary<Constraint, VisualElement> constraintVisualElements, string labelText, Action<VariableType> onClickAddConstraint, Action<Constraint> onClickRemoveConstraint)
        {
            VisualElement constraintsSection = UIElementsUtils.GetContainerWithLabel(labelText);

            EnumField constraintTypeEnumField = new EnumField(VariableType.String);

            VisualElement addConstraintButton = UIElementsUtils.GetSquareButton("+", () =>
            {
                onClickAddConstraint?.Invoke((VariableType)constraintTypeEnumField.value);
            });

            VisualElement addConstraintControlsContainer = new VisualElement();
            addConstraintControlsContainer.AddToClassList("add-constraint-controls-container");
            addConstraintControlsContainer.styleSheets.Add(StyleSheet);

            addConstraintControlsContainer.Insert(0, addConstraintButton);
            addConstraintControlsContainer.Insert(1, constraintTypeEnumField);

            constraintsSection.Add(addConstraintControlsContainer);

            if (constraintVisualElements.Count > 0)
            {
                foreach (KeyValuePair<Constraint, VisualElement> kvp in constraintVisualElements)
                {
                    VisualElement rowVisualElement = new VisualElement();

                    rowVisualElement.styleSheets.Add(StyleSheet);

                    rowVisualElement.AddToClassList("constraint-row");

                    VisualElement removeConstraintButton = UIElementsUtils.GetSquareButton("-", () =>
                    {
                        onClickRemoveConstraint?.Invoke(kvp.Key);
                    });

                    // Apply the constraints field container class to the returned visual element.
                    kvp.Value.styleSheets.Add(StyleSheet);
                    kvp.Value.AddToClassList("constraint-fields-container");

                    rowVisualElement.Add(removeConstraintButton);
                    rowVisualElement.Add(kvp.Value);

                    constraintsSection.Add(rowVisualElement);
                }
            }

            return constraintsSection;
        }
    }
}