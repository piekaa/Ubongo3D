using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementsPicker : MonoBehaviour
{
    public List<SolveItem> items = new List<SolveItem>();


    public void Pick(SolveItem item)
    {
        items.Add(item);
    }
}