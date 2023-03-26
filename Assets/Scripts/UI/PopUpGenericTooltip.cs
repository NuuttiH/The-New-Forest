using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpGenericTooltip : PopUpMenu
{
    [SerializeField] private GameObject _resourcePanel;
    private GameObject _resourceObject;
    private PurchasePanel _purchasePanel;

    public override void InitAdvanced()
    {
        _purchasePanel = _bossObject.GetComponent<PurchasePanel>();

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
                GameObject _newResourceObject = Instantiate(_resourceObject, this.gameObject.transform);
                SetImage(_newResourceObject.transform.GetChild(0).gameObject.
                    GetComponent<Image>(), cost.type);
                _newResourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().
                    text = cost.amount.ToString();
            }
        }
    }
}
