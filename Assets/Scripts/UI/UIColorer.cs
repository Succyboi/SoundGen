using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorer : MonoBehaviour
{
    public Color[] colors;
    public Selectable[] selectables;
    public Image[] images;
    public Text[] texts;

    private void Start()
    {
        int color = Random.Range(0, colors.Length);

        //selectables
        for (int s = 0; s < selectables.Length; s++)
        {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock = selectables[s].colors;
            colorBlock.highlightedColor = colors[color];
            colorBlock.selectedColor = colors[color];

            selectables[s].colors = colorBlock;
        }

        //images
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = colors[color];
        }

        //texts
        for (int t = 0; t < texts.Length; t++)
        {
            texts[t].color = colors[color];
        }
    }
}
