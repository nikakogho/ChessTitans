using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Chess.UI;
using System.Linq;
using Chess.Config;
using Chess.Ads;
using Chess.UI.Responsive;

public class MenuManager : MonoBehaviour
{
    public string gameSceneName = "main";
    public Sprite chosenIcon, notChosenIcon;
    
    public bool playerIsWhite;

    public GameObject loadedBoardPrefab;
    public GameObject loadedBoardTilePrefab;
    
    public static MenuManager instance;

    public static Dictionary<string, SavedGameData> loadedGames;

    static BoardConfig BoardConfig => BoardConfigManager.instance.Chosen; // may do change this

    LoadManager loadManager;

    public MenuUI UI { get; private set; }
    
    void Awake()
    {
        //Debug.Log("Entered menu");

        instance = this;
        loadedGames = new Dictionary<string, SavedGameData>();
        BoardCoords.Setup();

        Invoke("InitStuff", 0.5f);
    }

    void Start()
    {
        loadManager = LoadManager.instance;
        ScreenManager.instance.onScreenSizeChanged += OnScreenSizeChanged;
    }

    void InitStuff()
    {
        GetLoadedBoards();
        OnPlayerSideChanged(true);
        GameManager.initialState.InitPossibleMoves();
    }

    public void OnPlayerSideChanged(bool white)
    {
        playerIsWhite = white;

        if (white)
        {
            UI.Data.whiteSideImage.sprite = chosenIcon;
            UI.Data.blackSideImage.sprite = notChosenIcon;
        }
        else
        {
            UI.Data.blackSideImage.sprite = chosenIcon;
            UI.Data.whiteSideImage.sprite = notChosenIcon;
        }
    }

    static void LoadScene(string sceneName)
    {
        AdManager.instance.RequestInterstitial(() => SceneManager.LoadScene(sceneName));
    }

    #region Play
    public void Play(int level)
    {
        loadManager.DisableUI();

        if (level == -1) // 2 Player Mode
        {
            GameManager.Init2PlayerGame(BoardConfig);
            LoadScene(gameSceneName);
        }
        else Play(level, playerIsWhite, gameSceneName);
    }

    public static void Play(int level, bool playerIsWhite, string gameSceneName)
    {
        var bot = BotSelector.Select(level);

        if (bot == null)
        {
            Debug.LogError($"No bot for level {level}, eh");
            return;
        }

        GameManager.Init1PlayerGame(BoardConfig, playerIsWhite, level);
        LoadScene(gameSceneName);
    }
    #endregion

    #region Loaded Boards
    void GetLoadedBoards()
    {
        loadedGames.Clear();

        for(int i = UI.Data.loadedBoardsParent.childCount - 1; i >= 0; i--)
            Destroy(UI.Data.loadedBoardsParent.GetChild(i).gameObject); // remove all children

        var savedGameNames = Config.GetSavedGameNames();

        int count = savedGameNames.Length;

        UI.Data.somethingToLoadUI.SetActive(count > 0);
        UI.Data.nothingToLoadUI.SetActive(count == 0);

        //Debug.Log($"Saved games count: {count}");

        for(int i = 0; i < count; i++)
        {
            string name = savedGameNames[i];
            //Debug.Log($"Saved game {i}: {name}");

            var game = Config.LoadGame(name);

            loadedGames[name] = game;
            DrawLoadedBoard(game);
        }
    }

    public void RemoveLoadedBoard(string name)
    {
        Config.DeleteGame(name);
        loadedGames.Remove(name);
        Config.SetSavedGameNames(loadedGames.Keys.ToArray());

        if(loadedGames.Count == 0)
        {
            UI.Data.somethingToLoadUI.SetActive(false);
            UI.Data.nothingToLoadUI.SetActive(true);
        }
    }

    void DrawLoadedBoard(SavedGameData game)
    {
        var obj = Instantiate(loadedBoardPrefab, UI.Data.loadedBoardsParent);
        var ui = obj.GetComponent<LoadedBoardUI>();

        ui.Init(game, this);
    }
    #endregion

    #region Button Clicks
    public void Play2Players()
    {
        Play(-1);
    }

    public void PlayOldLoad(string name)
    {
        loadManager.DisableUI();

        var game = loadedGames[name];

        GameManager.LoadGame(BoardConfig, game);
        RemoveLoadedBoard(name);

        LoadScene(gameSceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ResetStats()
    {
        PlayerPrefs.DeleteAll();
        if(LevelsManager.instance != null) LevelsManager.instance.Clear();
        GetLoadedBoards();
    }
    #endregion

    void OnScreenSizeChanged(Vector2Int oldSize, Vector2Int newSize)
    {
        var newUI = loadManager.GetClosestMenuUI(newSize);

        if (newUI == UI) return;
        
        if (UI != null)
        {
            // may do more
            UI.FX.SetActive(false);
        }
        newUI.OnActivated(UI?.Data);

        UI = newUI;
    }

    [System.Serializable]
    public class SizeUI
    {
        public Vector2Int size;
        public GameObject ui;
    }
}