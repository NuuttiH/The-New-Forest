using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    void Start()
    {
        _closeButton.onClick.AddListener( delegate{ IngameUIHandler.OpenMenu(Menu.None); } );
    }
}
