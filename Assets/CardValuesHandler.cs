using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardValuesHandler : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [SerializeField] private List<Material> materials;

    private static readonly int Cut = Shader.PropertyToID("_Cut");

    private void Update()
    {
        materials.ForEach(m => m.SetFloat(Cut, slider.value));
    }
}