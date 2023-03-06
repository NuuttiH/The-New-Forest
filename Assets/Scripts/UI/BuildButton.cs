using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    public GameObject building;
    [SerializeField] private KeyCode _buildingHotkey;
    [SerializeField] private AudioEvent _audioEvent;
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

    void Update()
    {
        if(Input.GetKeyDown(_buildingHotkey))
        {
            TryBuild();
        }
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
        if(BuildingSystem.IsPlacingBuilding())
        {
            BuildingSystem.StopBuilding();
        } 
        else
        {
            BuildingSystem.BlockPlacement();
            BuildingSystem.InitializedWithObject(building);
            Tools.PlayAudio(null, _audioEvent);
        }
    }
}
