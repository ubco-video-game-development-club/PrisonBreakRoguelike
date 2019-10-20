using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDisplay : MonoBehaviour
{
    public RectTransform cooldownMask;

    public void SetProgress(float progress)
    {
        cooldownMask.anchorMax = new Vector2(1, progress);
    }
}
