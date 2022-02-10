using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom
{
	/// <summary>
	/// Container for the method name, which allows rendering with a custom property drawer.
	/// </summary>
	[Serializable]
	public class MethodNameContainer
	{
		/// <summary>
		/// When method reflection fails, this is thrown.
		/// </summary>
		public class InvalidMethodException : Exception { }

		/// <summary>
		/// The class which contains the method being used.
		/// </summary>
		[HideInInspector]
		public string methodClass = string.Empty;

		/// <summary>
		/// The name of the method to be invoked within the defined method class.
		/// </summary>
		public string methodName = string.Empty;

		/// <summary>
		/// MethodInfo for the defined, reflected method.
		/// </summary>
		public MethodInfo methodInfo { get; private set; } = null;

		/// <summary>
		/// Whether this component has successfully initialised.
		/// </summary>
		public bool Initialised { get { return methodInfo != null; } }

		/// <summary>
		/// Initialises the class and fetches the method to be invoked.
		/// </summary>
		internal void Init()
		{
			Type methodClassType = Type.GetType(methodClass.ToString());

			if (methodClassType != null)
			{
				if (!string.IsNullOrEmpty(methodName))
				{
					methodInfo = methodClassType.GetMethod(methodName);
				}
			}
		}
	}
}