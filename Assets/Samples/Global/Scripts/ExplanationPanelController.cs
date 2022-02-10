using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplanationPanelController : MonoBehaviour
{
	private ExplanationCanvasController explanationCanvasController = null;

	public GameObject backButton = null;

	public void Awake()
	{
		// Panels should be child of a canvas controller.
		explanationCanvasController = transform.parent.GetComponent<ExplanationCanvasController>();
	}

	public void DisableBackButton()
	{
		backButton.SetActive(false);
	}

	public void Next()
	{
		explanationCanvasController.OpenNextPanel();
	}

	public void Last()
	{
		explanationCanvasController.OpenLastPanel();
	}
}
