using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Rules
{
    public static bool CanMakeMove(BoardState state, string move, bool careAboutCausingCheck)
    {
        if (!careAboutCausingCheck)
        {
            string moveFrom = move.Split(' ')[0];
            string moveTo = move.Split(' ')[1];

            var coord1 = BoardCoords.nameToCoord[moveFrom];
            var coord2 = BoardCoords.nameToCoord[moveTo];

            if (coord1 == coord2) return false; // must move to a different tile!

            var figure1 = state[coord1];
            //var figure2 = state[coord2]; // may need later

            return FullCheck(state, coord1, coord2, figure1);
        }
        else return CanMakeMove(state, move, false) && new BoardState(state, move, BoardState.ConstructType.IgnorePossibleMoves).IsLegal; // all moves are allowed for now
    }

    static List<Vector2Int> GetPotentialSpots(int x, int y, Figure figure)
    {
        List<Vector2Int> potentials = null;

        switch (figure)
        {
            case Figure.Empty: return new List<Vector2Int>();
            case Figure.PawnBlack:
                potentials = new List<Vector2Int>()
                {
                    new Vector2Int(x + 1, y - 1), new Vector2Int(x - 1, y - 1), new Vector2Int(x, y - 1)
                };
                if (y == 6) potentials.Add(new Vector2Int(x, y - 2));
                break;
            case Figure.PawnWhite:
                potentials = new List<Vector2Int>()
                {
                    new Vector2Int(x + 1, y + 1), new Vector2Int(x - 1, y + 1), new Vector2Int(x, y + 1)
                };
                if (y == 1) potentials.Add(new Vector2Int(x, y + 2));
                break;
            case Figure.HorseBlack:
            case Figure.HorseWhite:
                potentials = new List<Vector2Int>()
                        {
                            new Vector2Int(x + 2, y + 1),  new Vector2Int(x - 2, y + 1),
                            new Vector2Int(x + 2, y - 1),  new Vector2Int(x - 2, y - 1),
                            new Vector2Int(x + 1, y + 2),  new Vector2Int(x - 1, y + 2),
                            new Vector2Int(x + 1, y - 2),  new Vector2Int(x - 1, y - 2)
                        };
                break;
            case Figure.BishopBlack:
            case Figure.BishopWhite:
                potentials = new List<Vector2Int>();
                for (int i = -8; i < 8; i++)
                {
                    potentials.Add(new Vector2Int(x + i, y + i));
                    potentials.Add(new Vector2Int(x + i, y - i));
                }
                break;
            case Figure.CastleBlack:
            case Figure.CastleWhite:
                potentials = new List<Vector2Int>();
                for (int i = 0; i < 8; i++)
                {
                    potentials.Add(new Vector2Int(x, i));
                    potentials.Add(new Vector2Int(i, y));
                }
                break;
            case Figure.QueenBlack:
            case Figure.QueenWhite:
                potentials = GetPotentialSpots(x, y, Figure.BishopBlack);
                potentials.AddRange(GetPotentialSpots(x, y, Figure.CastleBlack));
                break;
            case Figure.KingBlack:
            case Figure.KingWhite:
                potentials = new List<Vector2Int>();
                for (int X = x - 1; X < x + 2; X++) for (int Y = y - 1; Y < y + 2; Y++)
                        potentials.Add(new Vector2Int(X, Y));
                
                potentials.Add(new Vector2Int(x + 2, y));
                potentials.Add(new Vector2Int(x - 2, y));
                break;
        }

        return potentials;
    }

    public static List<BoardState> GetPossibleMoves(BoardState board)
    {
        //Debug.Log($"Getting possible moves for {(board.whiteTurnNext ? "Whites" : "Blacks")}");
        var moves = new List<BoardState>();

        for (int x1 = 0; x1 < 8; x1++) for (int y1 = 0; y1 < 8; y1++)
            {
                var figure1 = board[x1, y1];
                var coord1 = new Vector2Int(x1, y1);

                if (figure1 == Figure.Empty) continue; // empty tile
                if (board.blackTurnNext && figure1 < Figure.PawnBlack) continue; // not our figure
                if (board.whiteTurnNext && figure1 >= Figure.PawnBlack) continue; // not our figure

                string from = BoardCoords.coordToName[coord1];
                var potentials = GetPotentialSpots(x1, y1, figure1);

                foreach (var coord in potentials)
                {
                    int x2 = coord.x, y2 = coord.y;
                    if (x2 < 0 || y2 < 0 || x2 > 7 || y2 > 7) continue; // outside the board
                    if (coord == coord1) continue; // same coordinate not allowed

                    var figure2 = board[x2, y2];
                    var coord2 = new Vector2Int(x2, y2);

                    bool figure2IsBlack = (figure2 >= Figure.PawnBlack);
                    bool figure2IsWhite = (figure2 >= Figure.PawnWhite && figure2 <= Figure.KingWhite);

                    // can't kill a comrade
                    if ((figure2IsBlack && board.blackTurnNext) || (figure2IsWhite && board.whiteTurnNext)) continue;

                    bool canMakeThisMove = SpecificsCheck(board, coord1, coord2, figure1);

                    if (canMakeThisMove)
                    {
                        string to = BoardCoords.coordToName[coord2];

                        string move = from + " " + to;

                        var resultingState = new BoardState(board, move, BoardState.ConstructType.IgnorePossibleMoves);

                        if(resultingState.IsLegal) moves.Add(resultingState);
                    }
                }
            }

        return moves;
    }

    static bool FullCheck(BoardState board, Vector2Int coord1, Vector2Int coord2, Figure checkAsFigure)
    {
        return RoughCheck(board, coord1, coord2, checkAsFigure) && SpecificsCheck(board, coord1, coord2, checkAsFigure);
    }

    static bool RoughCheck(BoardState board, Vector2Int coord1, Vector2Int coord2, Figure figure)
    {
        int x1 = coord1.x, y1 = coord1.y;
        int x2 = coord2.x, y2 = coord2.y;

        switch (figure)
        {
            case Figure.Empty: return false;

            case Figure.PawnBlack:
                if (y2 == y1 - 1 && Math.Abs(x1 - x2) <= 1) return true;
                else if (y1 == 6) return x1 == x2 && y2 == y1 - 2;
                else return false;

            case Figure.PawnWhite:
                if (y2 == y1 + 1 && Math.Abs(x1 - x2) <= 1) return true;
                else if (y1 == 6) return x1 == x2 && y2 == y1 + 2;
                else return false;

            case Figure.HorseBlack:
            case Figure.HorseWhite:
                return (Math.Abs(x1 - x2) == 2 && Math.Abs(y1 - y2) == 1) || (Math.Abs(x1 - x2) == 1 && Math.Abs(y1 - y2) == 2);

            case Figure.BishopBlack:
            case Figure.BishopWhite:
                return Math.Abs(x1 - x2) == Math.Abs(y1 - y2);

            case Figure.CastleBlack:
            case Figure.CastleWhite:
                return (x1 == x2) || (y1 == y2);

            case Figure.QueenBlack:
            case Figure.QueenWhite:
                return (Math.Abs(x1 - x2) == Math.Abs(y1 - y2)) || (x1 == x2) || (y1 == y2);

            case Figure.KingBlack:
            case Figure.KingWhite:
                if (Math.Abs(x1 - x2) < 2 && Math.Abs(y1 - y2) < 2) return true;
                else return (Math.Abs(x1 - x2) == 2) && (y1 == y2);
        }

        string name1 = BoardCoords.coordToName[coord1];
        string name2 = BoardCoords.coordToName[coord2];

        Debug.LogError($"How did you get here? Your job was to check from {name1} to {name2}");
        return false;
    }

    static bool NobodyTouchesHere(BoardState board, Vector2Int coord, bool blacksAreEnemies)
    {
        string to = BoardCoords.coordToName[coord];

        for(int x = 0; x < 8; x++) for(int y = 0; y < 8; y++)
            {
                var figure = board[x, y];

                if (figure == Figure.Empty) continue;
                if ((figure < Figure.PawnBlack) == blacksAreEnemies) continue; // not an enemy figure

                string from = BoardCoords.coordToName[new Vector2Int(x, y)];
                string move = from + " " + to;

                if (CanMakeMove(board, move, false)) return false;
            }

        return true;
    }

    static bool NobodyTouchesHere(BoardState board, int x, int y, bool blacksAreEnemies)
    {
        return NobodyTouchesHere(board, new Vector2Int(x, y), blacksAreEnemies);
    }

    static bool SpecificsCheck(BoardState board, Vector2Int coord1, Vector2Int coord2, Figure checkAsFigure)
    {
        if (checkAsFigure == Figure.Empty) return false; // no move allowed for empty tiles

        bool isWhite = checkAsFigure < Figure.PawnBlack;

        if (coord2.x < 0 || coord2.y < 0 || coord2.x > 7 || coord2.y > 7) return false; // cant move outside the board

        var figure2 = board[coord2];

        bool fig2Empty = figure2 == Figure.Empty;
        bool fig2Black = figure2 >= Figure.PawnBlack;
        bool fig2White = !fig2Empty && !fig2Black;

        if (fig2Black && !isWhite) return false; // cant kill a comrade
        if (fig2White && isWhite) return false; // cant kill a comrade

        switch (checkAsFigure)
        {
            case Figure.KingBlack:
            case Figure.KingWhite:
                if (Math.Abs(coord1.x - coord2.x) <= 1) return true; // normal moves are always allowed

                #region Castling

                if (board.isCheck) return false; // castling is not allowed while we're under check
                else if (isWhite)
                {
                    if (board.whiteKingHasMoved) return false;
                    if (coord2.x == 2)
                        return !board.whiteRookLeftHasMoved && board[0, 0] == Figure.CastleWhite &&
                            board[1, 0] == Figure.Empty && board[2, 0] == Figure.Empty && board[3, 0] == Figure.Empty
                            && NobodyTouchesHere(board, 1, 0, true) && NobodyTouchesHere(board, 2, 0, true) &&
                            NobodyTouchesHere(board, 3, 0, true);

                    else
                        return !board.whiteRookRightHasMoved && board[7, 0] == Figure.CastleWhite &&
                            board[6, 0] == Figure.Empty && board[5, 0] == Figure.Empty &&
                            NobodyTouchesHere(board, 5, 0, true) && NobodyTouchesHere(board, 6, 0, true);
                }
                else
                {
                    if (board.blackKingHasMoved) return false;
                    if (coord2.x == 2)
                        return !board.blackRookLeftHasMoved && board[0, 7] == Figure.CastleBlack &&
                            board[1, 7] == Figure.Empty && board[2, 7] == Figure.Empty && board[3, 7] == Figure.Empty
                            && NobodyTouchesHere(board, 1, 7, false) && NobodyTouchesHere(board, 2, 7, false) &&
                            NobodyTouchesHere(board, 3, 7, false);

                    else
                        return !board.blackRookRightHasMoved && board[7, 7] == Figure.CastleBlack &&
                            board[6, 7] == Figure.Empty && board[5, 7] == Figure.Empty &&
                            NobodyTouchesHere(board, 5, 7, false) && NobodyTouchesHere(board, 6, 7, false);
                }
                #endregion

            case Figure.QueenBlack:
                return SpecificsCheck(board, coord1, coord2, Figure.BishopBlack) || SpecificsCheck(board, coord1, coord2, Figure.CastleBlack);

            case Figure.QueenWhite:
                return SpecificsCheck(board, coord1, coord2, Figure.BishopWhite) || SpecificsCheck(board, coord1, coord2, Figure.CastleWhite);

            case Figure.HorseBlack:
            case Figure.HorseWhite:
                return true; // horse can move just fine
                
            case Figure.PawnBlack:
            case Figure.PawnWhite:
                if(coord1.x == coord2.x)
                {
                    if (board[coord2] != Figure.Empty) return false; // pawn cant kill when moving straight
                    else if (Math.Abs(coord1.y - coord2.y) == 2) return board[coord1.x, (coord1.y + coord2.y) / 2] == Figure.Empty;
                    else return true;
                }
                else if (!fig2Empty)
                {
                    return (isWhite == fig2Black); // make sure we kill the enemy
                }
                else // passing kill
                {
                    if (board.lastMove == null) return false;

                    var figToKillCoords = new Vector2Int(coord2.x, coord1.y);
                    var figToKill = board[figToKillCoords];

                    if (isWhite && figToKill != Figure.PawnBlack) return false; // cant kill anything other than black pawn
                    if (!isWhite && figToKill != Figure.PawnWhite) return false; // cant kill anything other than white pawn

                    var lastToCoords = BoardCoords.nameToCoord[board.lastMovedTo];
                    var lastFig = board[lastToCoords];

                    var lastFromCoords = BoardCoords.nameToCoord[board.lastMovedFrom];

                    return Math.Abs(lastFromCoords.y - lastToCoords.y) == 2 && lastToCoords == figToKillCoords; // pawn moved this way last time
                }

            case Figure.BishopBlack:
            case Figure.BishopWhite:
                int deltaX = (coord2.x - coord1.x > 0) ? 1 : -1;
                int deltaY = (coord2.y - coord1.y > 0) ? 1 : -1;

                if (Math.Abs(coord1.x - coord2.x) != Math.Abs(coord1.y - coord2.y)) return false; // not a diagonal

                //Debug.Log($"Moving from {coord1} to {coord2}");
                
                for(int pathX = coord1.x + deltaX, pathY = coord1.y + deltaY; pathX != coord2.x; pathX += deltaX, pathY += deltaY)
                {
                    //Debug.Log($"On path we are now at ({pathX}, {pathY})");
                    if (board[pathX, pathY] != Figure.Empty) return false; // nothing should be in way
                }

                return true;

            case Figure.CastleBlack:
            case Figure.CastleWhite:
                if (coord1.x != coord2.x && coord1.y != coord2.y) return false; // not a straight line

                deltaX = deltaY = 0;

                if (coord2.x > coord1.x) deltaX = 1;
                else if (coord2.x < coord1.x) deltaX = -1;
                else if (coord2.y > coord1.y) deltaY = 1;
                else deltaY = -1;

                for (int pathX = coord1.x + deltaX, pathY = coord1.y + deltaY; pathX != coord2.x || pathY != coord2.y; pathX += deltaX, pathY += deltaY)
                {
                    if (board[pathX, pathY] != Figure.Empty) return false; // nothing should be in way
                }

                return true;
        }

        Debug.LogError("How did you get here? Well, you see, there are no moves for figure " + checkAsFigure);
        return false;
    }
}
