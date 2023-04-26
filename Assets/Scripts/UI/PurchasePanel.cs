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
    [SerializeField] private Flag _requiredFlag = Flag.None;

    private Button _button;
    private bool _canPay;
    private bool _canHouse;
    private bool _isDisabled;
    
    void Start()
    {
        if(_targetObjectInfo == null) return;

        _button = this.gameObject.GetComponent<Button>();
        _button.onClick.AddListener( HandlePurchase );

        _targetImage.sprite = _targetObjectInfo.sprite;
        _targetTMP.text = _targetObjectInfo.name;
        _costTMP.text = $"{Cost[0].amount}x lumber";
        
        Events.onResourceChange += CheckCost;
        CheckCost();
        _canHouse = true;
        _isDisabled = false;
        if(!GameManager.GetFlag(_requiredFlag)) 
            SetDisabled();
        else switch(_rewardType)
        {
            case RewardType.Resource:
                break;
            case RewardType.Villager:
                Events.onVillagerCountChange += CheckHousing;
                Events.onPopLimitChange += CheckHousing;
                CheckHousing();
                break;
            case RewardType.Flag:
                if(GameManager.GetFlag((Flag)_reward))
                {
                    // Hide if reward already acquired
                    SetDisabled();
                }
                break;
            case RewardType.TraderSpeed:
                break;
        }
    }

    void OnDestroy()
    {
        Events.onResourceChange -= CheckCost;
        switch(_rewardType)
        {
            case RewardType.Resource:
                break;
            case RewardType.Villager:
                Events.onVillagerCountChange -= CheckHousing;
                Events.onPopLimitChange -= CheckHousing;
                break;
            case RewardType.Flag:
                //
                break;
            case RewardType.TraderSpeed:
                break;
        }
    }

    public void SetDisabled()
    {
        _isDisabled = true;
        _button.interactable = false;
    }

    private void HandlePurchase()
    {
        if(_rewardType == RewardType.Villager && !CheckHousing2()) return;

        bool success = GameManager.TryPay(Cost);
        if(success)
        {
            switch(_rewardType)
            {
                case RewardType.Resource:
                    GameManager.AddResource(_rewardResourceType, (int)_reward);
                    break;
                case RewardType.Villager:
                    GameManager.CreateVillager(_rewardPrefab);
                    break;
                case RewardType.Flag:
                    GameManager.SetFlag((Flag)_reward);
                    break;
                case RewardType.TraderSpeed:
                    GameManager.AdjustTraderSpeed(_reward);
                    MessageLog.NewMessage(new MessageData(
                        $"Traders travelling speed has been increased!", 
                        MessageType.Upgrade));
                    break;
            }
            MissionManager.onIncrementMission(MissionGoal.BuyFromTrader, _targetObjectInfo.id);
        }
        else
        {
            Debug.Log($"HandlePurchase() for {_targetObjectInfo.name} failed");
        }
    }

    
    // Delegate version
    public void CheckCost(int a = 0, int b = 0)
    {
        if(_isDisabled) return;

        _canPay = Tools.CheckCost(Cost);
        _button.interactable = _canPay && _canHouse;
        //Debug.Log($"PurchasePanel.CheckCost(onResourceChange) for {_targetObjectInfo.Name}, can afford: {_button.interactable}");
    }
    public void CheckHousing()
    {
        if(_isDisabled) return;
        
        int villagerCount = GameManager.GetVillagerCount();
        int housingLimit = GameManager.GetPopulationLimit();
        _canHouse = housingLimit > villagerCount ? true : false;
        _button.interactable = _canPay && CheckHousing2();
    }
    public bool CheckHousing2()
    {
        int villagerCount = GameManager.GetVillagerCount();
        int housingLimit = GameManager.GetPopulationLimit();
        return housingLimit > villagerCount ? true : false;
    }
}
