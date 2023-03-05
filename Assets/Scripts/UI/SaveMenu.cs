using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        // TODO Adjust button appearance and text
        _Save1Button.onClick.AddListener( delegate{ Save(SaveIdentifier.First); } );
        _Save2Button.onClick.AddListener( delegate{ Save(SaveIdentifier.Second); } );
        _Save3Button.onClick.AddListener( delegate{ Save(SaveIdentifier.Third); } );
        
        _returnButton.onClick.AddListener( delegate{ Return(); } );
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }

    public void Save(SaveIdentifier saveIdentifier)
    {
        Tools.PlayAudio(null, _buttonAudioEvent);
        SaveManager.SaveData(saveIdentifier);
        // TODO Adjust button appearance and text
    }
    public void Return()
    {
        Destroy(this.gameObject);
    }
}
