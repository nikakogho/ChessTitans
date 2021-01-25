using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Chess.Config;
using Chess.UI.Responsive;

#pragma warning disable 0649

public class LoadManager : MonoBehaviour
{
    public BoardConfig[] boardConfigs;

    [SerializeField] GameObject[] _menuUIPrefabs;
    [SerializeField] GameObject[] _gameUIPrefabs;

    Dictionary<Vector2Int, MenuUI> menuUIs;
    Dictionary<Vector2Int, GameUI> gameUIs;

    public string menuSceneName = "menu";
    public string gameSceneName = "main";

    public string CurrentSceneName => SceneManager.GetActiveScene().name;
    public bool IsInMenu => CurrentSceneName == menuSceneName;
    public bool IsInMain => CurrentSceneName == gameSceneName;

    public static LoadManager instance;

    Transform menuUIsParent, gameUIsParent;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        menuUIs = new Dictionary<Vector2Int, MenuUI>();
        gameUIs = new Dictionary<Vector2Int, GameUI>();

        menuUIsParent = new GameObject("Menu UIs").transform;
        gameUIsParent = new GameObject("Game UIs").transform;

        foreach (var prefab in _menuUIPrefabs)
        {
            var ui = (Instantiate(prefab, menuUIsParent).GetComponent<MenuUI>());
            menuUIs.Add(ui.Size, ui);
        }

        foreach (var prefab in _gameUIPrefabs)
        {
            var ui = (Instantiate(prefab, gameUIsParent).GetComponent<GameUI>());
            gameUIs.Add(ui.Size, ui);
        }

        menuUIsParent.parent = gameUIsParent.parent = transform;
    }

    public MenuUI GetClosestMenuUI(Vector2Int resolution)
    {
        float best = float.MaxValue;
        MenuUI chosen = null;

        foreach (var pair in menuUIs)
        {
            var delta = pair.Key - resolution;
            float dist = delta.sqrMagnitude;

            if (dist < best)
            {
                best = dist;
                chosen = pair.Value;
            }
        }

        return chosen;
    }

    public GameUI GetClosestGameUI(Vector2Int resolution)
    {
        float best = float.MaxValue;
        GameUI chosen = null;

        foreach (var pair in gameUIs)
        {
            var delta = pair.Key - resolution;
            float dist = delta.sqrMagnitude;

            if (dist < best)
            {
                best = dist;
                chosen = pair.Value;
            }
        }

        //Debug.Log($"Chosen game UI is {chosen?.name}");
        return chosen;
    }

    public void DisableUI()
    {
        //Debug.Log("Started disabling UI");
        try
        {
            try
            {
                var resolution = new Vector2Int(Screen.width, Screen.height);

                GetClosestMenuUI(resolution).FX.SetActive(false);
                GetClosestGameUI(resolution).FX.SetActive(false);
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Null on main disables");
            }

            try
            {
                foreach (var ui in menuUIs.Values)
                {
                    try
                    {
                        ui.FX.SetActive(false);
                    }
                    catch (System.NullReferenceException)
                    {
                        Debug.Log("Null on some menu UI disable");
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Null on menu UI values");
            }
            try
            {
                foreach (var ui in gameUIs.Values)
                    try
                    {
                        ui.FX.SetActive(false);
                    }
                    catch (System.NullReferenceException)
                    {
                        Debug.Log("Null on some game UI disable");
                    }
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Null on game UI values");
            }
        }
        catch (System.NullReferenceException ex)
        {
            Debug.Log($"Null guy {ex}");
        }
        //Debug.Log("Done disabling UI");
    }
}
