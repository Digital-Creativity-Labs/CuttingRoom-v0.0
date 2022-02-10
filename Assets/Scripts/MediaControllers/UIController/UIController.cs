using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CuttingRoom.Exceptions;

namespace CuttingRoom
{
	/// <summary>
	/// A generic controller for Unity GUI elements.
	/// </summary>
	public class UIController : MediaController
	{
		private GameObject uiContainer = null;
		private List<GameObject> uiElements = new List<GameObject>();

		private string uiContainerName = string.Empty;

		public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Preload(narrativeSpace, atomicNarrativeObject);

			// Find a UI container scriptable object.
			UIContainer container = mediaSource.mediaSourceData.objects.FirstOrDefault(media => media is UIContainer) as UIContainer;

			if (container == null)
			{
				throw new InvalidMediaException("UI media source does not define a UIContainer scriptable object instance.");
			}

			// Instantiate the container.
			uiContainer = Instantiate(container.prefab, atomicNarrativeObject.mediaParent) as GameObject;

			// Store the name of the UI Container for reference in debugging.
			uiContainerName = uiContainer.name;

			// Find the UI elements to be spawned in the container.
			IEnumerable<UnityEngine.Object> elements = mediaSource.mediaSourceData.objects.Where(media => media is UIElement);

			foreach (UnityEngine.Object element in elements)
			{
				UIElement uiElement = element as UIElement;

				// Spawn the element defined, attached to the container for the elements.
				GameObject spawnedElement = uiElement.Instantiate(uiContainer.transform);

				// Disable the element.
				spawnedElement.SetActive(false);

				// Add it to the list of spawned objects for removal later.
				uiElements.Add(spawnedElement);				
			}
		}

		public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
		{
			for (int elementCount = 0; elementCount < uiElements.Count; elementCount++)
			{
				uiElements[elementCount].SetActive(true);
			}
		}

		public override void Stop(NarrativeSpace narrativeSpace)
		{
			for (int elementCount = 0; elementCount < uiElements.Count; elementCount++)
			{
				uiElements[elementCount].SetActive(false);
			}
		}

		public override void Shutdown(Action onShutdownComplete)
		{
			for (int elementCount = 0; elementCount < uiElements.Count; elementCount++)
			{
				Destroy(uiElements[elementCount]);
			}

			Destroy(uiContainer);

			// Call shutdown on base.
			base.Shutdown(onShutdownComplete);
		}

		public override string GetMediaName()
		{
			return uiContainerName;
		}
	}
}
