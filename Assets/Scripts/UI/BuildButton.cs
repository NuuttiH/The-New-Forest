using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildButton : MonoBehaviour
{
    public GameObject Building;
    [SerializeField] private KeyCode _buildingHotkey;
    [SerializeField] private AudioEvent _audioEvent;
    [SerializeField] private Flag _requiredFlag = Flag.None;
    private PlaceableObject _buildingScript;
    private Button _button;

    void Start()
    {
        _buildingScript = Building.GetComponent<PlaceableObject>();
        _button = this.gameObject.GetComponent<Button>();
        _button.onClick.AddListener( delegate{ TryBuild(); } ); 
        CheckCost();

        Events.onResourceChange += CheckCost;

        if(!GameManager.GetFlag(_requiredFlag)) 
            Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        Events.onResourceChange -= CheckCost;
    }

    void Update()
    {
        if(Input.GetKeyDown(_buildingHotkey))
        {
            if(CheckCost())
            {
                TryBuild();
            }
            else
            {
                string resourceString = ", insufficient resources: ";
                bool first = true;
                foreach(Cost cost in _buildingScript.BuildingCost)
                {
                    if(GameManager.GetResource(cost.type) < cost.amount)
                    {
                        if(!first) resourceString += ", ";
                        else first = false;

                        resourceString += $"{cost.type} ({GameManager.GetResource(cost.type)}/{cost.amount})";
                    }
                }
                MessageLog.NewMessage(new MessageData(
                    $"Can't build '{_buildingScript.Name}'{resourceString}", MessageType.Error));
            }
        }
    }


    // Delegate version
    public void CheckCost(int a = 0, int b = 0)
    {
        _button.interactable = Tools.CheckCost(_buildingScript.BuildingCost);
        //Debug.Log($"BuildButton.CheckCost(onResourceChange) for {_buildingScript.Name}, can afford: {_button.interactable}");
    }
    public bool CheckCost()
    {
        _button.interactable = Tools.CheckCost(_buildingScript.BuildingCost);
        //Debug.Log($"BuildButton.CheckCost for {_buildingScript.Name}, can afford: {_button.interactable}");
        return _button.interactable;
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
            BuildingSystem.InitializedWithObject(Building);
            Tools.PlayAudio(null, _audioEvent);
        }
    }
}
