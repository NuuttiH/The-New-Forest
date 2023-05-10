using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpBuilding : PopUpMenu
{
    [SerializeField] private Toggle _cutDownButton;
    [SerializeField] private TextMeshProUGUI _cutDownLabel;
    private PlaceableObject _buildingScript;

    public override void InitAdvanced()
    {
        _buildingScript = _bossObject.GetComponent<PlaceableObject>();
        _cutDownButton.isOn = _buildingScript.Cuttable;
        _cutDownButton.onValueChanged.AddListener( delegate{ 
            _buildingScript.MakeCuttable(_cutDownButton.isOn); 
        } );
        if(!_buildingScript.IsTree) _cutDownLabel.text = "Demolish";
    }
}
