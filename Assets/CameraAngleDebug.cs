using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraAngleDebug : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            var rot = Camera.main.transform.localRotation.eulerAngles;

            text.text = rot.x.ToString("#.##") + ", " + rot.y.ToString("#.##") + ", " + rot.z.ToString("#.##");
        }
    }
}