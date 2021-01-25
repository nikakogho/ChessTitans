using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Chess.Config;

public class TileVisual : MonoBehaviour
{
    public Transform fxParent;
    public Canvas canvas;
    public Image image;

    public Color defaultTileColor = new Color(1, 1, 1, 0);
    public Color threatenedTileColor = Color.red;
    public Color movedFromColor = new Color(1, 0.5f, 0);
    public Color movedToColor = Color.yellow;
    public Color selectedColor = Color.green;
    public Color possibleMoveColor = Color.cyan;

    public int X { get; private set; }
    public int Y { get; private set; }
    public Vector2Int Coord { get; private set; }
    public string CoordName { get; private set; }
    public Figure Figure { get; private set; }

    GameObject activeFigure;

    GameObject[] figures; // skip 0 cause it's empty
    Dictionary<GameObject, KeyValuePair<GameObject, GameObject>> figureModes;

    public static List<TileVisual> potentialTiles = new List<TileVisual>();
    public static TileVisual Selected { get; private set; }

    BoardState board;
    BoardVisuals visuals;

    bool show3d = true;

    void Awake()
    {
        Selected = null;
    }

    public void Init(int x, int y, BoardConfig.FigureFX[] prefabs, BoardState board, BoardVisuals visuals)
    {
        this.board = board;
        this.visuals = visuals;

        figureModes = new Dictionary<GameObject, KeyValuePair<GameObject, GameObject>>();

        canvas.worldCamera = Camera.main;

        X = x;
        Y = y;

        Coord = new Vector2Int(x, y);
        CoordName = BoardCoords.coordToName[Coord];

        figures = new GameObject[prefabs.Length];
        for(int i = 1; i < figures.Length; i++) // skip 0 cause it's empty
        {
            string figureName = Enum.GetName(typeof(Figure), board[Coord]);

            var obj = new GameObject(figureName);

            obj.transform.position = fxParent.position;
            obj.transform.rotation = fxParent.rotation;
            obj.transform.parent = fxParent;

            var fx3d = Instantiate(prefabs[i].prefab3D, fxParent.position, fxParent.rotation);
            var fx2d = Instantiate(prefabs[i].prefab2D, fxParent.position, fxParent.rotation);

            fx3d.transform.parent = obj.transform;
            fx2d.transform.parent = obj.transform;

            figureModes[obj] = new KeyValuePair<GameObject, GameObject>(fx3d, fx2d);

            obj.SetActive(false);

            figures[i] = obj;
        }

        Figure = board[x, y];
        int index = (int)Figure;

        activeFigure = figures[index];

        image.color = defaultTileColor;

        if(activeFigure != null) activeFigure.SetActive(true);
    }

    public void DrawBoard(BoardState newState)
    {
        board = newState;
        if (activeFigure != null) activeFigure.SetActive(false);

        Figure = newState[X, Y];

        activeFigure = figures[(int)Figure];

        if (activeFigure != null)
        {
            activeFigure.SetActive(true);
            figureModes[activeFigure].Key.SetActive(show3d);
            figureModes[activeFigure].Value.SetActive(!show3d);

            if (!show3d)
            {
                Vector3 euler = new Vector3(0, GameManager.IsDisplayingWhiteSide ? 0 : 180);
                figureModes[activeFigure].Value.transform.rotation = Quaternion.Euler(euler);
            }
        }

        ResetColor();
    }

    void ResetColor()
    {
        var tileColor = defaultTileColor;
        var figure = board[X, Y];

        string from = board.lastMovedFrom;
        string to = board.lastMovedTo;

        if (board.isCheck)
        {
            if (figure == Figure.KingBlack && board.lastMoveByWhite) tileColor = threatenedTileColor;
            else if (figure == Figure.KingWhite && board.lastMoveByBlack) tileColor = threatenedTileColor;
        }
        if (from != null && BoardCoords.nameToCoord[from] == Coord) tileColor = movedFromColor;
        else if (to != null && BoardCoords.nameToCoord[to] == Coord) tileColor = movedToColor;

        image.color = tileColor;
    }

    public void OnClick()
    {
        //Debug.Log($"{Coord} was clicked!");
        //Debug.Log($"{CoordName} was clicked!");
    }

    /* probably useless
    public void OnMouseEntered()
    {
        Debug.Log($"{CoordName} was entered!");
    }

    public void OnMouseExited()
    {

        Debug.Log($"{CoordName} was exited!");
    }
    */

    public void OnSelect()
    {
        Selected = this;
        image.color = selectedColor;

        foreach (string move in GameManager.instance.GetMovesFromPoint(CoordName))
        {
            string moveToName = move.Split(' ')[1];
            var moveToCoord = BoardCoords.nameToCoord[moveToName];
            
            var moveToTile = visuals[moveToCoord.x, moveToCoord.y];

            moveToTile.image.color = possibleMoveColor;
            potentialTiles.Add(moveToTile);
        }
    }

    public void SwitchDimensions()
    {
        show3d = !show3d;

        if (activeFigure == null) return;

        figureModes[activeFigure].Key.SetActive(show3d);
        figureModes[activeFigure].Value.SetActive(!show3d);

        if (!show3d)
        {
            Vector3 euler = new Vector3(0, GameManager.IsDisplayingWhiteSide ? 0 : 180);
            figureModes[activeFigure].Value.transform.rotation = Quaternion.Euler(euler);
        }
    }

    public void OnUnSelect()
    {
        Selected = null;
        ResetColor();
        foreach (var tile in potentialTiles) tile.ResetColor();
        potentialTiles.Clear();
    }
}
