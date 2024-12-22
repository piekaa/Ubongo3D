using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    [SerializeField] private SolveItem selfPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var solverCube in GetComponentsInChildren<SolverCube>())
        {
            solverCube.transform.localScale = Vector3.one;
        }
    }

    public void Pick()
    {
        GetComponentInParent<ElementsPicker>().Pick(selfPrefab);
        Destroy(gameObject);
    }
}