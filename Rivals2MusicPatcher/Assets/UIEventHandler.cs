using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEventHandler : MonoBehaviour
{
    public TMP_Text text;
    public Image BG;
    public Image border;
    
    public Color highlightColor;
    public Color normalColor;
    
    public Color highlightColorBG;
    public Color normalColorBG;

    public void SetHighlight()
    {
        text.color = highlightColor;
        BG.color = highlightColorBG;
        border.gameObject.SetActive(true);
    }
    
    public void SetNormal()
    {
        text.color = normalColor;
        BG.color = normalColorBG;
        border.gameObject.SetActive(false);
    }
}
