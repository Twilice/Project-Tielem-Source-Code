using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(RawImage))]
public class PortraitRenderer_UI : PortraitRenderer
{
    RenderTexture renderTex;
    public override void BindRenderTexture(Camera cam)
    {
        var rect = GetComponent<RectTransform>().rect;
        renderTex = RenderTexture.GetTemporary((int)rect.width, (int)rect.height);
        cam.targetTexture = renderTex;
        cam.forceIntoRenderTexture = true;
        GetComponent<RawImage>().texture = renderTex;
    }

    protected override void Cleanup()
    {
        base.Cleanup();
        if (renderTex != null)
            renderTex.Release();
    }
}
