using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortraitRenderer_World : PortraitRenderer
{
    public int texWidth = 512;
    public int texHeight = 512;
    RenderTexture renderTex;
    public override void BindRenderTexture(Camera cam)
    {
        renderTex = RenderTexture.GetTemporary(texWidth, texHeight);
        cam.targetTexture = renderTex;
        cam.forceIntoRenderTexture = true;
        var sprite = GetComponent<Sprite>();
        GetComponent<Renderer>().material.mainTexture = renderTex;
    }

    protected override void Cleanup()
    {
        base.Cleanup();
        if (renderTex != null)
            renderTex.Release();
    }
}
