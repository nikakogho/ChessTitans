using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Chess.Ads;
using Chess.AI;
using Chess.Config;
using Chess.UI.Responsive;

public class GameManager : MonoBehaviour
{
    static Bot bot;
    static bool playerIsWhite;
    static int level;

    public static bool IsPlayingAgainstBot => bot != null;
    public bool BotIsThinking { get; private set; }

    public LayerMask tilesLayer;

    public Camera cam3D, cam2D;

    public Camera CurrentCamera => IsDisplaying3D ? cam3D : cam2D;
    public bool IsDisplaying3D => cam3D.gameObject.activeSelf;

    Stack<BoardState> states;
    Stack<string> moves;

    public static bool IsWhitesTurn => CurrentBoard.whiteTurnNext;
    public static bool IsPlayerTurn => !IsPlayingAgainstBot || (IsWhitesTurn == playerIsWhite);

    public string menuSceneName;

    public static BoardState CurrentBoard { get; private set; }

    static readonly Figure[,] initialTiles =
    {
        { Figure.CastleWhite, Figure.HorseWhite, Figure.BishopWhite, Figure.QueenWhite, Figure.KingWhite, Figure.BishopWhite, Figure.HorseWhite, Figure.CastleWhite },
        { Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite, Figure.PawnWhite },
        { Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty },
        { Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty },
        { Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty },
        { Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty, Figure.Empty },
        { Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack, Figure.PawnBlack },
        { Figure.CastleBlack, Figure.HorseBlack, Figure.BishopBlack, Figure.QueenBlack, Figure.KingBlack, Figure.BishopBlack, Figure.HorseBlack, Figure.CastleBlack }
    };
    public static readonly BoardState initialState = new BoardState(initialTiles);

    public float stopBotAfter = 6;

    public static GameManager instance;
    BoardVisuals visuals;

    IEnumerator botRoutine;
    IEnumerator stopBotRoutine;

    TileVisual selectedFrom;

    int moveNumber = 0;

    static int whiteScore, blackScore;

    public GameUI UI { get; private set; }
    public GameUI.UIData Data => UI.Data;

    public Color drawColor = Color.gray;

    public Transform boardParent;

    [Header("Audio")]
    public Sprite audioEnabledSprite;
    public Sprite audioDisabledSprite;
    public AudioSource source3D;
    public AudioSource source2D;
    public AudioClip winSound, loseSound, killSound, checkSound, drawSound, moveSound, invalidMoveSound;
    public AudioClip pawnAscensionSound, selectSound, unselectSound, castlingSound;

    public AudioSource CurrentAudioSource => IsDisplaying3D ? source3D : source2D;
    bool audioAllowed;

    public bool IsGameOver => CurrentBoard.IsOver;

    public static BoardConfig CurrentBoardConfig { get; private set; }

    #region Loaded Game
    static SavedGameData savedGameData;
    #endregion

    public static bool IsDisplayingWhiteSide => (IsPlayingAgainstBot && playerIsWhite) || (!IsPlayingAgainstBot && IsWhitesTurn);
    
    void SnapCameras()
    {
        //if (!IsPlayerTurn) return;

        bool snapToWhite = (IsPlayingAgainstBot && playerIsWhite) || (!IsPlayingAgainstBot && IsWhitesTurn);
        var snap3DTo = snapToWhite ? Data.whitesCameraSpot : Data.blacksCameraSpot;
        
        cam3D.transform.position = snap3DTo.position;
        cam3D.transform.rotation = snap3DTo.rotation;

        Vector3 cam2DEuler = new Vector3(90, 0, snapToWhite ? 90 : -90);
        cam2D.transform.rotation = Quaternion.Euler(cam2DEuler);
    }

    #region Init Static
    public static void Init1PlayerGame(BoardConfig boardConfig, bool playerIsWhite, int level)
    {
        CurrentBoardConfig = boardConfig;
        bot = BotSelector.Select(level);
        GameManager.playerIsWhite = playerIsWhite;
        GameManager.level = level;
        whiteScore = blackScore = 0;
    }
    
