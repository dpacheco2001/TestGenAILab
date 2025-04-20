// ViewerCard.cs
using System;
using UnityEngine;
using UnityEngine.UI;

public class ViewerCard : MonoBehaviour {
    Toggle _toggle;
    Image  _thumbImage;
    ResourceUIManager.ResourceData _data;
    Action<ResourceUIManager.ResourceData> _onSelected;

    void Awake() {
        _toggle     = GetComponent<Toggle>();
        // busca el Image dentro de Content/MiniaturaP/Image
        _thumbImage = transform
            .Find("Content/MiniaturaP/Image")
            .GetComponent<Image>();
    }

    public void Setup(
        ResourceUIManager.ResourceData data,
        Action<ResourceUIManager.ResourceData> onSelected
    ) {
        _data       = data;
        _onSelected = onSelected;
        _thumbImage.sprite = data.thumbnail;

        // resetear estado y suscripción
        _toggle.isOn = false;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(isOn => {
            if (isOn) _onSelected(_data);
        });
    }
}
