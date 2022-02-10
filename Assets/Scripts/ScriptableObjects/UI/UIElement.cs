using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	//[CreateAssetMenu(fileName = "UIElement.asset", menuName = "Cutting Room/UI/Element")]
	public class UIElement : ScriptableObject
	{
		[SerializeField]
		protected GameObject prefab = null;

		[SerializeField]
		protected string interactionName = string.Empty;

		[SerializeField]
		protected string interactionResult = string.Empty;

		public virtual GameObject Instantiate(Transform parent)
		{
			throw new NotImplementedException("UIElement does not define an Instantiation routine.");
		}
	}
}
