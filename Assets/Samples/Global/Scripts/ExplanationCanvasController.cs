using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplanationCanvasController : MonoBehaviour
{
	/// <summary>
	/// The panels making up this sample explanation.
	/// </summary>
	public List<ExplanationPanelController> panels = new List<ExplanationPanelController>();

	/// <summary>
	/// Index into panels list of current panel being shown.
	/// </summary>
	private int currentPanelIndex = 0;

	private void Awake()
	{
		// Disable all panels.
		for (int panelCount = 0; panelCount < panels.Count; panelCount++)
		{
			panels[panelCount].gameObject.SetActive(false);
		}

		// Disable last button on first panel (as you cant go backwards from first screen).
		panels[currentPanelIndex].DisableBackButton();
		
		panels[currentPanelIndex].gameObject.SetActive(true);
	}

	public void OpenNextPanel()
	{
		// If there is a panel currently open.
		panels[currentPanelIndex].gameObject.SetActive(false);

		// Increment the panel index.
		currentPanelIndex = Mathf.Clamp(++currentPanelIndex, 0, panels.Count - 1);

		// Enable the new panel game object.
		panels[currentPanelIndex].gameObject.SetActive(true);
	}

	public void OpenLastPanel()
	{
		// If there is a panel currently open.
		panels[currentPanelIndex].gameObject.SetActive(false);

		// Increment the panel index.
		currentPanelIndex = Mathf.Clamp(--currentPanelIndex, 0, panels.Count - 1);

		// Enable the new panel game object.
		panels[currentPanelIndex].gameObject.SetActive(true);
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}
}
