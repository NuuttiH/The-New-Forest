using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum MessageType {Default, Error, Progress, Unimportant}

[System.Serializable]
public class MessageData
{
    public string content;
    public MessageType type;
    public float duration;

    public MessageData(string content, MessageType type = MessageType.Default, float duration = 60f)
    {
        this.content = content;
        this.type = type;
        this.duration = duration;
    }
}

public class MessageLog : MonoBehaviour
{
    private static MessageLog _instance;

    [SerializeField] private GameObject _messagePrefab;

    void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this.gameObject);
			return;
        }

        // Clean log
        int childCount = 0;
        foreach (Transform child in this.gameObject.transform) childCount++;
        for(int i=childCount-1; i>=0; i--)
        {
            Destroy(this.gameObject.transform.GetChild(i).gameObject);
        }
    }

    public static void NewMessage(MessageData messageData)
    {
        GameObject newMessage = Instantiate(_instance._messagePrefab, _instance.gameObject.transform);
        TextMeshProUGUI tmp = newMessage.GetComponent<TextMeshProUGUI>();
        tmp.text = messageData.content;

        switch(messageData.type)
        {
            case MessageType.Error:
                tmp.color = new Color32(255, 15, 15, 255);
                break;
            case MessageType.Progress:
                tmp.color = new Color32(15, 15, 100, 255);
                break;
            case MessageType.Unimportant:
                tmp.color = new Color32(220, 220, 220, 220);
                break;
            default:
                break;
        }

        _instance.StartCoroutine(_instance.HandleMessage(messageData, newMessage));
    }

    IEnumerator HandleMessage(MessageData messageData, GameObject obj)
    {
        float time = messageData.duration * 0.90f;
        yield return new WaitForSeconds(time);
        time = messageData.duration - time;

        // Fade text
        float tickCount = 25f;
        float tick = time / tickCount;
        TextMeshProUGUI tmPro = obj.GetComponent<TextMeshProUGUI>();
        Color color = tmPro.color;
        while(time > 0f)
        {
            color.a -= (1f / tickCount);
            tmPro.color = color;
            yield return new WaitForSeconds(tick);
            time -= tick;
        }
        if(obj != null)
        {
            Destroy(obj);
        } 
    }
}
