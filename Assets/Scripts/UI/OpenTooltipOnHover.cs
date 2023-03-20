using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _UIPrefab;
    [SerializeField] private ObjectInfo _objectInfo;
    [SerializeField] private float _tooltipWaitTime = 2f;
    private PopUpMenu _popUpMenu;
    private float _lastChange;


    void Start()
    {
        _popUpMenu = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OpenTooltipOnHover.OnPointerEnter()");
        _lastChange = Time.time;
        StartCoroutine(TryOpenPopup(_lastChange));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OpenTooltipOnHover.OnPointerExit()");
        _lastChange = Time.time;

        if(_popUpMenu)
        {
            Destroy(_popUpMenu.gameObject);
            _popUpMenu = null;
        }
    }

    IEnumerator TryOpenPopup(float startupTime)
    {
        yield return new WaitForSecondsRealtime(_tooltipWaitTime);
        Debug.Log("OpenTooltipOnHover.TryOpenPopup() Wait over");

        if(startupTime != _lastChange) yield break;

        GameObject obj = Instantiate(_UIPrefab);
        _popUpMenu = obj.GetComponent<PopUpMenu>();
        _popUpMenu.Init(this.gameObject, _objectInfo);

        Tools.AdjustPanelPlacementInCanvasToMousePos(
            obj.GetComponent<Canvas>(), obj.transform.GetChild(0).gameObject);
        Debug.Log("OpenTooltipOnHover.TryOpenPopup() Finished");
    }

    void OnDestroy()
    {
        CloseOldPopUp();
    }

    public void CloseOldPopUp()
    {
        if(_popUpMenu != null)
        {
            Destroy(_popUpMenu.gameObject);
            _popUpMenu = null;
        }
    }
}
