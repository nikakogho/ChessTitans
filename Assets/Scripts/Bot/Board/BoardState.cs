using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public enum ConstructType { Full, IgnorePossibleMoves, IgnoreLegalityAndPossibleMoves }

    #region Last Move
    public readonly string lastMove;
    public readonly string lastMovedFrom;
    public readonly string lastMovedTo;

    public readonly bool lastMoveByWhite;
    public readonly bool lastMoveByBlack;
    public readonly bool whiteTurnNext;
    public readonly bool blackTurnNext;
    #endregion

    public readonly bool isCheck;
    public bool CanMakeMove => PossibleMoves.Count > 0;

    public readonly bool IsLegal;

    #region Castling
    public readonly bool whiteKingHasMoved;
    public readonly bool blackKingHasMoved;

    public readonly bool whiteRookLeftHasMoved;
    public readonly bool blackRookLeftHasMoved;
    public readonly bool whiteRookRightHasMoved;
    public readonly bool blackRookRightHasMoved;
    #endregion

    public readonly bool justAscendedPawn;
    public readonly bool justKilledAFigure;
    public readonly bool justCastled;

    public bool IsOver => !CanMakeMove;
    public bool IsCheckmate => isCheck && IsOver;
    public bool IsDraw => !isCheck && IsOver;

    public Figure this[int x, int y]
    {
        get { return board[y, x]; }
        private set { board[y, x] = value; }
    }

    public Figure this[Vector2Int coord]
    {
        get { return board[coord.y, coord.x]; }
        private set { board[coord.y, coord.x] = value; }
    }

    public List<Figure> KilledWhites { get; private set; }
    public List<Figure> KilledBlacks { get; private set; }

    readonly Figure[,] board;
    public List<BoardState> PossibleMoves { get; private set; }

    //public int Value { get; private set; }
    //public bool IsValueEvaluated => Value != int.MinValue; // should I allow reevaluation?

    public BoardState(Figure[,] board) // starting board
    {
        this.board = new Figure[8, 8];
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++) this.board[x, y] = board[x, y];

        #region Initialize as clear game
        lastMoveByWhite = lastMoveByBlack = whiteKingHasMoved = blackKingHasMoved = false;
        isCheck = false;
        whiteTurnNext = true;
        blackTurnNext = false;
        whiteRookLeftHasMoved = blackRookLeftHasMoved = whiteRookRightHasMoved = false;
        blackRookRightHasMoved = false;
        IsLegal = true;
        justKilledAFigure = false;
        lastMove = lastMovedFrom = lastMovedTo = null;
        #endregion

        KilledWhites = new List<Figure>();
        KilledBlacks = new List<Figure>();

        //PossibleMoves = Rules.GetPossibleMoves(this);
    }

    public BoardState(BoardState previous, string move, ConstructType constructType)
    {
        #region Apply previous stats

        #region Moved
        whiteKingHasMoved = previous.whiteKingHasMoved;
        blackKingHasMoved = previous.blackKingHasMoved;
        whiteRookLeftHasMoved = previous.whiteRookLeftHasMoved;
        blackRookLeftHasMoved = previous.blackRookLeftHasMoved;
        whiteRookRightHasMoved = previous.whiteRookRightHasMoved;
        blackRookRightHasMoved = previous.blackRookRightHasMoved;
        #endregion

        #region Board

        board = new Figure[8, 8];
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++) board[x, y] = previous.board[x, y];

        KilledWhites = new List<Figure>(previous.KilledWhites);
        KilledBlacks = new List<Figure>(previous.KilledBlacks);

        #endregion

        #endregion

        #region Move Stuff
        lastMove = move;
        lastMovedFrom = lastMove.Split(' ')[0];
        lastMovedTo = lastMove.Split(' ')[1];

        var movedFromCoords = BoardCoords.nameToCoord[lastMovedFrom];
        var movedToCoords = BoardCoords.nameToCoord[lastMovedTo];

        var movedFigure = this[movedFromCoords]; 
        var movedToFigure = this[movedToCoords];

        lastMoveByWhite = movedFigure >= Figure.PawnWhite && movedFigure <= Figure.KingWhite;
        lastMoveByBlack = movedFigure >= Figure.PawnBlack && movedFigure <= Figure.KingBlack;

        whiteTurnNext = lastMoveByBlack;
        blackTurnNext = lastMoveByWhite;

        #region Apply Changes

        var killedFigure = this[movedToCoords];

        if (killedFigure != Figure.Empty)
        {
            if (lastMoveByWhite) KilledBlacks.Add(killedFigure);
            else KilledWhites.Add(killedFigure);

            justKilledAFigure = true;
        }

        this[movedFromCoords] = Figure.Empty;
        this[movedToCoords] = movedFigure;

        // special cases for King and Pawn

        switch (movedFigure)
        {
            #region Castling
            case Figure.KingBlack:
                if(movedToCoords.x == movedFromCoords.x + 2) // castling right
                {
                    this[7, 7] = Figure.Empty;
                    this[5, 7] = Figure.CastleBlack;
                    blackRookRightHasMoved = true;
                    justCastled = true;
                }
                else if(movedToCoords.x == movedFromCoords.x - 2) // castling left
                {
                    this[0, 7] = Figure.Empty;
                    this[3, 7] = Figure.CastleBlack;
                    blackRookLeftHasMoved = true;
                    justCastled = true;
                }
                blackKingHasMoved = true;
                break;

            case Figure.KingWhite:
                if (movedToCoords.x == movedFromCoords.x + 2) // castling right
                {
                    this[7, 0] = Figure.Empty;
                    this[5, 0] = Figure.CastleWhite;
                    whiteRookRightHasMoved = true;
                    justCastled = true;
                }
                else if (movedToCoords.x == movedFromCoords.x - 2) // castling left
                {
                    this[0, 0] = Figure.Empty;
                    this[3, 0] = Figure.CastleWhite;
                    whiteRookLeftHasMoved = true;
                    justCastled = true;
                }
                whiteKingHasMoved = true;
                break;
            #endregion

            case Figure.PawnBlack:
            case Figure.PawnWhite:
                // for now, turn into queen automatically

                if(movedToCoords.y == 0 || movedToCoords.y == 7)
                {
                    this[movedToCoords] = (whiteTurnNext ? Figure.QueenBlack : Figure.QueenWhite); // ascend to queen
                    justAscendedPawn = true;
                }
                else if(movedFromCoords.x != movedToCoords.x && killedFigure == Figure.Empty) // pass kill
                {
                    var killedPawnCoords = new Vector2Int(movedToCoords.x, movedFromCoords.y);
                    var justKilledFigure = this[killedPawnCoords];

                    if (lastMoveByWhite) KilledBlacks.Add(justKilledFigure);
                    else KilledWhites.Add(justKilledFigure);

                    this[killedPawnCoords] = Figure.Empty; // kill the pawn
                    justKilledAFigure = true;
                }

                break;
        }

        #endregion

        #region Find Kings

        Figure ourKing = lastMoveByWhite ? Figure.KingBlack : Figure.KingWhite;
        Figure enemyKing = lastMoveByBlack ? Figure.KingBlack : Figure.KingWhite;
        Vector2Int ourKingCoords = new Vector2Int(-1, -1);
        Vector2Int enemyKingCoords = new Vector2Int(-1, -1);
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++)
            {
                if (this[x, y] == ourKing)
                {
                    ourKingCoords.x = x;
                    ourKingCoords.y = y;
                }
                else if (this[x, y] == enemyKing)
                {
                    enemyKingCoords.x = x;
                    enemyKingCoords.y = y;
                }
            }

        string ourKingCoordName = BoardCoords.coordToName[ourKingCoords];
        string enemyKingCoordName = BoardCoords.coordToName[enemyKingCoords];

        #endregion

        #region Check For Check
        for (int x = 0; !isCheck && x < 8; x++) for(int y = 0; y < 8; y++)
            {
                var fig = this[x, y];

                if (fig == Figure.Empty) continue;

                bool isBlack = fig >= Figure.PawnBlack;

                bool isEnemy = (whiteTurnNext == isBlack);

                if (!isEnemy) continue;

                string from = BoardCoords.coordToName[new Vector2Int(x, y)];
                string checkMove = from + " " + ourKingCoordName; 

                if(Rules.CanMakeMove(this, checkMove, false))
                {
                    isCheck = true;
                    break;
                }
            }
        #endregion

        IsLegal = true;

        if (constructType == ConstructType.IgnoreLegalityAndPossibleMoves) return;

        #region Check That This Move Was Legal
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++)
            {
                var fig = this[x, y];
                if (fig == Figure.Empty) continue;
                if (blackTurnNext == (fig >= Figure.PawnBlack)) // our figure
                {
                    string figCoordName = BoardCoords.coordToName[new Vector2Int(x, y)];
                    string kingKillingMove = figCoordName + " " + enemyKingCoordName;

                    if (Rules.CanMakeMove(this, kingKillingMove, false)) // can kill on our turn so it's illegal
                    {
                        IsLegal = false;
                        //Debug.Log($"This move {move} is illegal because {kingKillingMove} can be made!");
                        return;
                    }
                }
            }
        #endregion

        if (constructType == ConstructType.IgnorePossibleMoves) return;

        #endregion

        InitPossibleMoves();
    }

    bool CheckForLivingFigures()
    {
        for (int x = 0; x < 8; x++) for (int y = 0; y < 8; y++)
            {
                var figure = this[x, y];

                if (figure != Figure.KingBlack && figure != Figure.KingWhite && figure != Figure.Empty) return true;
            }

       return false;
    }

    public void InitPossibleMoves()
    {
        if (PossibleMoves != null) return;
        if (!CheckForLivingFigures())
        {
            PossibleMoves = new List<BoardState>(); // make it a draw
            return;
        }

        PossibleMoves = Rules.GetPossibleMoves(this);
    }
}

public enum Figure {
    Empty, PawnWhite, HorseWhite, BishopWhite,
    CastleWhite, QueenWhite, KingWhite, PawnBlack,
    HorseBlack, BishopBlack, CastleBlack, QueenBlack, KingBlack }