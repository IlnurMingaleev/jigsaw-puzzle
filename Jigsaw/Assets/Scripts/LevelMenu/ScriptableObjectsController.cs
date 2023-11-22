using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptableObjectsController : MonoBehaviour
{
    [SerializeField] private ScriptableObject[] scriptableObjects;
    [SerializeField] private MapDisplay mapDisplay;
    private int currentIndex;
    

    private void Awake()
    {
        ChangrScriptableObject(0);
    }

    public void ChangrScriptableObject(int change)
    {
        currentIndex += change;
        if (currentIndex < 0) currentIndex = scriptableObjects.Length - 1;
        else if (currentIndex > scriptableObjects.Length - 1) currentIndex = 0;

        if (mapDisplay != null) mapDisplay.DisplayMap((Map)scriptableObjects[currentIndex]);
        if (mapDisplay != null) GameManager.Instance.SetCurrentImagePath(((Map)scriptableObjects[currentIndex]).imagePath);
    }

    public void PLay()
    {
        StartCoroutine(LoadYourAsyncScene());
    }
    
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scene2");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

}
