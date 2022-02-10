using System.Collections;
using System.Collections.Generic;
using CuttingRoom;
using UnityEngine;

public class CandidateBin : MonoBehaviour
{
    /// <summary>
    /// The path within the resources folder which contains the prefabs for the decision point associated with this bin.
    /// </summary>
    [SerializeField]
    private string resourcesPath = string.Empty;

    /// <summary>
    /// Media parent for candidates included in this bin.
    /// </summary>
    [SerializeField]
    private GameObject mediaParent = null;

    /// <summary>
    /// Instantiate the candidates from the bin.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> InstantiateCandidates()
    {
        // An array of gameobjects loaded from the resource path specified.
        GameObject[] candidatePrefabs = Resources.LoadAll<GameObject>(resourcesPath);

        // The candidates to be returned once they are instantiated.
        List<GameObject> candidates = new List<GameObject>();

        foreach (GameObject candidatePrefab in candidatePrefabs)
        {
            GameObject narrativeObjectGameObject = Instantiate(candidatePrefab, transform);

            // If the loaded object is not a narrative object, then disregard it.
            if (gameObject.GetComponent<NarrativeObject>() == null && gameObject.GetComponent<LayerDefinition>() == null)
            {
                Debug.LogWarning("Candidate bins can only contain game objects with a narrative object component or a layer definition component.");

                continue;
            }

            AtomicNarrativeObject atomicNarrativeObject = narrativeObjectGameObject.GetComponent<AtomicNarrativeObject>();

            // If it is an ANO, then a media parent needs assigned. This is normally done within the scene per ANO
            // but in this case it is not possible as the object is instantiated at runtime.
            if (atomicNarrativeObject != null)
            {
                atomicNarrativeObject.mediaParent = mediaParent.transform;
            }

            candidates.Add(narrativeObjectGameObject);
        }

        return candidates;
    }
}
