using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private TextMeshProUGUI _victoryText;
    [SerializeField] private AudioEvent _winAudioEvent;

    void Start()
    {
        _continueButton.onClick.AddListener( delegate{ Continue(); } );
        _menuButton.onClick.AddListener( delegate{ ReturnToMenu(); } );

        List<int> time = IngameUIHandler.GetTime();
        if(time[2] == 0) _victoryText.text = $"Your time was {time[1].ToString("00")}:{time[0].ToString("00")}"; 
        else _victoryText.text = $"Your time was {time[2]}:{time[1].ToString("00")}:{time[0].ToString("00")}"; 
        
        GameManager.SetGameSpeed(0f);
        Tools.PlayAudio(null, _winAudioEvent);
    }

    public void Continue()
    {
        GameManager.SetGameSpeedToPrevious();
        Destroy(this.gameObject);
    }

    public void ReturnToMenu()
    {
        SceneLoadingManager.LoadLevel("MainMenu");
    }
}
