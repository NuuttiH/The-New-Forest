using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedPanel : MonoBehaviour
{
    [SerializeField] private GameObject _speedButton0;
    [SerializeField] private GameObject _speedButton1;
    [SerializeField] private GameObject _speedButton2;
    [SerializeField] private GameObject _speedButton4;

    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _defaultSprite;

    private GameObject _lastButton;

    void Start()
    {
        _lastButton = _speedButton1;
        _speedButton0.GetComponent<Button>().onClick.AddListener( delegate{ ToggleButton(_speedButton0, 0f); } );
        _speedButton1.GetComponent<Button>().onClick.AddListener( delegate{ ToggleButton(_speedButton1, 1f); } );
        _speedButton2.GetComponent<Button>().onClick.AddListener( delegate{ ToggleButton(_speedButton2, 2f); } );
        _speedButton4.GetComponent<Button>().onClick.AddListener( delegate{ ToggleButton(_speedButton4, 4f); } );
    }

    public void ToggleButton(GameObject button, float speed)
    {
        _lastButton.GetComponent<Image>().sprite = _defaultSprite;
        button.GetComponent<Image>().sprite = _selectedSprite;
        _lastButton = button;
        GameManager.SetGameSpeed(speed);
        MessageLog.NewMessage(new MessageData($"Game speed set to {speed}x."));
    }
}
