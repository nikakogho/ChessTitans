using System.Collections.Generic;
using UnityEngine;

public static class BoardCoords
{
    public static Dictionary<string, int> nameToIndex;
    public static string[] indexToName;
    public static Vector2Int[] indexToCoord;
    public static Dictionary<string, Vector2Int> nameToCoord;
    public static Dictionary<Vector2Int, string> coordToName;

    static bool setup = false;
    
    public static void Setup()
    {
        if (setup) return;

        setup = true;

        nameToIndex = new Dictionary<string, int>();
        indexToName = new string[64];
        indexToCoord = new Vector2Int[64];
        nameToCoord = new Dictionary<string, Vector2Int>();
        coordToName = new Dictionary<Vector2Int, string>();

        string alphabet = "abcdefgh";

        for (int y = 0; y < 8; y++) for (int x = 0; x < 8; x++)
            {
                string name = alphabet[x] + (y + 1).ToString();
                int index = y * 8 + x;
                var coord = new Vector2Int(x, y);

                nameToIndex[name] = index;
                indexToName[index] = name;
                indexToCoord[index] = coord;
                nameToCoord[name] = coord;
                coordToName[coord] = name;
            }
    }
}
