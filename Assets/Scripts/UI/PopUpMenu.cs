using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpMenu : MonoBehaviour
{
    protected GameObject _bossObject;
    protected OpenPopUpOnClick _bossScript;
    protected ObjectInfo _objectInfo;
    [SerializeField] protected Image _image;
    [SerializeField] protected TextMeshProUGUI _textArea;
    [SerializeField] protected TextMeshProUGUI _extraText;
    [SerializeField] protected Button _exitButton;

    [SerializeField] private ObjectInfo _foodInfo;
    [SerializeField] private ObjectInfo _lumberInfo;
    [SerializeField] private ObjectInfo _magicInfo;
    
    
    public virtual void Init(GameObject bossObject, ObjectInfo objectInfo)
    {
        _bossObject = bossObject;
        _bossScript = _bossObject.GetComponent<OpenPopUpOnClick>();
        _objectInfo = objectInfo;

        // Move based on mouse position?

        _image.sprite = _objectInfo.sprite;
        _textArea.text = $"<size=120%>{_objectInfo.name}</size><br><br>{_objectInfo.description}";
        if(_extraText) _extraText.text = "";
        if(_exitButton) _exitButton.onClick.AddListener( delegate{ _bossScript.CloseOldPopUp(); } );

        InitAdvanced(); 
    }
    public virtual void InitAdvanced()
    {
        // Override
    }

    public void SetImage(Image image, Resource resource)
    {
        switch(resource)
        {
            case Resource.Food:
                image.sprite = _foodInfo.sprite;
                break;
            case Resource.Lumber:
                image.sprite = _lumberInfo.sprite;
                break;
            case Resource.Magic:
                image.sprite = _magicInfo.sprite;
                break;
            default:
                Debug.LogError($"ERROR: PopUpMenu.SetImage() can't handle '{resource}'");
                break;
        }
    }
}
