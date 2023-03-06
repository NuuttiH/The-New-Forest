using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum Menu { None, Build, Escape, Trade }

public class IngameUIHandler : MonoBehaviour
{
    private static IngameUIHandler _instance;

    [SerializeField] private GameObject _menuButtonGroup;
    [SerializeField] private AudioEvent _toggleAudioEvent;
    [SerializeField] private Button _buildMenuButton;
    [SerializeField] private GameObject _buildMenu;
    [SerializeField] private Button _tradeMenuButton;
    [SerializeField] private TextMeshProUGUI _tradeMenuButtonText;
    [SerializeField] private GameObject _tradeMenu;
    [SerializeField] private Button _escapeButton;
    [SerializeField] private GameObject _escapeMenu;
    [SerializeField] private GameObject _screenCover;
    [SerializeField] private TextMeshProUGUI _timeText;
    private int _seconds = 0;
    private int _minutes = 0;
    private int _hours = 0;
    private int _secondsTrade = 70;
    private int _minutesTrade = 0;
    private bool _tradersAvailable = false;

    private Menu _currentMenu;
    private GameObject _instantiatedMenu;

    void Awake()
    {
        if(_instance == null) _instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        _currentMenu = Menu.None;

        _screenCover.SetActive(false);
        _screenCover.GetComponent<Button>().onClick.AddListener( delegate{ ScreenCoverClick(); } );
        _buildMenuButton.onClick.AddListener( delegate{ OpenMenu(Menu.Build); } );
        _tradeMenuButton.onClick.AddListener( delegate{ OpenMenu(Menu.Trade); } );
        _escapeButton.onClick.AddListener( delegate{ OpenMenu(Menu.Escape); } );

        StartCoroutine(TimeManagement());
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(_currentMenu == Menu.None)
            {
                OpenMenu(Menu.Escape);
            } 
            else
            {
                OpenMenu(Menu.None);
            } 
        }
    }

    IEnumerator TimeManagement()
    {
        while(_secondsTrade >= 60)
        {
            _minutesTrade++;
            _secondsTrade -= 60;
        }
        while(true)
        {
            yield return new WaitForSeconds(1f);

            _seconds++;
            _secondsTrade--;
            if(_seconds >= 60)
            {
                _minutes++;
                _seconds -= 60;

                if(_minutes >= 60)
                {
                    _hours++;
                    _minutes -= 60;
                }
            }
            if(_secondsTrade < 0)
            {
                _minutesTrade--;
                _secondsTrade += 60;

                if(_minutesTrade == -1)
                {
                    _minutesTrade = 0;
                    _tradersAvailable = !_tradersAvailable;
                    _tradeMenuButton.interactable = _tradersAvailable;

                    if(_tradersAvailable) _secondsTrade = 100;
                    else // Make trade menu unavailable
                    {
                        if(_currentMenu == Menu.Trade) OpenMenu();
                        _secondsTrade = 300;    // TODO proper data sourcing
                    } 

                    while(_secondsTrade >= 60)
                    {
                        _minutesTrade++;
                        _secondsTrade -= 60;
                    }
                }
            }
            _timeText.text  =   _hours.ToString("00") + ":" + 
                                _minutes.ToString("00") + ":" +
                                _seconds.ToString("00");
            if(_tradersAvailable)
            {
                _tradeMenuButtonText.text  =  "Trade\n(Available for "
                + _minutesTrade.ToString("0") + ":" + _secondsTrade.ToString("00") + ")";
            }
            else
            {
                _tradeMenuButtonText.text  =  "Trade\n( " + _minutesTrade.ToString("0") 
                + ":" + _secondsTrade.ToString("00") + " until available)";
            }
        }
    }

    public static void OpenMenu(Menu menu = Menu.None, bool playAudio = true)
    {
        Debug.Log("OpenMenu...");
        if(playAudio) Tools.PlayAudio(null, _instance._toggleAudioEvent);

        if(_instance._currentMenu == menu) 
            OpenMenu(Menu.None);
        else
        {
            GameObject prefab = null;

            switch(menu)
            {
                case Menu.None:
                    _instance._currentMenu = Menu.None;
                    _instance._menuButtonGroup.SetActive(true);
                    break;
                case Menu.Build:
                    _instance._currentMenu = Menu.Build;
                    prefab = _instance._buildMenu;
                    _instance._menuButtonGroup.SetActive(false);
                    break;
                case Menu.Trade:
                    _instance._currentMenu = Menu.Trade;
                    prefab = _instance._tradeMenu;
                    _instance._menuButtonGroup.SetActive(false);
                    break;
                case Menu.Escape:
                    _instance._currentMenu = Menu.Escape;
                    prefab = _instance._escapeMenu;
                    break;
            }

            // Close previously opened menu, open new or return to initial state
            if(_instance._instantiatedMenu != null) Destroy(_instance._instantiatedMenu);
            if(prefab != null)
            {
                _instance._instantiatedMenu = Instantiate(prefab);
                _instance._screenCover.SetActive(true);
            }
            else    
            {
                _instance._screenCover.SetActive(false);
            }
        }
    }
    public static void ScreenCoverClick()
    {
        if(BuildingSystem.IsPlacingBuilding()) return;

        OpenMenu(Menu.None, false);
    }
}
