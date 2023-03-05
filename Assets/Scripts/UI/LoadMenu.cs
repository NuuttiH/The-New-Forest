using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        // TODO Adjust button appearance and text
        _Save1Button.onClick.AddListener( delegate{ Load(SaveIdentifier.First); } );
        _Save2Button.onClick.AddListener( delegate{ Load(SaveIdentifier.Second); } );
        _Save3Button.onClick.AddListener( delegate{ Load(SaveIdentifier.Third); } );
        
        _returnButton.onClick.AddListener( delegate{ Return(); } );
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Return();
        }
    }

    public void Load(SaveIdentifier saveIdentifier)
    {
        Tools.PlayAudio(null, _buttonAudioEvent, true);
        SceneLoadingManager.LoadLevel("GameTest", saveIdentifier);
        // TODO Adjust button appearance and text
    }
    public void Return()
    {
        Destroy(this.gameObject);
    }
}
