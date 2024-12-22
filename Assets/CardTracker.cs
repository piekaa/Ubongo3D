using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CardTracker : MonoBehaviour
{
    private ARTrackedImageManager manager;
    [SerializeField] private ResultSolver resultSolver;

    public Texture2D testTex;

    public MutableRuntimeReferenceImageLibrary library;

    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<ARTrackedImageManager>();
        manager.trackedImagesChanged += OnChanged;
        library = manager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
        // var tex = new Texture2D(testTex.width, testTex.height);
        // tex.SetPixels(testTex.GetPixels());
        // tex.Apply();
        // library.ScheduleAddImageWithValidationJob(tex, "dupacycki", 0.075f);
        manager.referenceLibrary = library;
        manager.enabled = true;
    }

    public void AddImage(Texture2D tex, float width, Vector2 offset)
    {
        this.offset = new Vector3(offset.x, 0, offset.y);
        Debug.Log("Width in meters: " + width);
        library.ScheduleAddImageWithValidationJob(tex, "card", width);
    }


    void OnDisable() => manager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        ARTrackedImage img = null;
        foreach (var newImage in eventArgs.added)
        {
            img = newImage;
            // Handle added event
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            img = updatedImage;
        }

        if (img == null)
        {
            return;
        }

        var rotatedOffset = img.transform.rotation * offset;

        resultSolver.transform.position = img.transform.position + rotatedOffset;
        resultSolver.transform.rotation = img.transform.rotation;
    }
}