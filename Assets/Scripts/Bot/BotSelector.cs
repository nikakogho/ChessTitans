using UnityEngine;
using Chess.AI;

public static class BotSelector
{
    public static Bot Select(int level)
    {
        switch (level)
        {
            case 1: return new Bot(BoardStateEvaluator.EvaluateRandomly);
            case 2: return new Bot(BoardStateEvaluator.EvaluateByFigures);
            case 3: return new Bot(BoardStateEvaluator.EvaluateByPosition);
            case 4: return new MiniMaxerBot(1);
            case 5: return new MiniMaxerBot(2);
            case 6: return new MiniMaxerBot(3);
            case 7: return new MiniMaxerBot(4);
            case 8: return new MiniMaxerBot(5);
            // rest is not here for now
            default:
                Debug.LogError($"No bot for level {level}");
                return null;
        }
    }
}