    public static void Init2PlayerGame(BoardConfig boardConfig)
    {
        CurrentBoardConfig = boardConfig;
        level = -1;
        playerIsWhite = true;
        whiteScore = blackScore = 0;
        bot = null;
    }

    public static void LoadGame(BoardConfig boardConfig, SavedGameData data)
    {
        CurrentBoardConfig = boardConfig;
        savedGameData = data;
        whiteScore = blackScore = 0;
        level = data.botLevel;
        playerIsWhite = data.playerIsWhite;

        CurrentBoard = data.boards.Last();
        CurrentBoard.InitPossibleMoves();

        if(data.botLevel == -1) // 2 Player Game
        {
            // may do something
            bot = null;
        }
        else // 1 Player Game
        {
            bot = BotSelector.Select(level);
        }
    }
    #endregion

    #region Init In Scene
    void Init()
    {
        instance = this;
        audioAllowed = true;
        BoardCoords.Setup();

        if(selectedFrom != null)
        {
            selectedFrom = null; // may do more
        }

        Instantiate(CurrentBoardConfig.boardPrefab, boardParent);

        Invoke("UpdateScoresUI", 0.5f);
    }

    void InitNewBoard()
    {
        states = new Stack<BoardState>();
        moves = new Stack<string>();
        states.Push(CurrentBoard = initialState);
        visuals.Init(this);
    }

    void InitSavedBoard()
    {
        states = new Stack<BoardState>();
        moves = new Stack<string>();

        states.Push(initialState);

        foreach(var state in savedGameData.boards)
        {
            states.Push(state);
            moves.Push(state.lastMove);
        }

        visuals.Init(this);

        savedGameData.Dispose();
        savedGameData = null;
    }
    #endregion

    void Awake()
    {
        Init();
    }

    void Start()
    {
        visuals = BoardVisuals.instance;

        if (savedGameData == null) InitNewBoard();
        else InitSavedBoard();
        
        ScreenManager.instance.onScreenSizeChanged += OnScreenFlipped;

        Invoke("CheckIfBotStarts", 0.5f);

        SelectDisplayDimension(false);
        SelectDisplayDimension(true);
    }

    void CheckRayCast(Vector3 point)
    {
        Ray raycast = CurrentCamera.ScreenPointToRay(point);
        if (Physics.Raycast(raycast, out RaycastHit hit, 1000, tilesLayer))
        {
            //Debug.LogError("Hit it!");
            var tile = hit.transform.GetComponent<TileVisual>();

            tile.OnClick();
            OnTileClicked(tile);
        }
    }

    void Update()
    {
        if (!IsPlayerTurn) return;
        if (IsGameOver) return;
        
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //Debug.LogWarning("Checking touch");
            CheckRayCast(Input.GetTouch(0).position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            //Debug.LogWarning("Checking click");
            CheckRayCast(Input.mousePosition);
        }
    }

    bool IsCurrentTurnFigure(Figure figure)
    {
        return (figure != Figure.Empty) && ((figure >= Figure.PawnBlack) == (CurrentBoard.blackTurnNext));
    }

    IEnumerator BotMoveRoutine()
    {
        BotIsThinking = true;
        yield return new WaitForSeconds(0.5f);
        var nextState = Chess.AI.SampleMoveChoosers.GetMoveByJustEvaluatingBoard(CurrentBoard, IsWhitesTurn, bot.Evaluator);
        //Debug.Log("Chosen move was " + nextMove);
        MakeMove(nextState);
        StopCoroutine(stopBotRoutine);
        yield return new WaitForSeconds(0.5f);
        BotIsThinking = false;
        botRoutine = null;
    }

    void MakeBotMove()
    {
        botRoutine = BotMoveRoutine();
        stopBotRoutine = StopBotSoonRoutine();

        StartCoroutine(botRoutine);
        StartCoroutine(stopBotRoutine);
    }

