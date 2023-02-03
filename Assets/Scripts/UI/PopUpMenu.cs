using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpMenu : MonoBehaviour
{
    protected GameObject _bossObject;
    protected OpenPopUpOnClick _bossScript;
    [SerializeField] protected Button _exitButton;
    
    
    public void Init(GameObject bossObject)
    {
        _bossObject = bossObject;
        _bossScript = _bossObject.GetComponent<OpenPopUpOnClick>();

        // Move based on mouse position

        _exitButton.onClick.AddListener( delegate{ _bossScript.CloseOldPopUp(); } );

        InitAdvanced(); 
    }
    public virtual void InitAdvanced()
    {
        // Override
    }
}
