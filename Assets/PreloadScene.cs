using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadScene : MonoBehaviour
{
    private string preloadScene = "Preload";
    private string gameScene = "Game";
    private bool isGameConnectedToWrapper = false;
	
    private AsyncOperation loadAsyncOp;

    public float initDelayToLoad = 2.0f;

    private void Start()
    {
        Debug.Log("--Preload Scene--");
        StartCoroutine(_StartLoadingGame());
        if (GameBridge.instance != null)
        {
            if (!GameBridge.instance.IsGameClosed)
            {
                GameBridge.ConnectToGameApi();
            }
        }
    }

    private IEnumerator _StartLoadingGame()
    {
        Application.backgroundLoadingPriority = ThreadPriority.High; 
		
        var timer = 0f;
		
        while(timer <= initDelayToLoad)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        loadAsyncOp = SceneManager.LoadSceneAsync(gameScene, LoadSceneMode.Additive);
        loadAsyncOp.allowSceneActivation = false;

        while(!loadAsyncOp.isDone)
        {
            if(loadAsyncOp.progress >= 0.9f)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                loadAsyncOp.allowSceneActivation = true;
#else
				if(isGameConnectedToWrapper)
				{				
					loadAsyncOp.allowSceneActivation = true;
				}
#endif
            }
            yield return null;
        }

        OnFinishLoading();
    }

    private void OnFinishLoading()
    {
        SceneManager.UnloadSceneAsync(preloadScene);
    }

    public void  OnAppAuthorized()
    {
        isGameConnectedToWrapper = true;
    }
}