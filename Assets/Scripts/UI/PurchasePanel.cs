using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum RewardType { None, Resource, Villager, Flag, TraderSpeed}

public class PurchasePanel : MonoBehaviour
{
    [SerializeField] private Image _targetImage;
    [SerializeField] private TextMeshProUGUI _targetTMP;
    [SerializeField] private Image _costImage;
    [SerializeField] private TextMeshProUGUI _costTMP;

    [SerializeField] private ObjectInfo _targetObjectInfo;
    public Cost[] Cost;
    [SerializeField] private RewardType _rewardType;
    [SerializeField] private Resource _rewardResourceType;
    [SerializeField] private float _reward;
    [SerializeField] private GameObject _rewardPrefab;
    [SerializeField] private string _rewardString;
    
    void Start()
    {
        if(_targetObjectInfo == null) return;

        this.gameObject.GetComponent<Button>().onClick.AddListener( HandlePurchase );
        _targetImage.sprite = _targetObjectInfo.sprite;
        _targetTMP.text = _targetObjectInfo.name;
        _costTMP.text = $"{Cost[0].amount}x lumber";
    }

    private void HandlePurchase()
    {
        bool success = GameManager.TryPay(Cost);

        if(success)
        {
            switch(_rewardType)
            {
                case RewardType.Resource:
                    GameManager.AddResource(_rewardResourceType, (int)_reward);
                    break;
                case RewardType.Villager:
                    Instantiate(_rewardPrefab, GameManager.Characters.transform);
                    break;
                case RewardType.Flag:
                    // TODO
                    break;
                case RewardType.TraderSpeed:
                    GameManager.AdjustTraderSpeed(_reward);
                    MessageLog.NewMessage(new MessageData(
                        $"Traders travelling speed has been increased!", 
                        MessageType.Upgrade));
                    break;
            }
        }
        else
        {
            Debug.Log($"HandlePurchase() for {_targetObjectInfo.name} failed");
        }
    }
}
