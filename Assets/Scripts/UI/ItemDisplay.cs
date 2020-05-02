using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public RectTransform cooldownMask;
    public Text counterText;

    public void UpdateDisplay(float progress, int count)
    {
        if (count == 0)
        {
            progress = 0;
        }
        
        cooldownMask.anchorMax = new Vector2(1, progress);
        counterText.text = "" + count;
    }
}
