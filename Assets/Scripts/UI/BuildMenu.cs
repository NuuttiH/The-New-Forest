using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenu : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    void Start()
    {
        BuildingSystem.ShowGrid(true);
        _closeButton.onClick.AddListener( delegate{ IngameUIHandler.OpenMenu(Menu.None); } );
    }

    void OnDestroy()
    {
        BuildingSystem.ShowGrid(false);
    }
}
