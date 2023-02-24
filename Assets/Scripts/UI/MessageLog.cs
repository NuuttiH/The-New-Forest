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
    private RectTransform _logPanel;
    [SerializeField] private float _messageHeight = 40f;
    [SerializeField] private float _maxMessages = 8;
    private Vector2 _logOriginalPosition;
    private int _messageCount;

    void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this.gameObject);
			return;
        }
        _logPanel = gameObject.GetComponent<RectTransform>();
        _logOriginalPosition = _logPanel.position;
        Events.onIncrementMission += IncrementMission;
    }

    public static void IncrementMission(MissionGoal goal, int count)
    {
        MessageData messageData = new MessageData("IncrementMission", MessageType.Error);
        NewMessage(messageData);
        Debug.Log("IncrementMission");
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

        _instance.StartCoroutine(_instance.DeleteMessage(messageData, newMessage));
        _instance._messageCount++;
        UpdateSize();
    }

    IEnumerator DeleteMessage(MessageData messageData, GameObject obj)
    {
        yield return new WaitForSeconds(messageData.duration);
        if(obj != null)
        {
            Destroy(obj);
            _instance._messageCount--;
            UpdateSize();
        } 
    }

    public static void UpdateSize()
    {
        float newSize = _instance._messageHeight * (float)_instance._messageCount;
        while(_instance._messageCount > _instance._maxMessages)
        {
            // Delete oldest message
            Destroy(_instance.gameObject.transform.GetChild(0));
            _instance._messageCount--;
            newSize -= _instance._messageHeight;
        }
        _instance._logPanel.sizeDelta = new Vector2(_instance._logPanel.sizeDelta.x, newSize);
        _instance._logPanel.position = new Vector2(    
                                _instance._logOriginalPosition.x, 
                                _instance._logOriginalPosition.y + (0.5f * newSize));
    }
}
