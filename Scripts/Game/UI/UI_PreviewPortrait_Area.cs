using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_PreviewPortrait_Area : MonoBehaviour
{
    public PortraitRenderer portraitTemplate;
    public GameObject errorPortrait;
    public PortraitRenderer currentPortraitRenderer;

    private Dictionary<PortraitRenderer, bool> portraitRenderers;

    void Start()
    {
        GameCoordinator.instance.previewArea = this;
    }

    public void CreatePortraitRenderers(List<UI_WeaponShop_Item> items, Action onComplete)
    {
        portraitRenderers = new Dictionary<PortraitRenderer, bool>();

        foreach (var item in items)
        {
            var portrait = Instantiate(portraitTemplate, transform) as PortraitRenderer;
            portrait.name = $"PreviewPortrait_{item.previewSceneName}";
            portraitRenderers.Add(portrait, false);
            item.portraitRenderer = portrait;
            item.previewPortraitArea = this;
            portrait.Init(item.previewSceneName, (portraitRenderer) =>
            {
                portraitRenderers[portraitRenderer] = true;
                if (portraitRenderers.All((pr) => pr.Value))
                {
                    onComplete();
                }
            });
        }
    }

    public void SetPortraitRenderer(PortraitRenderer newPortraitRenderer)
    {
        if (currentPortraitRenderer != null)
            currentPortraitRenderer.Stop();
        currentPortraitRenderer = newPortraitRenderer;
        newPortraitRenderer.Play();
    }
}
