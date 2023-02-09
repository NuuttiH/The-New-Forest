using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ResourcePanel : MonoBehaviour
{
    [SerializeField] private Resource _resourceType;
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
        StartCoroutine(UpdateEstimate());
        RefreshResource();
        switch(_resourceType)
        {
            case Resource.Food:
                Events.onFoodChange += RefreshResource;
                break;
            case Resource.Lumber:
                Events.onLumberChange += RefreshResource;
                break;
            case Resource.Magic:
                Events.onMagicChange += RefreshResource;
                break;
        }
    }

    void OnDestroy()
    {
        switch(_resourceType)
        {
            case Resource.Food:
                Events.onFoodChange -= RefreshResource;
                break;
            case Resource.Lumber:
                Events.onLumberChange -= RefreshResource;
                break;
            case Resource.Magic:
                Events.onMagicChange -= RefreshResource;
                break;
        }
    }

    private void RefreshResource()
    {
        int value = -1;

        switch(_resourceType)
        {
            case Resource.Food:
                value = GameManager.GetResource(Resource.Food);
                break;
            case Resource.Lumber:
                value = GameManager.GetResource(Resource.Lumber);
                break;
            case Resource.Magic:
                value = GameManager.GetResource(Resource.Magic);
                break;
        }

        if(_oldResourceGain == -100)   // Don't show resource gain per minute during first minute
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

        if(_oldResourceGain == -100)   // Don't show resource gain per minute during first minute
        {
            _textField.text = $"{newValue}";
        }
        else
        {
            _textField.text = $"{newValue} ({_oldResourceGain}/min)";
        }
        _resourceGain += change;

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
