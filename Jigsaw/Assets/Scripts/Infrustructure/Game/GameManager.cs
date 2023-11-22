using System.Collections;
using System.Collections.Generic;
using Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneIndexes
{
    PERSISTANT_SCREEN = 0, 
    LEVEL_SELECT = 1,
    PUZZLE_GAME = 2,
     
    
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private GameObject eventSystem;

    private string currentImagePath;
    private float totalSceneProgress;
    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    public string CurrentImagePath => currentImagePath;

    public void SetCurrentImagePath(string path)
    {
        currentImagePath = path;
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(loadingScreen);
        DontDestroyOnLoad(eventSystem);
        LoadLevelSelectScreen();
        //SceneManager.LoadSceneAsync((int) SceneIndexes.PERSISTANT_SCREEN, LoadSceneMode.Additive);
    }


    public void LoadLevelSelectScreen()
    {
        loadingScreen.gameObject.SetActive(true);
        scenesLoading.Clear();
        scenesLoading.Add(SceneManager.LoadSceneAsync((int) SceneIndexes.LEVEL_SELECT, LoadSceneMode.Single));

        StartCoroutine(GetSceneLoadProgress());
    }

    public void LoadGameScreen()
    {
        loadingScreen.gameObject.SetActive(true);
        scenesLoading.Clear();
        scenesLoading.Add(SceneManager.LoadSceneAsync((int) SceneIndexes.PUZZLE_GAME, LoadSceneMode.Single));
        StartCoroutine(GetSceneLoadProgress());
    }



    public IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;

                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress = operation.progress;
                }

                totalSceneProgress = (totalSceneProgress / scenesLoading.Count) * 100;
                progressBar.SetFillAmount(totalSceneProgress/100.0f);
                yield return null;
            }
        }
        loadingScreen.SetActive(false);
    }
}

