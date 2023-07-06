using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PopUpSmallTooltip : PopUpMenu, IPointerEnterHandler, IPointerExitHandler
{
    private OpenTooltipOnHover _hoverScript;
    
    public override void Init(GameObject bossObject, ObjectInfo objectInfo)
    {
        _bossObject = bossObject;
        _hoverScript = _bossObject.GetComponent<OpenTooltipOnHover>();
        _objectInfo = objectInfo;

        // Move based on mouse position?

        _image.sprite = _objectInfo.sprite;
        _titleText.text = $"{_objectInfo.name}";
        _textArea.text = $"{_objectInfo.description}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("PopUpGenericTooltip.OnPointerEnter()");
        _hoverScript.SetTooltipHoverState(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("PopUpGenericTooltip.OnPointerExit()");
        _hoverScript.SetTooltipHoverState(false);
    }
}
