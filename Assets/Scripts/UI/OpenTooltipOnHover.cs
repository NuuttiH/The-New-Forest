using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenTooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _UIPrefab;
    [SerializeField] private ObjectInfo _objectInfo;
    [SerializeField] private float _tooltipWaitTime = 1.5f;
    private PopUpMenu _popUpMenu;
    private float _lastChange;
    private bool _objectHoverState;
    private bool _tooltipHoverState;


    void Start()
    {
        _popUpMenu = null;
        _objectHoverState = false;
        _tooltipHoverState = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OpenTooltipOnHover.OnPointerEnter()");
        _objectHoverState = true;
        _lastChange = Time.time;
        if(_popUpMenu == null)
            StartCoroutine(TryOpenPopup(_lastChange));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OpenTooltipOnHover.OnPointerExit()");
        _objectHoverState = false;
        _lastChange = Time.time;

        if(_popUpMenu && !_tooltipHoverState)
        {
            StartCoroutine(CloseOldPopUpDelay());
        }
    }

    IEnumerator TryOpenPopup(float startupTime)
    {
        yield return new WaitForSecondsRealtime(_tooltipWaitTime);
        //Debug.Log("OpenTooltipOnHover.TryOpenPopup() Wait over");

        if(startupTime != _lastChange) yield break;

        GameObject obj = Instantiate(_UIPrefab);
        _popUpMenu = obj.GetComponent<PopUpMenu>();
        _popUpMenu.Init(this.gameObject, _objectInfo);

        Tools.AdjustPanelPlacementInCanvasToMousePos(
            obj.GetComponent<Canvas>(), obj.transform.GetChild(0).gameObject);
        //Debug.Log("OpenTooltipOnHover.TryOpenPopup() Finished");
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
    IEnumerator CloseOldPopUpDelay(float time = 0.1f)
    {
        yield return new WaitForSecondsRealtime(time);

        if(_popUpMenu && !_objectHoverState && !_tooltipHoverState)
        {
            Destroy(_popUpMenu.gameObject);
            _popUpMenu = null;
        }
    }

    public void SetTooltipHoverState(bool state)
    {
        _tooltipHoverState = state;

        if(!state && _popUpMenu && !_objectHoverState)
        {
            StartCoroutine(CloseOldPopUpDelay());
        }
    }
}
