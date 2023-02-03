using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFade : MonoBehaviour
{
    [SerializeField] private float _waitDuration = 2f;
    [SerializeField] private float _fadeDuration = 1f;
    private CanvasGroup _panel;

    void Start()
    {
        _panel = this.gameObject.GetComponent<CanvasGroup>();
		StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
	{
        yield return new WaitForSeconds(_waitDuration);

        float tick = _fadeDuration / 10f;
        while(_panel.alpha > 0.1f)
        {
            _panel.alpha -= tick;
            yield return new WaitForSeconds(tick);
        }
        Destroy(this.gameObject);
    }
}
