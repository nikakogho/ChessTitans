using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Chess.AI
{
    public static class SampleMoveChoosers
    {
        public static BoardState GetMoveByJustEvaluatingBoard(BoardState state, bool bestForWhite, BoardEvaluator evaluator)
        {
            var moves = state.PossibleMoves;
            var options = new List<BoardState>();

            /*
            Debug.Log("For this step");
            Debug.Log("Possible moves are:");
            foreach (string move in state.possibleMoves) Debug.Log(move);
            Debug.Log("Done for this step");
            */

            if (bestForWhite)
            {
                int bestValue = int.MinValue;

                foreach (var next in moves)
                {
                    next.InitPossibleMoves(); // MAYDO might move this inside the evaluator
                    int value = evaluator(next);

                    if (value > bestValue)
                    {
                        options.Clear();
                        bestValue = value;
                    }
                    if (value == bestValue) options.Add(next);
                }
            }
            else
            {
                int bestValue = int.MaxValue;

                foreach (var next in moves)
                {
                    next.InitPossibleMoves(); // might move this inside the evaluator
                    int value = evaluator(next);

                    if (value < bestValue)
                    {
                        options.Clear();
                        bestValue = value;
                    }
                    if (value == bestValue) options.Add(next);
                }
            }
            
            return options[Random.Range(0, options.Count)];
        }
    }
}