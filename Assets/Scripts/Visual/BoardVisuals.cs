using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Config;

public class BoardVisuals : MonoBehaviour
{
    BoardConfig boardConfig;
    public GameObject tilePrefab, killedFigurePrefab;
    public Transform tilesParent;

    public Transform killedWhitesParent;
    public Transform killedBlacksParent;

    TileRow[] grid;

    KilledFigureVisual[] killedWhites;
    KilledFigureVisual[] killedBlacks;

    public TileVisual this[int x, int y] => grid[y][x];

    GameManager manager;

    public Vector2 startPos;
    public Vector2 delta;

    public static BoardVisuals instance;

    bool show3D;

    void Awake()
    {
        instance = this;
    }

    public void Init(GameManager manager)
    {
        show3D = true;

        killedWhites = new KilledFigureVisual[16];
        killedBlacks = new KilledFigureVisual[16];
        grid = new TileRow[8];

        for (int i = 0; i < 8; i++) grid[i] = new TileRow();
        for(int i = 0; i < 16; i++)
        {
            killedWhites[i] = Instantiate(killedFigurePrefab, killedWhitesParent.GetChild(i)).GetComponent<KilledFigureVisual>();
            killedBlacks[i] = Instantiate(killedFigurePrefab, killedBlacksParent.GetChild(i)).GetComponent<KilledFigureVisual>();
        }

        this.manager = manager;
        boardConfig = GameManager.CurrentBoardConfig;

        for(int y = 0; y < 8; y++) for(int x = 0; x < 8; x++) 
            {
                Vector3 tilePos = new Vector3(startPos.x + y * delta.x, 0, startPos.y + x * delta.y);
                var tile = Instantiate(tilePrefab, tilePos, Quaternion.identity);
                tile.transform.parent = tilesParent;
                (grid[y][x] = tile.GetComponent<TileVisual>()).Init(x, y, boardConfig.figuresFX, GameManager.CurrentBoard, this); // may fix
            }

        DrawBoard(GameManager.CurrentBoard);
    }

    void InitKilledSpot(KilledFigureVisual spot, Figure killed)
    {
        var fx = boardConfig.figuresFX[(int)killed];

        spot.Init(fx.prefab3D, fx.prefab2D, show3D);
    }

    public void DrawBoard(BoardState board)
    {
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++) grid[y][x].DrawBoard(board);

        for(int i = 0; i < 16; i++)
        {
            if (board.KilledBlacks.Count > i) InitKilledSpot(killedBlacks[i], board.KilledBlacks[i]);
            else killedBlacks[i].Clear();

            if (board.KilledWhites.Count > i) InitKilledSpot(killedWhites[i], board.KilledWhites[i]);
            else killedWhites[i].Clear();
        }
    }

    public void SwitchDimensions()
    {
        show3D = !show3D;

        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++) grid[y][x].SwitchDimensions();
        for(int i = 0; i < 16; i++)
        {
            killedWhites[i].SwitchDimensions();
            killedBlacks[i].SwitchDimensions();
        }
    }
    
    class TileRow
    {
        public TileVisual[] tiles;

        public TileRow()
        {
            tiles = new TileVisual[8];
        }

        public TileVisual this[int index] { get { return tiles[index]; } set { tiles[index] = value; } }
    }
}
