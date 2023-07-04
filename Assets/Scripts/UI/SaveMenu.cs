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

        SetUpSaveButton(SaveIdentifier.Auto);
        SetUpSaveButton(SaveIdentifier.First);
        SetUpSaveButton(SaveIdentifier.Second);
        SetUpSaveButton(SaveIdentifier.Third);
        
        _returnButton.onClick.AddListener( delegate{ Return(); } );
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }

    public void SetUpSaveButton(SaveIdentifier saveIdentifier, bool onlyUpdate = false)
    {
        Button button = GetSaveButton(saveIdentifier);
        if(saveIdentifier != SaveIdentifier.Auto && !onlyUpdate)
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
        SetUpSaveButton(saveIdentifier, true); // Update text
    }
    public Button GetSaveButton(SaveIdentifier saveIdentifier)
    {
        switch(saveIdentifier)
        {
            case SaveIdentifier.Auto:
                return _autoSaveButton;
            case SaveIdentifier.First:
                return _Save1Button;
            case SaveIdentifier.Second:
                return _Save2Button;
            case SaveIdentifier.Third:
                return _Save3Button;
        }
        return null;
    }
    public void Return()
    {
        IngameUIHandler.PopFromMenuStack(this.gameObject);
    }
}
