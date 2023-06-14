using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadMenu : MonoBehaviour
{
    [SerializeField] private AudioEvent _buttonAudioEvent;
    [SerializeField] private Button _autoSaveButton;

    [SerializeField] private Button _Save1Button;
    [SerializeField] private Button _Save2Button;
    [SerializeField] private Button _Save3Button;

    [SerializeField] private Button _returnButton;


    void Start()
    {
        IngameUIHandler.PushToMenuStack(this.gameObject);
        
        SetUpLoadButton(_autoSaveButton, SaveIdentifier.Auto);
        SetUpLoadButton(_Save1Button, SaveIdentifier.First);
        SetUpLoadButton(_Save2Button, SaveIdentifier.Second);
        SetUpLoadButton(_Save3Button, SaveIdentifier.Third);
        
        _returnButton.onClick.AddListener( delegate{ Return(); } );
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }

    public void SetUpLoadButton(Button button, SaveIdentifier saveIdentifier)
    {
        button.onClick.AddListener( delegate{ Load(saveIdentifier); } );
        GameState state = SaveManager.GetData(saveIdentifier);
        if(!state.isSave) return;

        // Adjust the name of the save if save file exists
        ObjectInfo scenarioInfo = GameManager.GetObjectInfo(state.scenarioInfoId);

        string timeText = "";
        // seconds: state.time[0], minutes: state.time[1], hours: state.time[2]
        if(state.time[2] != 0) 
            timeText += state.time[2].ToString("00") + ":" + state.time[1].ToString("00") + ":";
        else timeText += state.time[1].ToString() + ":";
        timeText += state.time[0].ToString("00");
        
        TextMeshProUGUI tmp = button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        tmp.text = $"{scenarioInfo.name} - {timeText}";
    }
    public void Load(SaveIdentifier saveIdentifier)
    {
        Tools.PlayAudio(null, _buttonAudioEvent, true);
        SceneLoadingManager.LoadLevel("GameTest", saveIdentifier);
    }
    public void Return()
    {
        IngameUIHandler.PopFromMenuStack(this.gameObject);
    }
}
