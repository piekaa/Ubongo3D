using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEststs : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        
    }

    private IEnumerator cor(int id)
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log(id);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}