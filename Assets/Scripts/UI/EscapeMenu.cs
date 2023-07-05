using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    [SerializeField] private AudioEvent _toggleAudioEvent;

    [SerializeField] private Button _continueButton;

    [SerializeField] private Button _saveGameButton;
    [SerializeField] private GameObject _saveGameMenu;

    [SerializeField] private Button _loadGameButton;
    [SerializeField] private GameObject _loadGameMenu;

    [SerializeField] private Button _optionsButton;
    [SerializeField] private GameObject _optionsMenu;

    [SerializeField] private Button _returnToMenuButton;
    [SerializeField] private Button _exitGameButton;


    void Start()
    {
        _continueButton.onClick.AddListener( delegate{ Continue(); } );

        _saveGameButton.onClick.AddListener( delegate{ OpenSaveMenu(); } );
        _loadGameButton.onClick.AddListener( delegate{ OpenLoadMenu(); } );
        _optionsButton.onClick.AddListener( delegate{ OpenOptionsMenu(); } );
        
        _returnToMenuButton.onClick.AddListener( delegate{ ReturnToMenu(); } );
        _exitGameButton.onClick.AddListener( delegate{ ExitGame(); } );

        GameManager.SetGameSpeed(0f);
    }
    void OnDestroy()
    {
        GameManager.SetGameSpeedToPrevious();
    }

    public void Continue()
    {
        IngameUIHandler.OpenMenu(Menu.None);
    }
    public void OpenSaveMenu()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        Instantiate(_saveGameMenu);
    }
    public void OpenLoadMenu()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        GameObject loadMenu = Instantiate(_loadGameMenu);
        loadMenu.GetComponent<LoadMenu>().InitInGame();
    }
    public void OpenOptionsMenu()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        Instantiate(_optionsMenu);
    }
    public void ReturnToMenu()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        SceneLoadingManager.LoadLevel("MainMenu");
    }
    public void ExitGame()
    {
        Tools.PlayAudio(null, _toggleAudioEvent);
        SceneLoadingManager.TryToExit();
    }
}
