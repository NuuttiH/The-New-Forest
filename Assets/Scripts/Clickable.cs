using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Clickable : MonoBehaviour
{
    public GameObject messagePrefab;
    public string message = "Your text here";
    public float duration = 5f;


    void OnMouseDown()
    {
        //Debug.Log("it works");
        Vector3 pos = transform.position + new Vector3(0, 7, 0);
        GameObject obj = Instantiate(messagePrefab, pos, Quaternion.identity, transform);
        obj.GetComponent<TextMeshPro>().text = message;
        Destroy(obj, duration);
    }
}
