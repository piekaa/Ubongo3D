using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CardImageEffect : MonoBehaviour
{
    [SerializeField] private Material calculateEffectMaterial;

    [SerializeField] private Material showEffectMaterial;

    [SerializeField] private Material closeEffectMaterial;

    [SerializeField] private RawImage rawImage;

    [SerializeField] private RawImage cropImage;

    [SerializeField] private ResultSolver solver;

    [SerializeField] private Slider closeCountSlider;

    [SerializeField] private CardTracker cardTracker;

    public bool showEffect = true;

    public bool ff = true;

    public bool fillNextFrame;

    private FillResult fillResult;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!showEffect && !fillNextFrame)
        {
            Graphics.Blit(src, dest);
            return;
        }

        if (fillNextFrame)
        {
            Debug.Log("FILL NEXT FRAME");

            var temp = RenderTexture.GetTemporary(src.width, src.height);
            Graphics.Blit(src, temp, calculateEffectMaterial);

            for (int i = 0; i < closeCountSlider.value; i++)
            {
                var temp2 = RenderTexture.GetTemporary(src.width, src.height);
                Graphics.Blit(temp, temp2, closeEffectMaterial);
                RenderTexture.ReleaseTemporary(temp);
                temp = temp2;
            }

            Graphics.Blit(src, dest);


            Vector3 screenPos;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                screenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            }
            else
            {
                screenPos = Input.mousePosition;
            }

            fillResult = null;
            Fill.fill(src, temp, screenPos, setResult, rawImage);
            fillNextFrame = false;

            RenderTexture.ReleaseTemporary(temp);
        }
        else
        {
            Graphics.Blit(src, dest, showEffectMaterial);
        }
    }

    public bool setResult(FillResult fillResult)
    {
        this.fillResult = fillResult;
        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("click");
            fillNextFrame = true;
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            solver.InitSolve();
        }

        if (fillResult != null)
        {
            Texture2D tex = new Texture2D(fillResult.width, fillResult.height);
            tex.SetPixels(fillResult.colors);
            tex.Apply();

            if (rawImage != null)
            {
                rawImage.texture = tex;
            }

            Texture2D croppedTex = new Texture2D(fillResult.croppedWidth, fillResult.croppedHeight);
            croppedTex.SetPixels(fillResult.originalImage.GetPixels(
                fillResult.cropX,
                fillResult.cropY,
                fillResult.croppedWidth,
                fillResult.croppedHeight));
            croppedTex.Apply();

            if (cropImage != null)
            {
                cropImage.texture = croppedTex;
            }

            if (cardTracker != null)
            {
                cardTracker.AddImage(croppedTex, fillResult.croppedWidthInMeters, fillResult.offsetInMeters);
            }
            
            solver.path = fillResult.path;

            fillResult = null;
        }
    }
}