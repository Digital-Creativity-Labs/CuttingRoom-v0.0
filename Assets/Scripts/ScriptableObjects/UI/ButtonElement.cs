using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CuttingRoom
{
	[CreateAssetMenu(fileName = "ButtonElement.asset", menuName = "Cutting Room/UI/Button Element")]
	public class ButtonElement : UIElement
	{
		[SerializeField]
		private string buttonText = string.Empty;

		[SerializeField]
		private Sprite buttonSprite = null;

		public override GameObject Instantiate(Transform parent)
		{
			GameObject instantiated = Instantiate(prefab, parent);

			// If this button has some text assigned, then set it.
			if (!string.IsNullOrEmpty(buttonText))
			{
				Text textForButton = instantiated.GetComponentInChildren<Text>();

				if (textForButton != null)
				{
					textForButton.text = buttonText;
				}
			}

			// If this button has a sprite assigned, then set it.
			if (buttonSprite != null)
			{
				Image buttonImage = instantiated.GetComponent<Image>();

				if (buttonImage != null)
				{
					buttonImage.sprite = buttonSprite;
				}
			}

			// TODO: THIS USED TO BE WHERE INTERACTIONS WERE COMPLETED AND RESULTS STORED.

			return instantiated;
		}
	}
}