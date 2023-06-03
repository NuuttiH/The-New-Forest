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
    [SerializeField] private AudioEvent _traderArrival;
    [SerializeField] private AudioEvent _traderDeparture;
    [SerializeField] private int _baseTraderDuration = 180;
    private int _seconds, _minutes, _hours, _secondsTrade, _minutesTrade;
    private bool _tradersAvailable = false;
    private bool _tradingEnabled = false;

    private Menu _currentMenu;
    private GameObject _instantiatedMenu;
    private Stack<GameObject> _menuStack;
    private GameObject _activePopup;

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

        _tradeMenuButton.interactable = _tradersAvailable;
        _menuStack = new Stack<GameObject>();
    }
    void Start()
    {
        StartCoroutine(Init());
    }
    IEnumerator Init()
    {
        yield return null;
        Events.onTraderSpeedChange += AdjustTraderTimer;
        Events.onSaveLoaded += InitTraderFlags;
        Events.onFlagTriggered += CheckFlagTrigger;
    } 
    void OnDestroy()
    {
        Events.onTraderSpeedChange -= AdjustTraderTimer;
        Events.onSaveLoaded -= InitTraderFlags;
        Events.onFlagTriggered -= CheckFlagTrigger;
    }
    public static void InitTraderFlags()
    {
        if(GameManager.GetFlag(Flag.TradingTimerEnabled))
            _instance._tradingEnabled = true;
        else if(GameManager.GetFlag(Flag.TradeAvailable)) 
        {
            _instance._tradersAvailable = true;
            _instance._tradeMenuButton.interactable = true;
        }  
    }
    public static void CheckFlagTrigger(Flag flag)
    {
        Debug.Log($"IngameUIHandler.CheckFlagTrigger({flag})");
        switch(flag)
        {
            case Flag.TradeAvailable:
                _instance._tradersAvailable = true;
                _instance._tradeMenuButton.interactable = true;
                break;
            case Flag.TradingTimerEnabled:
                _instance._tradingEnabled = true;
                while(_instance._secondsTrade >= 60)
                {
                    _instance._minutesTrade++;
                    _instance._secondsTrade -= 60;
                }
                break;
        }
    }

    public static void InitTime(int seconds = 0, int minutes = 0, int hours = 0, int secondsTrade = 70, int minutesTrade = 0)
    {
        Debug.Log($"IngameUIHandler.InitTime({seconds}, {minutes}, {hours}, {secondsTrade}, {minutesTrade},)");

        _instance._seconds = seconds; 
        _instance._minutes = minutes; 
        _instance._hours = hours;
        _instance._secondsTrade = secondsTrade;
        _instance._minutesTrade = minutesTrade;

        _instance.StartCoroutine(_instance.TimeManagement());
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
        while(_tradingEnabled && _secondsTrade >= 60)
        {
            _minutesTrade++;
            _secondsTrade -= 60;
        }
        while(true)
        {
            yield return new WaitForSeconds(1f);

            _seconds++;
            if(_tradingEnabled) _secondsTrade--;
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
            if(_tradingEnabled && _secondsTrade < 0)
            {
                _minutesTrade--;
                _secondsTrade += 60;

                if(_minutesTrade == -1)
                {
                    // Handle trading status change
                    _minutesTrade = 0;
                    _tradersAvailable = !_tradersAvailable;
                    _tradeMenuButton.interactable = _tradersAvailable;

                    if(_tradersAvailable) _secondsTrade = _baseTraderDuration / 3;
                    else // Make trade menu unavailable
                    {
                        if(_currentMenu == Menu.Trade) OpenMenu();
                        _secondsTrade = (int)(_baseTraderDuration / GameManager.GetTraderSpeed());
                    } 

                    while(_secondsTrade >= 60)
                    {
                        _minutesTrade++;
                        _secondsTrade -= 60;
                    }

                    // Effects of trading caravan arriving/departing
                    if(_tradersAvailable)
                    {
                        Tools.PlayAudio(null, _traderArrival);
                        string extraString = "";
                        if(_instance._tradingEnabled)
                            extraString = $" ({_minutesTrade.ToString("0")}:{_secondsTrade.ToString("00")} until departure)";
                        MessageLog.NewMessage(new MessageData(
                            $"Trade caravan has arrived!{extraString}", MessageType.Progress));
                    }
                    else
                    {
                        Tools.PlayAudio(null, _traderDeparture);
                        string extraString = "";
                        if(_instance._tradingEnabled)
                            extraString = $" ({_minutesTrade.ToString("0")}:{_secondsTrade.ToString("00")} until next arrival)";
                        MessageLog.NewMessage(new MessageData(
                            $"Trade caravan has departed!{extraString}", MessageType.Progress));
                    }
                }
            }
            _timeText.text  =   _hours.ToString("00") + ":" + 
                                _minutes.ToString("00") + ":" +
                                _seconds.ToString("00");
            if(!_tradingEnabled)
            {
                _tradeMenuButtonText.text  =  "Trade";
            }
            else if(_tradersAvailable)
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
            if(_instance._instantiatedMenu != null)
            {
                while(_instance._menuStack.Count > 0)
                {
                    GameObject menuObject = _instance._menuStack.Pop();
                    Destroy(menuObject);
                }
                _instance._instantiatedMenu = null;
            }
            if(prefab != null)
            {
                _instance._instantiatedMenu = Instantiate(prefab);
                PushToMenuStack(_instance._instantiatedMenu);
            }
            else    
            {
                _instance._screenCover.SetActive(false);
            }
        }
    }
    public static void PushToMenuStack(GameObject menuObject)
    {
        GameObject lastMenu = null;
        if(_instance._menuStack.Count > 0) lastMenu = _instance._menuStack.Peek();
        if(lastMenu != null) lastMenu.SetActive(false);
        Debug.Log($"IngameUIHandler.PushToMenuStack() lastMenu == null: {lastMenu==null}");

        _instance._menuStack.Push(menuObject);
        _instance._screenCover.SetActive(true);
    }
    public static void PopFromMenuStack(GameObject menuObject = null)
    {
        GameObject lastMenu = null;
        if(_instance._menuStack.Count > 0) lastMenu = _instance._menuStack.Pop();
        if(lastMenu == null)
        {
            Debug.Log($"IngameUIHandler.PopFromMenuStack() error: lastMenu == null");
            return;
        } 
        else if(menuObject != null && menuObject != lastMenu) 
        {
            Debug.Log($"IngameUIHandler.PopFromMenuStack() error: menuObject != lastMenu");
            return;
        }
        
        Destroy(lastMenu);
        if(_instance._menuStack.Count == 0) _instance._screenCover.SetActive(true);
        else 
        {
            lastMenu = _instance._menuStack.Peek();
            lastMenu.SetActive(true);
        }
    }
    public static void ScreenCoverClick()
    {
        if(BuildingSystem.IsPlacingBuilding()) return;

        OpenMenu(Menu.None, false);
    }

    public void AdjustTraderTimer(float oldValue, float newValue)
    {
        Debug.Log($"IngameUIHandler.AdjustTraderTimer({oldValue}, {newValue})");
        int secondAdjustment = (int)(300f * oldValue - 300f * newValue);

        _secondsTrade -= secondAdjustment;
        while(_secondsTrade <= 0)
        {
            _minutesTrade--;
            _secondsTrade += 60;
        }
    }

    public static List<int> GetTime()
    {
        List<int> time = new List<int>();
        time.Add(_instance._seconds);
        time.Add(_instance._minutes);
        time.Add(_instance._hours);
        time.Add(_instance._secondsTrade);
        time.Add(_instance._minutesTrade);
        return time;
    }

    public static void SetActivePopUp(GameObject popUp)
    {
        if(_instance._activePopup != null) Destroy(_instance._activePopup);
        _instance._activePopup = popUp;
    }
}
