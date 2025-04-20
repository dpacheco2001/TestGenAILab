// ViewportContent.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewportContent : MonoBehaviour
{
    public Image       image;
    public TMP_Text    header;
    public TMP_Text    subHeader;
    public TMP_Text    description;

    public void Setup(ResourceUIManager.ResourceData data)
    {
        image.sprite      = data.thumbnail;
        header.text       = data.header;
        subHeader.text    = data.subHeader;
        description.text  = data.description;
    }
}
