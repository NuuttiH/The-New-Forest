using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : MonoBehaviour
{
	private static SceneLoadingManager _instance;
	//[SerializeField] private GameObject _screenFadePrefab;

	private void Awake()
	{
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
		}
		DontDestroyOnLoad(this.gameObject);
		Events.ResetEventDelegates();
	}

	public static void LoadLevel(string levelName, SaveIdentifier saveIdentifier = SaveIdentifier.None)
	{
		_instance.StartCoroutine(AsyncSceneLoad(levelName, saveIdentifier));
	}
	private static IEnumerator AsyncSceneLoad(string levelName, SaveIdentifier saveIdentifier)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
		while(!operation.isDone)
		{
			yield return null;
		}
		Events.ResetEventDelegates();
		operation.allowSceneActivation = true;

		yield return null;

		//GameObject screenFadeObject = Instantiate(_instance._screenFadePrefab);

		if(saveIdentifier != SaveIdentifier.None)
		{
			SaveManager.LoadData(saveIdentifier);
		}
	}

	public static void TryToExit()
	{
		Debug.Log("TryToExit: Application.Quit()");
		Application.Quit();
	}
}
