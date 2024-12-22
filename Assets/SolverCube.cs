using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolverCube : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "wall")
        {
            GetComponentInParent<SolveItem>().failedInit = true;
            GetComponentInParent<SolveItem>().failed = true;
        }

        if (other.tag == "cube")
        {
            GetComponentInParent<SolveItem>().failed = true;
        }
    }
}