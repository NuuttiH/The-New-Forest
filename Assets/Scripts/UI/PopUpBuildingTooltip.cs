using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PopUpBuildingTooltip : PopUpMenu, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _resourcePanel;
    private GameObject _resourceObject;
    private PlaceableObject _placeableObject;
    private OpenTooltipOnHover _hoverScript;

    public override void InitAdvanced()
    {
        BuildButton buildButtonScript = _bossObject.GetComponent<BuildButton>();
        _placeableObject = buildButtonScript.Building.GetComponent<PlaceableObject>();
        Debug.Log($"PopUpGenericTooltip.InitAdvanced() found _placeableObject: {_placeableObject != null}");
        _hoverScript = _bossObject.GetComponent<OpenTooltipOnHover>();
        gameObject.GetComponent<Button>().onClick.AddListener( delegate{ buildButtonScript.TryBuild(); } ); 

        bool first = true;
        if(_placeableObject.BuildingCost.Length == 0)
        {
            _resourceObject = _resourcePanel.transform.GetChild(0).gameObject;
            _resourceObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
            _resourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "";
        }
        else foreach(Cost cost in _placeableObject.BuildingCost)
        {
            if(first)
            {
                first = false;

                _resourceObject = _resourcePanel.transform.GetChild(0).gameObject;
                SetImage(_resourceObject.transform.GetChild(0).gameObject.
                    GetComponent<Image>(), cost.type);
                _resourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().
                    text = cost.amount.ToString();
                Debug.Log($"PopUpGenericTooltip.InitAdvanced() set {cost.type} and {cost.amount}");
            }
            else
            {
                GameObject _newResourceObject = Instantiate(_resourceObject, this.gameObject.transform);
                SetImage(_newResourceObject.transform.GetChild(0).gameObject.
                    GetComponent<Image>(), cost.type);
                _newResourceObject.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().
                    text = cost.amount.ToString();
                Debug.Log($"PopUpGenericTooltip.InitAdvanced() set {cost.type} and {cost.amount}");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("PopUpGenericTooltip.OnPointerEnter()");
        _hoverScript.SetTooltipHoverState(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("PopUpGenericTooltip.OnPointerExit()");
        _hoverScript.SetTooltipHoverState(false);
    }
}