    IEnumerator StopBotSoonRoutine()
    {
        int oldMoves = moveNumber;
        yield return new WaitForSeconds(stopBotAfter);
        if (oldMoves == moveNumber)
        {
            Debug.Log("Stopping bot cause it's taking too long");
            StopCoroutine(botRoutine);
        }
    }

    void CheckIfBotStarts()
    {
        if (bot == null) return;

        if (IsPlayerTurn) BotIsThinking = false;
        else MakeBotMove();
    }

    void PushBoard(BoardState board)
    {
        CurrentBoard = board;
        states.Push(board);
        if(board.lastMove != null) moves.Push(board.lastMove);
        RenderBoard(board);
    }

    void RenderBoard(BoardState board)
    {
        board.InitPossibleMoves();
        visuals.DrawBoard(board);

        SnapCameras();
    }

    public void ResetBoard()
    {
        if (!IsPlayerTurn && !IsGameOver) return;
        if (moves.Count == 0) return;

        AdManager.instance.RequestInterstitial(() =>
        {
            UnSelect();

            Data.gameOverUI.SetActive(false);

            states.Clear();
            moves.Clear();

            CurrentBoard = initialState;
            PushBoard(initialState);
            RenderBoard(initialState);

            Invoke("CheckIfBotStarts", 0.5f);
        });
    }

    public void Undo(int amount)
    {
        if (amount >= moves.Count) ResetBoard();
        else
        {
            for(int i = 0; i < amount; i++)
            {
                states.Pop();
                moves.Pop();
            }
            
            RenderBoard(CurrentBoard = states.Peek());
        }
    }

    public void Undo()
    {
        if (!IsPlayerTurn) return; // only undo on player turns

        UnSelect();
        Undo(2);
    }

    public void GoToMenu()
    {
        LoadManager.instance.DisableUI();
        SceneManager.LoadScene(menuSceneName);
    }

    void PlaySound(AudioClip sound)
    {
        if (!audioAllowed) return;

        CurrentAudioSource.clip = sound;
        CurrentAudioSource.Play();
    }

    void GameOver()
    {
        bool won = !CurrentBoard.IsDraw && (playerIsWhite == (CurrentBoard.lastMoveByWhite));

        if (CurrentBoard.IsDraw)
        {
            Data.gameOverText.text = "Draw!";
            Data.gameOverText.color = drawColor;
            PlaySound(drawSound);
        }
        else if (CurrentBoard.lastMoveByWhite)
        {
            Data.gameOverText.text = "White Won!";
            Data.gameOverText.color = Color.white;
            whiteScore++;
            PlaySound(playerIsWhite ? winSound : loseSound);
        }
        else
        {
            Data.gameOverText.text = "Black Won!";
            Data.gameOverText.color = Color.black;
            blackScore++;
            PlaySound(playerIsWhite ? loseSound : winSound);
        }

        if (won)
        {
            int levelSoFar = PlayerPrefs.GetInt("Level", 1);

            if (level == levelSoFar)
            {
                PlayerPrefs.SetInt("Level", level + 1); // unlock next level
                Data.nextLevelUI.SetActive(true);
            }
        }

        UpdateScoresUI();
        Data.gameOverUI.SetActive(true);

        UnSelect();
    }

    void UpdateScoresUI()
    {
        Data.whiteScoreText.text = whiteScore.ToString();
        Data.blackScoreText.text = blackScore.ToString();
    }

    void OnTileClicked(TileVisual tile)
    {
        if (tile == selectedFrom) UnSelect(true);
        else if (selectedFrom == null) Select(tile);
        else CheckForMove(tile);
    }

    void CheckForMove(TileVisual tile)
    {
        Vector2Int from = selectedFrom.Coord;
        Vector2Int to = tile.Coord;

        if (!TileVisual.potentialTiles.Contains(tile))
        {
            PlaySound(invalidMoveSound);
            UnSelect();
            return;
        }

        string move = BoardCoords.coordToName[from] + " " + BoardCoords.coordToName[to];
        var nextState = new BoardState(CurrentBoard, move, BoardState.ConstructType.Full);

        MakeMove(nextState);
    }

