using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelCoroutines : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		StartCoroutine(CoroutineTest("First: "));   
		StartCoroutine(CoroutineTest("Second: "));
	}

	IEnumerator CoroutineTest(string name)
	{
		while (true)
		{
			Debug.Log(name + Time.frameCount);

			yield return new WaitForSeconds(0.5f);
		}
	}
}
