using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpBuildingTooltip : PopUpMenu
{
    [SerializeField] private GameObject _resourcePanel;
    private GameObject _resourceObject;
    private PlaceableObject _placeableObject;

    public override void InitAdvanced()
    {
        _placeableObject = _bossObject.GetComponent<BuildButton>().Building.GetComponent<PlaceableObject>();

        bool first = true;
        foreach(Cost cost in _placeableObject.BuildingCost)
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
