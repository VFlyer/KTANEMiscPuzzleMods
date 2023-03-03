using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleLightsOut : PuzzleGeneric {

    bool[] lightStates;

    public override void GenerateBoard()
    {
        lightStates = new bool[16];
    }
    public override void CheckCurrentBoard()
    {
        puzzleSolved = !lightStates.Any(a => a);
    }
    public override void DisplayCurrentBoard()
    {
        base.DisplayCurrentBoard();
        for (var x = 0; x < 16; x++)
            usedRenderers[x].material.color = lightStates[x] ? Color.white : Color.black;
    }
    public override void ShuffleCurrentBoard()
    {
        var idxesToggled = new List<int>();
        // Randomly pick as many numbers from 1-16 inclusive.
        for (var x = 0; x < 16; x++)
        {
            if (Random.value < 0.5f)
                idxesToggled.Add(x);
        }
        for (var x = 0; x < 16; x++)
        {
            var idxX = x % 4;
            var idxY = x / 4;
            /*
            if (idxesToggled.Contains(x))
                lightStates[x] ^= true;
            if (idxX > 0 && idxesToggled.Contains(idxX - 1 + 4 * idxY))
                lightStates[x] ^= true;
            if (idxX < 3 && idxesToggled.Contains(idxX + 1 + 4 * idxY))
                lightStates[x] ^= true;
            if (idxY > 0 && idxesToggled.Contains(idxX - 4 + 4 * idxY))
                lightStates[x] ^= true;
            if (idxY < 3 && idxesToggled.Contains(idxX + 4 + 4 * idxY))
                lightStates[x] ^= true;
            */
            lightStates[x] = idxesToggled.Contains(x) ^
                (idxX > 0 && idxesToggled.Contains(idxX - 1 + 4 * idxY)) ^
                (idxX < 3 && idxesToggled.Contains(idxX + 1 + 4 * idxY)) ^
                (idxY > 0 && idxesToggled.Contains(idxX - 4 + 4 * idxY)) ^
                (idxY < 3 && idxesToggled.Contains(idxX + 4 + 4 * idxY)); // Simplification of a bunch of if conditions above.
        }
    }
    public override void HandleIdxPress(int idx)
    {
        if (idx < 0 || idx >= 16) return;

        var idxX = idx % 4;
        var idxY = idx / 4;

        lightStates[idx] ^= true;
        if (idxX > 0)
            lightStates[idxX - 1 + 4 * idxY] ^= true;
        if (idxX < 3)
            lightStates[idxX + 1 + 4 * idxY] ^= true;
        if (idxY > 0)
            lightStates[idxX - 4 + 4 * idxY] ^= true;
        if (idxY < 3)
            lightStates[idxX + 4 + 4 * idxY] ^= true;

    }

    public override IEnumerable<int> GetCurrentBoard()
    {
        return lightStates.Select(a => a ? 1 : 0);
    }
    public override IEnumerable<int> GetSolutionBoard()
    {
        return Enumerable.Repeat(0, 16);
    }

}
