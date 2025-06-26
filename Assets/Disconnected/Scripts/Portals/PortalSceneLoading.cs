using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disconnected.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;
using Fragilem17.MirrorsAndPortals;
using UnityEngine.SceneManagement;

public class PortalSceneLoading : MonoBehaviour
{
    
    public SaveSystem saveSystem;

    [SerializeField] private bool _portalStartDisabled = true;
    [SerializeField] private List<string> _sceneNames = new List<string>();
    
    [SerializeField] private Button _prevSceneButton;
    [SerializeField] private Button _nextSceneButton;
    
    [SerializeField] private Fragilem17.MirrorsAndPortals.Portal _portal;
    [SerializeField] private PortalTransporter _portalTransporter;
    [SerializeField] private GameObject _portalMesh;
    [SerializeField] private GameObject _portalLoadingMesh;
    
    private bool isLoaded = false;
    private int _currentSceneIndex = -1;

    private void Start()
    {
        if (_portalStartDisabled) { DisablePortal(); }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        _currentSceneIndex++;
        if (_currentSceneIndex >= _sceneNames.Count) { _currentSceneIndex = 0; }
        
        LoadPortalScene();
    }
    
    private void LoadPrevScene()
    {
        _currentSceneIndex--;
        if (_currentSceneIndex < 0) { _currentSceneIndex = _sceneNames.Count - 1; }
        
        LoadPortalScene();
    }

    public void ClickNextScene()
    {
        UnloadPortalScene();
        
        StartCoroutine(ToggleSceneSwitch());
    }
    
    public void ClickPrevScene()
    {
        UnloadPortalScene();
        
        StartCoroutine(ToggleSceneSwitch(false));
    }

    IEnumerator ToggleSceneSwitch(bool loadNextScene = true)
    {
        yield return new WaitForSeconds(1);
        
        if (loadNextScene) { LoadNextScene(); } else { LoadPrevScene(); }
        
        Debug.Log("Coroutine finished.");
    }

    private void DisablePortal()
    {
        _nextSceneButton.interactable = false;
        _prevSceneButton.interactable = false;
        _portalMesh.SetActive(false);
        _portalLoadingMesh.SetActive(true);
        _portal.enabled = false;
        _portalTransporter.enabled = false;
    }

    private void EnablePortal()
    {
        _nextSceneButton.interactable = true;
        _prevSceneButton.interactable = true;
        _portalMesh.SetActive(true);
        _portalLoadingMesh.SetActive(false);
        _portal.enabled = true;
        _portalTransporter.enabled = true;
    }
    
    void LoadPortalScene()
    {
        isLoaded = true;
        StartCoroutine(LoadSceneCoroutine());
    }

    IEnumerator LoadSceneCoroutine()
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(_sceneNames[_currentSceneIndex], LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOp.isDone);
        
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneNames[_currentSceneIndex]));
        
        Task loadTask = saveSystem.LoadLevelAsync(_sceneNames[_currentSceneIndex]);
        while (!loadTask.IsCompleted)   // Wait for it to complete
            yield return null;
        
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
        
        EnablePortal();
    }

    void UnloadPortalScene()
    {
        isLoaded = false;
        SceneManager.UnloadSceneAsync(_sceneNames[_currentSceneIndex]);
        DisablePortal();
    }
    
}
