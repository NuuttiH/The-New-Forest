using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    public GameObject building;
    private PlaceableObject _buildingScript;
    private Button _button;

    void Awake()
    {
        _buildingScript = building.GetComponent<PlaceableObject>();
        _button = this.gameObject.GetComponent<Button>();
        GetComponent<Button>().onClick.AddListener( delegate{ TryBuild(); } ); 
        CheckCost();
        Events.onResourceChange += CheckCost;
    }

    void OnDestroy()
    {
        Events.onResourceChange -= CheckCost;
    }

    public void CheckCost(int a = 0, int b = 0)
    {
        _button.interactable = true;
        foreach(Cost cost in _buildingScript.BuildingCost)
        {
            if(GameManager.GetResource(cost.type) < cost.amount)
            {
                _button.interactable = false;
                break;
            }
        }
    }

    public void TryBuild()
    {
        BuildingSystem.BlockPlacement();
        BuildingSystem.InitializedWithObject(building);
    }
}