    public IEnumerable<string> GetMovesFromPoint(string from)
    {
        foreach (var possibleState in CurrentBoard.PossibleMoves)
        {
            string move = possibleState.lastMove;

            if (move.StartsWith(from))
            {
                yield return move;
            }
        }
    }

    bool CanMoveFromPoint(string from)
    {
        foreach(var possibleState in CurrentBoard.PossibleMoves)
        {
            if (possibleState.lastMove.StartsWith(from)) return true;
        }

        return false;
    }

    void Select(TileVisual tile)
    {
        if (!IsCurrentTurnFigure(tile.Figure)) return;
        if (!CanMoveFromPoint(tile.CoordName)) return;

        PlaySound(selectSound);
        selectedFrom = tile;
        tile.OnSelect();
    }

    public void UnSelect(bool withSound = false)
    {
        if (selectedFrom == null) return;

        if (withSound) PlaySound(unselectSound);

        selectedFrom.OnUnSelect();
        selectedFrom = null;
    }

    public void MakeMove(BoardState nextState)
    {
        /* probably not needed if we got this far
        if(!Rules.CanMakeMove(CurrentBoard, move, true))
        {
            Debug.LogError($"{move} is illegal!");
            return;
        }
        */

        nextState.InitPossibleMoves();

        moveNumber++;
        UnSelect();
        PushBoard(nextState);

        if (nextState.IsOver)
        {
            GameOver();
            return;
        }

        if (nextState.justAscendedPawn) PlaySound(pawnAscensionSound);
        else if (nextState.isCheck) PlaySound(checkSound);
        else if (nextState.justCastled) PlaySound(castlingSound);
        else if (nextState.justKilledAFigure) PlaySound(killSound);
        else PlaySound(moveSound);

        SnapCameras();

        bool aiTurnNow = (bot != null) && (nextState.lastMoveByWhite == playerIsWhite); // player's color moved

        if (aiTurnNow) MakeBotMove();
    }

    public void Switch2D3D()
    {
        SelectDisplayDimension(!IsDisplaying3D);
    }

    void SelectDisplayDimension(bool newModeIs3D)
    {
        if (IsDisplaying3D == newModeIs3D) return;

        cam3D.gameObject.SetActive(newModeIs3D);
        cam2D.gameObject.SetActive(!newModeIs3D);

        visuals.SwitchDimensions();
    }

    void OnScreenFlipped(Vector2Int oldSize, Vector2Int newSize)
    {
        bool isHorizontal = newSize.x > newSize.y;

        //Debug.Log("Screen just flipped!");
        
        var newUI = LoadManager.instance.GetClosestGameUI(newSize);

        if (newUI == UI) return;
        if (UI != null) UI.FX.SetActive(false);

        newUI.OnActivated(UI?.Data);
        UI = newUI;

        SnapCameras();
    }

    #region Buttons

    public void NextLevel(bool asWhite)
    {
        MenuManager.Play(level + 1, asWhite, SceneManager.GetActiveScene().name);
    }

    public void ToggleAudio()
    {
        audioAllowed = !audioAllowed;
        Data.audioAllowedImage.sprite = audioAllowed ? audioEnabledSprite : audioDisabledSprite;
    }

    public void Exit()
    {
        Application.Quit();
    }

    #endregion

    public void SaveBoard(string name)
    {
        Config.SaveGame(name, level, playerIsWhite, moves);
        var list = new List<string>(Config.GetSavedGameNames())
        {
            name
        };
        Config.SetSavedGameNames(list.ToArray());
    }

    void LoadBoard(string name) // may be obsolete
    {
        states.Clear();
        moves.Clear();

        states.Push(initialState);

        var data = Config.LoadGame(name);

        foreach(var state in data.boards)
        {
            string move = state.lastMove;

            states.Push(state);
            moves.Push(move);
        }
    }
}
