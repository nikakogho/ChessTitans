using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Config
{
    public static class Config
    {
        #region Saveds Listing
        public static void ClearSavedGameNames()
        {
            int count = PlayerPrefs.GetInt("Saved games count", 0);

            if (count == 0) return;

            PlayerPrefs.SetInt("Saved games count", 0);

            for (int i = 0; i < count; i++) PlayerPrefs.DeleteKey($"Saved game {i}");

            PlayerPrefs.Save();
        }

        public static void SetSavedGameNames(string[] names)
        {
            ClearSavedGameNames();
            PlayerPrefs.SetInt("Saved games count", names.Length);
            for (int i = 0; i < names.Length; i++) PlayerPrefs.SetString($"Saved game {i}", names[i]);
            PlayerPrefs.Save();
        }

        public static string[] GetSavedGameNames()
        {
            int size = PlayerPrefs.GetInt("Saved games count", 0);
            var names = new string[size];
            for (int i = 0; i < size; i++) names[i] = PlayerPrefs.GetString($"Saved game {i}");
            return names;
        }
        #endregion

        #region Individual Save
        public static void SaveGame(string name, int level, bool playerIsWhite, Stack<string> moves)
        {
            string boardName = "Board Save " + name;

            //Debug.Log($"Started saving {boardName}");

            PlayerPrefs.SetInt($"{boardName} Exists", 1);
            PlayerPrefs.SetInt($"{boardName} Level", level);
            PlayerPrefs.SetInt($"{boardName} Player Is White", playerIsWhite ? 1 : 0);
            PlayerPrefs.SetInt($"{boardName} Moves Count", moves.Count);

            int i = moves.Count - 1;

            foreach (string move in moves)
            {
                PlayerPrefs.SetString($"{boardName} move {i--}", move);
            }

            PlayerPrefs.Save();

            //Debug.Log($"Done saving {boardName}");
        }

        public static SavedGameData LoadGame(string name)
        {
            string boardName = "Board Save " + name;

            if (PlayerPrefs.GetInt($"{boardName} Exists", 0) == 0)
            {
                Debug.LogError($"There is no board saved by name {name}!");
                //throw new System.ArgumentException($"There is no bard saved by name {name}"); // may change
                return null;
            }

            int level = PlayerPrefs.GetInt($"{boardName} Level");
            bool playerIsWhite = PlayerPrefs.GetInt($"{boardName} Player Is White") == 1;

            int count = PlayerPrefs.GetInt($"{boardName} Moves Count");

            var boards = new BoardState[count];
            var moves = new string[count];

            var board = GameManager.initialState;

            // boards.Push(board);

            //Debug.Log(count);
            //for (int i = 0; i < count; i++) Debug.Log(PlayerPrefs.GetString($"{boardName} move {i}"));

            for (int i = 0; i < count; i++)
            {
                string move = PlayerPrefs.GetString($"{boardName} move {i}");
                //Debug.Log(move);
                board = new BoardState(board, move, BoardState.ConstructType.IgnoreLegalityAndPossibleMoves);

                moves[i] = move;
                boards[i] = board;
            }

            return new SavedGameData(name, level, playerIsWhite, moves, boards);
        }

        public static void DeleteGame(string name)
        {
            string boardName = "Board Save " + name;

            if (PlayerPrefs.GetInt($"{boardName} Exists", 0) == 0)
            {
                Debug.LogError($"There is no board saved by name {name}!");
                return;
            }

            PlayerPrefs.DeleteKey($"{boardName} Exists");
            PlayerPrefs.DeleteKey($"{boardName} Level");
            PlayerPrefs.DeleteKey($"{boardName} Player Is White");

            int count = PlayerPrefs.GetInt($"{boardName} Moves Count");
            PlayerPrefs.DeleteKey($"{boardName} Moves Count");

            for (int i = 0; i < count; i++) PlayerPrefs.DeleteKey($"{boardName} move {i}");

            PlayerPrefs.Save();
        }
        #endregion
    }

    public class SavedGameData : IDisposable
    {
        public readonly string name;
        public readonly bool playerIsWhite;
        public readonly int botLevel;
        public readonly string[] moves;
        public readonly BoardState[] boards;

        public SavedGameData(string name, int botLevel, bool playerIsWhite, string[] moves, BoardState[] boards)
        {
            this.name = name;
            this.playerIsWhite = playerIsWhite;
            this.botLevel = botLevel;
            this.moves = moves;
            this.boards = boards;
        }

        public void Dispose()
        {

        }
    }
}