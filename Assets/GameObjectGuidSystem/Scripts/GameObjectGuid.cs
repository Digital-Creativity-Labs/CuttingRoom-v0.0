using System;
using UnityEngine;

[ExecuteInEditMode]
public class GameObjectGuid : MonoBehaviour
{
	[SerializeField]
	string m_Guid;

	public string Guid
	{
		get
		{
			return m_Guid;
		}
	}

#if UNITY_EDITOR

	void Awake()
	{
		if (gameObject.GetComponents<GameObjectGuid>().Length > 1)
		{
			Debug.LogError("GameObjects can only have a single GameObjectGuid component attached to them.");

			DestroyImmediate(this);
		}

		GameObjectGuid[] acGameObjectGuids = FindObjectsOfType<GameObjectGuid>();

		for (int nGameObjectGuid = 0; nGameObjectGuid < acGameObjectGuids.Length; nGameObjectGuid++)
		{
			if (acGameObjectGuids[nGameObjectGuid] != this)
			{
				if (acGameObjectGuids[nGameObjectGuid].Guid == Guid)
				{
					m_Guid = string.Empty;

					break;
				}
			}
		}
	}

	public void GenerateGuid()
	{
		m_Guid = System.Guid.NewGuid().ToString();
	}

#endif
}
