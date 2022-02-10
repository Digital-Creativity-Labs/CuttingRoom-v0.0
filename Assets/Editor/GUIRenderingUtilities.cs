using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GUIRenderingUtilities
{
	public delegate void OnRenderGUIElementCallback(Vector2 position, Vector2 size);

	public class Constraints
	{
		public bool lockedX = false;
		public bool lockedY = false;
	}

	public class RenderSettings
	{
		public Vector2 position = Vector2.zero;
		public Vector2 size = Vector2.zero;
		public Constraints guiConstraints = null;
	}

	/// <summary>
	/// Calculates the size and position of a ui element before calling back to render it.
	/// </summary>
	/// <param name="contentRect">A rect representing the space taken up by rendering a series of elements.</param>
	/// <param name="content">The content to be rendered.</param>
	/// <param name="guiStyle">The style to apply to the rendered object.</param>
	/// <param name="guiConstraints">The constraints used to size the content rect.</param>
	/// <param name="onRenderGUIElement">Callback which should render the actual element.</param>
	public static void RenderGUIElement(RenderSettings renderSettings, GUIContent guiContent, GUIStyle guiStyle, OnRenderGUIElementCallback onRenderGUIElement)
	{
		Vector2 elementSize = guiStyle.CalcSize(guiContent);

		RenderGUIElement(renderSettings, elementSize, onRenderGUIElement);
	}

	public static void RenderGUIElement(RenderSettings renderSettings, Vector2 elementSize, OnRenderGUIElementCallback onRenderGUIElement)
	{
		onRenderGUIElement?.Invoke(renderSettings.position, elementSize);

		renderSettings.position = new Vector2(renderSettings.guiConstraints.lockedX ? renderSettings.position.x : renderSettings.position.x + elementSize.x, renderSettings.guiConstraints.lockedY ? renderSettings.position.y : renderSettings.position.y + elementSize.y);

		float xSize = elementSize.x;
		float ySize = elementSize.y;

		if (renderSettings.guiConstraints.lockedX)
		{
			if (xSize < renderSettings.size.x)
			{
				xSize = renderSettings.size.x;
			}
		}
		else
		{
			xSize += renderSettings.size.x;
		}

		if (renderSettings.guiConstraints.lockedY)
		{
			if (ySize < renderSettings.size.y)
			{
				ySize = renderSettings.size.y;
			}
		}
		else
		{
			ySize += renderSettings.size.y;
		}

		renderSettings.size = new Vector2(xSize, ySize);
	}
}
