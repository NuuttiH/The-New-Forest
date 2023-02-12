using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PanelType { None, Food, Lumber, Magic, GrowthModifier}

public class ResourcePanel : MonoBehaviour
{
    [SerializeField] private PanelType _panelType;
    [SerializeField] private TextMeshProUGUI _textField;
    [SerializeField] private GameObject _valueChangeTextPrefab;
    private int _oldResourceGain = -100;
    private int _resourceGain = 0;


    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshResource();
        switch(_panelType)
        {
            case PanelType.Food:
                Events.onFoodChange += RefreshResource;
                StartCoroutine(UpdateEstimate());
                break;
            case PanelType.Lumber:
                Events.onLumberChange += RefreshResource;
                StartCoroutine(UpdateEstimate());
                break;
            case PanelType.Magic:
                Events.onMagicChange += RefreshResource;
                StartCoroutine(UpdateEstimate());
                break; 
            case PanelType.GrowthModifier:
                Events.onGrowthModChange += RefreshResource;
                break; 
        }
    }

    void OnDestroy()
    {
        switch(_panelType)
        {
            case PanelType.Food:
                Events.onFoodChange -= RefreshResource;
                break;
            case PanelType.Lumber:
                Events.onLumberChange -= RefreshResource;
                break;
            case PanelType.Magic:
                Events.onMagicChange -= RefreshResource;
                break;
            case PanelType.GrowthModifier:
                Events.onGrowthModChange -= RefreshResource;
                break; 
        }
    }

    private void RefreshResource()
    {
        int value = -1;
        float fvalue = -1f;

        switch(_panelType)
        {
            case PanelType.Food:
                value = GameManager.GetResource(Resource.Food);
                break;
            case PanelType.Lumber:
                value = GameManager.GetResource(Resource.Lumber);
                break;
            case PanelType.Magic:
                value = GameManager.GetResource(Resource.Magic);
                break;
            case PanelType.GrowthModifier:
                fvalue = GameManager.GetGrowthValue();
                break; 
        }

        // Only show resource gain per minute for resources after the first minute 
        if(_panelType > PanelType.Magic)   
        {
            _textField.text = fvalue.ToString("000.0") + "%";
        }
        else if(_oldResourceGain == -100)   
        {
            _textField.text = $"{value}";
        }
        else
        {
            _textField.text = $"{value} ({_oldResourceGain}/min)";
        }
    }
    private void RefreshResource(int oldValue, int newValue)
    {
        int change = newValue - oldValue;

        // Only show resource gain per minute after the first minute 
        if(_oldResourceGain == -100) 
        {
            _textField.text = $"{newValue}";
        }
        else
        {
            _textField.text = $"{newValue} ({_oldResourceGain}/min)";
        }

        if(change > 0) _resourceGain += change;

        GameObject valueChangeText = Instantiate(
            _valueChangeTextPrefab, this.transform.position, 
            Quaternion.identity, this.transform);
        valueChangeText.GetComponent<ValuePopUp>().Init(change);
    }
    private void RefreshResource(float oldValue, float newValue)
    {
        float change = newValue - oldValue;
        _textField.text = newValue.ToString("000.0") + "%";

        GameObject valueChangeText = Instantiate(
            _valueChangeTextPrefab, this.transform.position, 
            Quaternion.identity, this.transform);
        valueChangeText.GetComponent<ValuePopUp>().Init(change);
    }

    IEnumerator UpdateEstimate()
    {
        while(true)
        {
            // Update once per minute
            yield return new WaitForSeconds(60f);

            _oldResourceGain = _resourceGain;
            _resourceGain = 0;
            RefreshResource();
        }
    }
}
