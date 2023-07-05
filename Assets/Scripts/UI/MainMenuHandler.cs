using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private AudioEvent _toggleAudioEvent;

    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _loadGameButton;
    [SerializeField] private GameObject _loadGameMenu;
    //[SerializeField] private Button _exitButton;

    void Awake()
    {
        _newGameButton.onClick.AddListener( delegate{ NewGame(); } );
        _loadGameButton.onClick.AddListener( delegate{ OpenLoadMenu(); } );
        //_exitButton.onClick.AddListener( delegate{ Debug.Log("Exit not implemented"); } );
    }

    public void NewGame()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        SceneLoadingManager.LoadLevel("GameTest");
    }
    public void OpenLoadMenu()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        Instantiate(_loadGameMenu);
    }
}
