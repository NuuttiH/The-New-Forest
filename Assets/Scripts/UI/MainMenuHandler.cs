using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Button _testButton;
    [SerializeField] private Button _exitButton;

    void Awake()
    {
        _testButton.onClick.AddListener( delegate{ SceneLoadingManager.LoadLevel("GameTest"); } );
        _exitButton.onClick.AddListener( delegate{ Debug.Log("Exit not implemented"); } );
    }

    void Start()
    {
        
    }
}
