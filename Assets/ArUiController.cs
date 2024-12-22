using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ArUiController : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    // [SerializeField] private Material arPlaneMaterial;

    [SerializeField] private ResultSolver resultSolver;

    private int state = 1;
    private static readonly int TexTintColor = Shader.PropertyToID("_TexTintColor");

    private void Start()
    {
        // arPlaneMaterial.SetColor(TexTintColor, new Color(1, 0, 0, 0.5f));
        Camera.main.GetComponent<CardImageEffect>().enabled = true;
    }

    public void Click()
    {
        if (state == 1)
        {
            Camera.main.GetComponent<CardImageEffect>().showEffect = false;
            Camera.main.GetComponent<CardImageEffect>().fillNextFrame = true;
            state++;
            return;
        }

        if (state == 2)
        {
            RaycastHit info;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out info))
            {
                var pickable = info.collider.GetComponentInParent<Pickable>();
                if (pickable != null)
                {
                    pickable.Pick();
                }
            }
        }

        if (state == 4)
        {
            resultSolver.CreateElementsPicker();
            state = 2;
        }
    }

    public void ClickStart()
    {
        resultSolver.solveItems = resultSolver.GetComponentInChildren<ElementsPicker>().items;
        resultSolver.InitSolve();

        canvas.SetActive(false);

        state = 3;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && state == 3)
        {
            canvas.SetActive(true);
            state = 4;
        }
    }
}