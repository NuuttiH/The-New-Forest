using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ValuePopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmp;
    private Vector3 _initialPositionAdjustment = new Vector3(-5f, -30f, 0f);
    private float _positionAdjustmentSpeed = 15f;

    public void Init(int value)
    {
        gameObject.transform.position += _initialPositionAdjustment;
        if(value >= 0)
        {
            _tmp.color = new Color32(15, 255, 15, 255);
            _tmp.text = $"+{value}";
        }
        else
        {
            _tmp.color = new Color32(255, 15, 15, 255);
            _tmp.text = $"{value}";
        }
        StartCoroutine(End());
    }
    public void Init(float value)
    {
        gameObject.transform.position += _initialPositionAdjustment;
        if(value >= 0f)
        {
            _tmp.color = new Color32(15, 255, 15, 255);
            _tmp.text = $"+{value}%";
        }
        else
        {
            _tmp.color = new Color32(255, 15, 15, 255);
            _tmp.text = $"{value}%";
        }
        StartCoroutine(End());
    }

    void Update()
    {
        Vector3 newPosition = gameObject.transform.position;
        newPosition.y += _positionAdjustmentSpeed * Time.deltaTime;
        gameObject.transform.position = newPosition;
    }

    IEnumerator End() 
    {
        yield return new WaitForSeconds(4.0f);
        Destroy(gameObject);
	}
}
