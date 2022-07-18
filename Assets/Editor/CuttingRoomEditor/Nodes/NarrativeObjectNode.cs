using CuttingRoom.VariableSystem;
using CuttingRoom.VariableSystem.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class NarrativeObjectNode : Node
    {
        /// <summary>
        /// The narrative object represented by this node.
        /// </summary>
        public NarrativeObject NarrativeObject { get; private set; } = null;

        /// <summary>
        /// The narrative object which is represented by the view which this node exists within.
        /// </summary>
        public NarrativeObject ParentNarrativeObject { get; private set; } = null;

        /// <summary>
        /// The input port for this node.
        /// </summary>
        public Port InputPort { get; private set; } = null;

        /// <summary>
        /// The output port for this node.
        /// </summary>
        public Port OutputPort { get; private set; } = null;

        /// <summary>
        /// The style sheet for this node.
        /// </summary>
        private StyleSheet StyleSheet = null;

        /// <summary>
        /// Event invoked when set as root is clicked.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsNarrativeSpaceRoot;

        /// <summary>
        /// Event invoked when set as root is clicked.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsParentNarrativeObjectRoot;

        /// <summary>
        /// Event invoked when set as candidate is clicked in context menu.
        /// </summary>
        public event Action OnSetAsCandidate;

        /// <summary>
        /// Event invoked when remove as candidate is clicked in context menu.
        /// </summary>
        public event Action OnRemoveAsCandidate;

        /// <summary>
        /// Abstract method which must be implememented to update nodes when the
        /// narrative object they are representing has its values changed in the inspector.
        /// </summary>
        protected abstract void OnNarrativeObjectChanged();

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private Image rootImage = null;

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private Image candidateImage = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="narrativeObject"></param>
        public NarrativeObjectNode(NarrativeObject narrativeObject, NarrativeObject parentNarrativeObject)
        {
            // Store reference to the narrative object being represented.
            NarrativeObject = narrativeObject;

            // Store reference to narrative object whose view container contains this node.
            ParentNarrativeObject = parentNarrativeObject;

            // Set the title of the node to the name of the game object it represents.
            title = narrativeObject.gameObject.name;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "Input";

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.portName = "Output";

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            StyleSheet = Resources.Load<StyleSheet>("NarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            // Get the contents container and add the stylesheet.
            VisualElement contents = this.Q<VisualElement>("contents");
            contents.styleSheets.Add(StyleSheet);
        }

        /// <summary>
        /// Enable the visual element for this being a root.
        /// </summary>
        public void EnableRootVisuals()
        {
            VisualElement titleElement = this.Q<VisualElement>("title");

            if (!titleElement.Contains(rootImage))
            {
                rootImage = new Image();

                rootImage.name = "root-icon";

                rootImage.styleSheets.Add(StyleSheet);

                Texture rootIcon = Resources.Load<Texture>("Icons/root-icon-24x24");

                rootImage.image = rootIcon;

                titleElement.Insert(0, rootImage);
            }
        }

        /// <summary>
        /// Enable the visual element for this being a candidate of a decision point.
        /// </summary>
        public void EnableCandidateVisuals()
        {
            VisualElement titleElement = this.Q<VisualElement>("title");

            if (!titleElement.Contains(candidateImage))
            {
                candidateImage = new Image();

                candidateImage.name = "candidate-icon";

                candidateImage.styleSheets.Add(StyleSheet);

                Texture candidateIcon = Resources.Load<Texture>("Icons/candidate-icon-24x24");

                candidateImage.image = candidateIcon;

                titleElement.Insert(0, candidateImage);
            }
        }

        /// <summary>
        /// Called when this node has to construct its contextual menu.
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Set as Narrative Space Root", OnSetAsNarrativeSpaceRootFromContextualMenu, DropdownMenuAction.Status.Normal);

            if (ParentNarrativeObject != null)
            {
                if (ParentNarrativeObject is GraphNarrativeObject)
                {
                    evt.menu.AppendAction("Set as Parent Narrative Object Root", OnSetAsParentNarrativeObjectRootFromContextualMenu, DropdownMenuAction.Status.Normal);
                }
            }

            // If null, then the object is on the root view level so can't be a candidate.
            if (ParentNarrativeObject != null)
            {
                // If this node exists inside a view which can have candidates.
                if (ParentNarrativeObject is GroupNarrativeObject)
                {
                    // If not currently candidate, add add candidate option, else remove candidate option.
                    GroupNarrativeObject groupNarrativeObject = ParentNarrativeObject.GetComponent<GroupNarrativeObject>();

                    if (groupNarrativeObject.groupSelectionDecisionPoint.Candidates.Contains(NarrativeObject.gameObject))
                    {
                        evt.menu.AppendAction("Remove as Candidate", OnRemoveAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                    else
                    {
                        evt.menu.AppendAction("Set as Candidate", OnSetAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                }
            }

            evt.menu.AppendSeparator();
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsNarrativeSpaceRootFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsNarrativeSpaceRoot?.Invoke(this);
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsParentNarrativeObjectRootFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsParentNarrativeObjectRoot?.Invoke(this);
        }

        /// <summary>
        /// Callback for Set as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsCandidate?.Invoke();
        }

        /// <summary>
        /// Callback for Remove as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnRemoveAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnRemoveAsCandidate?.Invoke();
        }

        /// <summary>
        /// Get the fields to be displayed on the blackboard for this narrative object node.
        /// </summary>
        public virtual List<VisualElement> GetEditableFieldRows()
        {
            List<VisualElement> editorRows = new List<VisualElement>();

            VisualElement nameTextFieldRow = CreateTextFieldRow("Name", NarrativeObject.gameObject.name, (newValue) =>
            {
                NarrativeObject.gameObject.name = newValue;
            });

            VisualElement outputSelectionMethodNameRow = CreateTextFieldRow("Output Selection Method", NarrativeObject.outputSelectionDecisionPoint.outputSelectionMethodName.methodName, (string newValue) =>
            {
                NarrativeObject.outputSelectionDecisionPoint.outputSelectionMethodName.methodName = newValue;
            });

            editorRows.Add(nameTextFieldRow);
            editorRows.Add(outputSelectionMethodNameRow);

            return editorRows;
        }

        public Dictionary<Constraint, VisualElement> GetOutputDecisionPointConstraintRows()
        {
            return GetConstraintRows(NarrativeObject.outputSelectionDecisionPoint.constraints);
        }

        public Dictionary<Constraint, VisualElement> GetCandidateConstraintRows()
        {
            return GetConstraintRows(NarrativeObject.constraints);
        }

        /// <summary>
        /// Get the constraints to be displayed on the blackboard.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Constraint, VisualElement> GetConstraintRows(List<Constraint> constraints)
        {
            Dictionary<Constraint, VisualElement> constraintRows = new Dictionary<Constraint, VisualElement>();

            foreach (Constraint constraint in constraints)
            {
                Label constraintRowLabel = new Label();
                EnumField comparisonTypeEnumField = null;

                void SetConstraintRowLabelText()
                {
#if UNITY_2021_1_OR_NEWER
                    string constraintName = $"{constraint.variableStoreLocation} {(constraint.variableName != null ? constraint.variableName.name : "<color=red>Undefined</color>")} {comparisonTypeEnumField.value} {constraint.Value}";
#else
                    // No rich text before 2021.1
                    string constraintName = $"{constraint.variableStoreLocation} {(constraint.variableName != null ? constraint.variableName.name : "Undefined")} {comparisonTypeEnumField.value} {constraint.Value}";
#endif
                    constraintRowLabel.text = constraintName;
                }

                VisualElement constraintContainer = new VisualElement();

                constraintContainer.styleSheets.Add(StyleSheet);

                constraintContainer.AddToClassList("constraint-container");

                EnumField variableStoreLocationEnumField = new EnumField(constraint.variableStoreLocation);
                variableStoreLocationEnumField.RegisterValueChangedCallback(evt =>
                {
                    constraint.variableStoreLocation = (VariableStoreLocation)evt.newValue;

                    SetConstraintRowLabelText();
                });

                constraintContainer.Add(variableStoreLocationEnumField);

                ObjectField variableNameField = new ObjectField();
                variableNameField.objectType = typeof(VariableName);
                variableNameField.value = constraint.variableName;
                variableNameField.RegisterValueChangedCallback(evt =>
                {
                    constraint.variableName = evt.newValue as VariableName;

                    SetConstraintRowLabelText();
                });

                constraintContainer.Add(variableNameField);

                comparisonTypeEnumField = GetConstraintComparisonTypeEnumField(constraint, () =>
                {
                    SetConstraintRowLabelText();
                });

                // Add the comparison type enum field based on the type of constraint being visualised.
                constraintContainer.Add(comparisonTypeEnumField);

                // Add a field for the value of the constraint specified.
                AddConstraintValueField(constraintContainer, constraint, () =>
                {
                    SetConstraintRowLabelText();
                });

                SetConstraintRowLabelText();

                VisualElement constraintRow = new VisualElement();
                constraintRow.Add(constraintRowLabel);
                constraintRow.Add(constraintContainer);

                constraintRows.Add(constraint, constraintRow);
            }

            return constraintRows;
        }

        /// <summary>
        /// Find the correct constraint object on a game object with a reference to the base instance of constraint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private T GetConstraint<T>(Constraint constraint) where T : Constraint
        {
            T[] typedVariableConstraints = constraint.GetComponents<T>();

            T typedVariableConstraint = typedVariableConstraints.Where(typedVariableConstraint => typedVariableConstraint == constraint).FirstOrDefault();

            return typedVariableConstraint;
        }

        /// <summary>
        /// Gets the enum field which represents the comparison type of the specified constraint.
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private EnumField GetConstraintComparisonTypeEnumField(Constraint constraint, Action onValueChanged)
        {
            EnumField enumField = null;

            if (constraint is StringVariableConstraint)
            {
                StringVariableConstraint stringVariableConstraint = GetConstraint<StringVariableConstraint>(constraint);

                enumField = new EnumField(stringVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    stringVariableConstraint.comparisonType = (StringVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is BoolVariableConstraint)
            {
                BoolVariableConstraint boolVariableConstraint = GetConstraint<BoolVariableConstraint>(constraint);

                enumField = new EnumField(boolVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    boolVariableConstraint.comparisonType = (BoolVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is FloatVariableConstraint)
            {
                FloatVariableConstraint floatVariableConstraint = GetConstraint<FloatVariableConstraint>(constraint);

                enumField = new EnumField(floatVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    floatVariableConstraint.comparisonType = (FloatVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }
            else if (constraint is IntVariableConstraint)
            {
                IntVariableConstraint intVariableConstraint = GetConstraint<IntVariableConstraint>(constraint);

                enumField = new EnumField(intVariableConstraint.comparisonType);
                enumField.RegisterValueChangedCallback(evt =>
                {
                    intVariableConstraint.comparisonType = (IntVariableConstraint.ComparisonType)evt.newValue;

                    onValueChanged?.Invoke();
                });
            }

            return enumField;
        }

        /// <summary>
        /// Adds the correct type of value field for the specified constraint.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="constraint"></param>
        private void AddConstraintValueField(VisualElement container, Constraint constraint, Action onValueChanged)
        {
            if (constraint is StringVariableConstraint)
            {
                StringVariableConstraint stringVariableConstraint = GetConstraint<StringVariableConstraint>(constraint);

                TextField textField = new TextField();
                textField.value = stringVariableConstraint.value;
                textField.RegisterValueChangedCallback(evt =>
                {
                    stringVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });

                container.Add(textField);
            }
            else if (constraint is BoolVariableConstraint)
            {
                BoolVariableConstraint boolVariableConstraint = GetConstraint<BoolVariableConstraint>(constraint);

                Toggle toggle = new Toggle();
                toggle.value = boolVariableConstraint.value;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    boolVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });

                container.Add(toggle);
            }
            else if (constraint is FloatVariableConstraint)
            {
                FloatVariableConstraint floatVariableConstraint = GetConstraint<FloatVariableConstraint>(constraint);

                FloatField floatField = new FloatField();
                floatField.value = floatVariableConstraint.value;
                floatField.RegisterValueChangedCallback(evt =>
                {
                    floatVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });

                container.Add(floatField);
            }
            else if (constraint is IntVariableConstraint)
            {
                IntVariableConstraint intVariableConstraint = GetConstraint<IntVariableConstraint>(constraint);

                IntegerField intField = new IntegerField();
                intField.value = intVariableConstraint.value;
                intField.RegisterValueChangedCallback(evt =>
                {
                    intVariableConstraint.value = evt.newValue;

                    onValueChanged?.Invoke();
                });

                container.Add(intField);
            }
        }

        /// <summary>
        /// Create a blackboard row with a text field.
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        /// <returns></returns>
        protected VisualElement CreateTextFieldRow(string labelText, string value, Action<string> OnValueChanged)
        {
            TextField textField = new TextField();
            textField.value = value;
            textField.RegisterValueChangedCallback(evt =>
            {
                OnValueChanged?.Invoke(evt.newValue);
            });

            BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), textField);

            blackboardRow.expanded = true;

            return blackboardRow;
        }

        /// <summary>
        /// Create a blackboard row with an object field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="labelText"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        /// <returns></returns>
        protected VisualElement CreateObjectFieldRow<T>(string labelText, T value, Action<UnityEngine.Object> OnValueChanged) where T : UnityEngine.Object
        {
            VisualElement objectField = UIElementsUtils.GetObjectField<T>(value, OnValueChanged);

            BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), objectField);

            blackboardRow.expanded = true;

            return blackboardRow;
        }

        /// <summary>
        /// Create a blackboard row with a float field.
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        /// <returns></returns>
        protected VisualElement CreateFloatFieldRow(string labelText, float value, Action<float> OnValueChanged)
        {
            FloatField floatField = new FloatField();
            floatField.value = value;
            floatField.RegisterValueChangedCallback(evt =>
            {
                OnValueChanged?.Invoke(evt.newValue);
            });

            BlackboardRow blackboardRow = new BlackboardRow(new Label(labelText), floatField);

            blackboardRow.expanded = true;

            return blackboardRow;
        }
    }
}
