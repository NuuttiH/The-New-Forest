using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveMenu : MonoBehaviour
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

        SetUpSaveButton(_autoSaveButton, SaveIdentifier.Auto);
        SetUpSaveButton(_Save1Button, SaveIdentifier.First);
        SetUpSaveButton(_Save2Button, SaveIdentifier.Second);
        SetUpSaveButton(_Save3Button, SaveIdentifier.Third);
        
        _returnButton.onClick.AddListener( delegate{ Return(); } );
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }

    public void SetUpSaveButton(Button button, SaveIdentifier saveIdentifier)
    {
        if(saveIdentifier != SaveIdentifier.Auto)
            button.onClick.AddListener( delegate{ Save(saveIdentifier); } );
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
    public void Save(SaveIdentifier saveIdentifier)
    {
        Tools.PlayAudio(null, _buttonAudioEvent);
        SaveManager.SaveData(saveIdentifier);
    }
    public void Return()
    {
        IngameUIHandler.PopFromMenuStack(this.gameObject);
    }
}
