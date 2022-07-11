using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIElementsUtils
{
    /// <summary>
    /// Get a visual element for a horizontal divider.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetHorizontalDivider()
    {
        VisualElement horizontalDivider = GetDivider("horizontal");

        // The .horizontal class isnt built into Unity like .horizontal so get it from stylesheet and apply.
        StyleSheet styleSheet = Resources.Load<StyleSheet>("Divider");
        horizontalDivider.styleSheets.Add(styleSheet);

        return horizontalDivider;
    }

    /// <summary>
    /// Get a visual element for a vertical divider.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetVerticalDivider()
    {
        VisualElement verticalDivider = GetDivider("vertical");

        // The .vertical class isnt built into Unity like .horizontal so get it from stylesheet and apply.
        StyleSheet styleSheet = Resources.Load<StyleSheet>("Divider");
        verticalDivider.styleSheets.Add(styleSheet);

        return verticalDivider;
    }

    /// <summary>
    /// Get a divider with the specified orientation class added.
    /// </summary>
    /// <param name="orientationClass"></param>
    /// <returns></returns>
    private static VisualElement GetDivider(string orientationClass)
    {
        VisualElement divider = new VisualElement();
        divider.AddToClassList(orientationClass);

        return divider;
    }

    /// <summary>
    /// Get an empty container visual element with a label at the top.
    /// </summary>
    /// <param name="labelText"></param>
    /// <returns></returns>
    public static VisualElement GetContainerWithLabel(string labelText)
    {
        VisualElement container = GetContainer();

        VisualElement labelContainer = GetLabelContainer();

        Label label = new Label(labelText);

        labelContainer.Add(new Label(labelText));

        container.Add(labelContainer);

        return container;
    }

    /// <summary>
    /// Get a container with a row flex direction.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetRowContainer()
    {
        VisualElement container = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("Row");
        container.styleSheets.Add(styleSheet);

        container.AddToClassList("row");

        return container;
    }

    /// <summary>
    /// Get a text field.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="onValueChanged"></param>
    /// <returns></returns>
    public static VisualElement GetTextField(string value, Action<string> onValueChanged)
    {
        TextField textField = new TextField();
        textField.value = value;
        textField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("TextField");
        textField.styleSheets.Add(styleSheet);
        textField.AddToClassList("text-field");

        return textField;
    }

    public static VisualElement GetBoolField(bool value, Action<bool> onValueChanged)
    {
        Toggle toggle = new Toggle();
        toggle.value = value;
        toggle.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("BoolField");
        toggle.styleSheets.Add(styleSheet);
        toggle.AddToClassList("bool-field");

        return toggle;
    }

    public static VisualElement GetFloatField(float value, Action<float> onValueChanged)
    {
        FloatField floatField = new FloatField();
        floatField.value = value;
        floatField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("FloatField");
        floatField.styleSheets.Add(styleSheet);
        floatField.AddToClassList("float-field");

        return floatField;
    }

    public static VisualElement GetIntField(int value, Action<int> onValueChanged)
    {
        IntegerField intField = new IntegerField();
        intField.value = value;
        intField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("IntField");
        intField.styleSheets.Add(styleSheet);
        intField.AddToClassList("int-field");

        return intField;
    }

    public static VisualElement GetObjectField<T>(T value, Action<T> onValueChanged) where T : UnityEngine.Object
    {
        ObjectField objectField = new ObjectField();
        objectField.objectType = typeof(T);
        objectField.value = value;
        objectField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke((T)evt.newValue);
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("ObjectField");
        objectField.styleSheets.Add(styleSheet);
        objectField.AddToClassList("object-field");

        return objectField;
    }

    /// <summary>
    /// Get a standard container.
    /// </summary>
    /// <returns></returns>
    public static VisualElement GetContainer()
    {
        VisualElement container = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("Container");

        container.styleSheets.Add(styleSheet);
        container.AddToClassList("container");

        return container;
    }

    /// <summary>
    /// Get a standard container for a label.
    /// </summary>
    /// <returns></returns>
    private static VisualElement GetLabelContainer()
    {
        VisualElement labelContainer = new VisualElement();

        StyleSheet styleSheet = Resources.Load<StyleSheet>("LabelContainer");

        labelContainer.styleSheets.Add(styleSheet);
        labelContainer.AddToClassList("label-container");

        return labelContainer;
    }

    /// <summary>
    /// Get a small square button.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    public static VisualElement GetSquareButton(string text, Action onClick)
    {
        Button squareButton = new Button(() =>
        {
            onClick?.Invoke();
        });

        StyleSheet styleSheet = Resources.Load<StyleSheet>("SquareButton");

        squareButton.text = text;

        squareButton.styleSheets.Add(styleSheet);
        squareButton.AddToClassList("square-button");

        return squareButton;
    }
}
