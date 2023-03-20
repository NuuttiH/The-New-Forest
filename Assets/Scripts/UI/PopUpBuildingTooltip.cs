using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpBuildingTooltip : PopUpMenu
{
    [SerializeField] private ObjectInfo _foodInfo;
    [SerializeField] private ObjectInfo _lumberInfo;
    [SerializeField] private ObjectInfo _magicInfo;
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
    public void SetImage(Image image, Resource resource)
    {
        switch(resource)
        {
            case Resource.Food:
                image.sprite = _foodInfo.sprite;
                break;
            case Resource.Lumber:
                image.sprite = _lumberInfo.sprite;
                break;
            case Resource.Magic:
                image.sprite = _magicInfo.sprite;
                break;
            default:
                Debug.LogError($"ERROR: PopUpBuildingTooltip.SetImage() can't handle '{resource}'");
                break;
        }
    }
}
