using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ArDebug : MonoBehaviour
{
    // Start is called before the first frame update

    private ARTrackedImageManager manager;

    void Start()
    {
        manager = GetComponent<ARTrackedImageManager>();
        manager.trackedImagesChanged += log;
    }

    void log(ARTrackedImagesChangedEventArgs args)
    {
        Debug.Log(args);
    }
}