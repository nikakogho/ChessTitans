using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.AI
{
    public class Bot
    {
        public BoardEvaluator Evaluator { get; protected set; }

        public Bot(BoardEvaluator evaluator)
        {
            Evaluator = evaluator;
        }

        protected Bot()
        {
            // make sure to give something to evaluator
        }
    }

    public class MiniMaxerBot : Bot
    {
        public readonly int depth;

        public MiniMaxerBot(int depth)
        {
            this.depth = depth;

            Evaluator = Evaluate;
        }

        int Evaluate(BoardState state)
        {
            return EvaluateForState(state, depth, int.MinValue, int.MaxValue);
        }

        int EvaluateForState(BoardState state, int depth, int alpha, int beta) // alpha-beta pruning is now applied (should be faster)
        {
            if (depth == 0) return BoardStateEvaluator.EvaluateByPosition(state);

            state.InitPossibleMoves();

            bool isWhite = state.whiteTurnNext;

            if (isWhite)
            {
                int max = int.MinValue;

                foreach(var next in state.PossibleMoves)
                {
                    int val = EvaluateForState(next, depth - 1, alpha, beta);

                    if (val > max) max = val;
                    if (val > alpha) alpha = val;

                    if (beta <= alpha)
                    {
                        //Debug.Log($"Pruned at level {this.depth - depth + 1}");
                        break;
                    }
                }

                return max;
            }
            else
            {
                int min = int.MaxValue;

                foreach(var next in state.PossibleMoves)
                {
                    int val = EvaluateForState(next, depth - 1, alpha, beta);

                    if (val < min) min = val;
                    if (val < beta) beta = val;

                    if (beta <= alpha)
                    {
                        //Debug.Log($"Pruned at level {this.depth - depth + 1}");
                        break;
                    }
                }

                return min;
            }
        }
    }
}