using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PopUpGenericTooltip : PopUpMenu, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _resourcePanel;
    private GameObject _resourceObject;
    private PurchasePanel _purchasePanel;
    private OpenTooltipOnHover _hoverScript;

    public override void InitAdvanced()
    {
        _purchasePanel = _bossObject.GetComponent<PurchasePanel>();
        _hoverScript = _bossObject.GetComponent<OpenTooltipOnHover>();

        bool first = true;
        foreach(Cost cost in _purchasePanel.Cost)
        {
            if(first)
            {
                first = false;

                _resourceObject = _resourcePanel.transform.GetChild(0).gameObject;
                SetImage(_resourceObject.transform.GetChild(0).gameObject.
                    GetComponent<Image>(), cost.type);
                _resourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().
                    text = cost.amount.ToString();
            }
            else
            {
                GameObject _newResourceObject = Instantiate(_resourceObject, this._resourcePanel.transform);
                SetImage(_newResourceObject.transform.GetChild(0).gameObject.
                    GetComponent<Image>(), cost.type);
                _newResourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().
                    text = cost.amount.ToString();
            }
        }
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
