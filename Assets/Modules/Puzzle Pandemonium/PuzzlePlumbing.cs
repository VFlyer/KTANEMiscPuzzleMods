using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzlePlumbing : PuzzleGeneric {

    enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        None = -1,
    }

    bool[][] accessibleDirections, solvedDirections;
    int startingIdx;

    public override void GenerateBoard()
    {
        startingIdx = Random.Range(0, 16);
        accessibleDirections = new bool[16][];
        for (var x = 0; x < 16; x++)
            accessibleDirections[x] = new bool[4];

        // Generate a path corresponding to the solution board. The procedure used is a growing tree algorithm.
        var visitedTiles = new List<int> { startingIdx };
        var searchingTiles = new List<int> { startingIdx };

        while (searchingTiles.Any())
        {
            var curTileSearch = searchingTiles.PickRandom();
            if (!visitedTiles.Contains(curTileSearch))
                visitedTiles.Add(curTileSearch);
            var possibleDirectionsVisit = new List<Direction>();
            var tileSearchIdxX = curTileSearch % 4;
            var tileSearchIdxY = curTileSearch / 4;
            if (tileSearchIdxY > 0 && !visitedTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY - 1)) && !searchingTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY - 1)))
                possibleDirectionsVisit.Add(Direction.Up);
            if (tileSearchIdxY < 3 && !visitedTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY + 1)) && !searchingTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY + 1)))
                possibleDirectionsVisit.Add(Direction.Down);
            if (tileSearchIdxX > 0 && !visitedTiles.Contains(tileSearchIdxX - 1 + 4 * tileSearchIdxY) && !searchingTiles.Contains(tileSearchIdxX - 1 + 4 * tileSearchIdxY))
                possibleDirectionsVisit.Add(Direction.Left);
            if (tileSearchIdxX < 3 && !visitedTiles.Contains(tileSearchIdxX + 1 + 4 * tileSearchIdxY) && !searchingTiles.Contains(tileSearchIdxX + 1 + 4 * tileSearchIdxY))
                possibleDirectionsVisit.Add(Direction.Right);
            foreach (var dir in possibleDirectionsVisit)
                switch (dir)
                {
                    case Direction.Up:
                        searchingTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY - 1));
                        break;
                    case Direction.Right:
                        searchingTiles.Add(tileSearchIdxX + 1 + 4 * tileSearchIdxY);
                        break;
                    case Direction.Down:
                        searchingTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY + 1));
                        break;
                    case Direction.Left:
                        searchingTiles.Add(tileSearchIdxX - 1 + 4 * tileSearchIdxY);
                        break;
                }
            // Scan for possible tiles to connect.
            var possibleDirectionsConnect = new List<Direction>();
            if (tileSearchIdxY > 0 && visitedTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY - 1)))
                possibleDirectionsConnect.Add(Direction.Up);
            if (tileSearchIdxY < 3 && visitedTiles.Contains(tileSearchIdxX + 4 * (tileSearchIdxY + 1)))
                possibleDirectionsConnect.Add(Direction.Down);
            if (tileSearchIdxX > 0 && visitedTiles.Contains(tileSearchIdxX - 1 + 4 * tileSearchIdxY))
                possibleDirectionsConnect.Add(Direction.Left);
            if (tileSearchIdxX < 3 && visitedTiles.Contains(tileSearchIdxX + 1 + 4 * tileSearchIdxY))
                possibleDirectionsConnect.Add(Direction.Right);
            if (possibleDirectionsConnect.Any())
                switch (possibleDirectionsConnect.PickRandom())
                {
                    case Direction.Up:
                        accessibleDirections[curTileSearch][0] = true;
                        accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY - 1)][2] = true;
                        break;
                    case Direction.Right:
                        accessibleDirections[curTileSearch][1] = true;
                        accessibleDirections[tileSearchIdxX + 1 + 4 * tileSearchIdxY][3] = true;
                        break;
                    case Direction.Down:
                        accessibleDirections[curTileSearch][2] = true;
                        accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY + 1)][0] = true;
                        break;
                    case Direction.Left:
                        accessibleDirections[curTileSearch][3] = true;
                        accessibleDirections[tileSearchIdxX - 1 + 4 * tileSearchIdxY][1] = true;
                        break;
                }
            searchingTiles.Remove(curTileSearch);
        }
        solvedDirections = accessibleDirections.Select(a => a.ToArray()).ToArray();

    }

    public override void DisplayCurrentBoard()
    {
        base.DisplayCurrentBoard();
        /* Diagram for a single tile:
         *   0
         * 3 4 1
         *   2
         * 
         * 0-3: pipes corresponding to up, and going clockwise
         * 4: the center piece.
         */

        var filledTiles = new List<int>();
        var scanningTiles = new List<int> { startingIdx };
        // Perform a flood fill corresponding to if the items are connected in both directions.

        while (scanningTiles.Any())
        {
            var nextScanningTiles = new List<int>();
            foreach (var tile in scanningTiles)
            {
                if (!filledTiles.Contains(tile))
                    filledTiles.Add(tile);

                var tileSearchIdxX = tile % 4;
                var tileSearchIdxY = tile / 4;
                if (tileSearchIdxY > 0 && accessibleDirections[tile][0] && accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY - 1)][2])
                    nextScanningTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY - 1));
                if (tileSearchIdxY < 3 && accessibleDirections[tile][2] && accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY + 1)][0])
                    nextScanningTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY + 1));
                if (tileSearchIdxX > 0 && accessibleDirections[tile][3] && accessibleDirections[tileSearchIdxX - 1 + 4 * tileSearchIdxY][1])
                    nextScanningTiles.Add(tileSearchIdxX - 1 + 4 * tileSearchIdxY);
                if (tileSearchIdxX < 3 && accessibleDirections[tile][1] && accessibleDirections[tileSearchIdxX + 1 + 4 * tileSearchIdxY][3])
                    nextScanningTiles.Add(tileSearchIdxX + 1 + 4 * tileSearchIdxY);
                
            }
            scanningTiles.Clear();
            if (nextScanningTiles.Any())
                scanningTiles.AddRange(nextScanningTiles.Distinct().Where(a => !filledTiles.Contains(a)));
        }
        
        // 
        for (var x = 0; x < 16; x++)
        {
            for (var p = 0; p < 4; p++)
            {
                usedRenderers[5 * x + p].enabled = accessibleDirections[x][p];
            }
            usedRenderers[5 * x + 4].material.color = x == startingIdx ? Color.yellow : filledTiles.Contains(x) ? Color.white : Color.black;

        }

        

    }

    public override void ShuffleCurrentBoard()
    {
        for (var x = 0; x < 16; x++)
        {
            var randomAmount = Random.Range(0, 4);
            accessibleDirections[x] = accessibleDirections[x].Skip(randomAmount).Concat(accessibleDirections[x].Take(randomAmount)).ToArray();
        }
    }
    public override bool CheckCurrentBoard()
    {
        var filledTiles = new List<int>();
        var scanningTiles = new List<int> { startingIdx };
        // Perform a flood fill corresponding to if the items are connected in both directions.

        while (scanningTiles.Any())
        {
            var nextScanningTiles = new List<int>();
            foreach (var tile in scanningTiles)
            {
                if (!filledTiles.Contains(tile))
                    filledTiles.Add(tile);

                var tileSearchIdxX = tile % 4;
                var tileSearchIdxY = tile / 4;
                if (tileSearchIdxY > 0 && accessibleDirections[tile][0] && accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY - 1)][2])
                    nextScanningTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY - 1));
                if (tileSearchIdxY < 3 && accessibleDirections[tile][2] && accessibleDirections[tileSearchIdxX + 4 * (tileSearchIdxY + 1)][0])
                    nextScanningTiles.Add(tileSearchIdxX + 4 * (tileSearchIdxY + 1));
                if (tileSearchIdxX > 0 && accessibleDirections[tile][3] && accessibleDirections[tileSearchIdxX - 1 + 4 * tileSearchIdxY][1])
                    nextScanningTiles.Add(tileSearchIdxX - 1 + 4 * tileSearchIdxY);
                if (tileSearchIdxX < 3 && accessibleDirections[tile][1] && accessibleDirections[tileSearchIdxX + 1 + 4 * tileSearchIdxY][3])
                    nextScanningTiles.Add(tileSearchIdxX + 1 + 4 * tileSearchIdxY);

            }
            scanningTiles.Clear();
            if (nextScanningTiles.Any())
                scanningTiles.AddRange(nextScanningTiles.Distinct().Where(a => !filledTiles.Contains(a)));
        }

        puzzleSolved = filledTiles.Count >= 16;
        return base.CheckCurrentBoard();
    }

    public override void HandleIdxPress(int idx)
    {
        if (idx < 0 || idx >= 16) return;
        accessibleDirections[idx] = accessibleDirections[idx].Skip(3).Concat(accessibleDirections[idx].Take(3)).ToArray();
    }

    public override IEnumerable<int> GetCurrentBoard()
    {
        return accessibleDirections.Select(a => Enumerable.Range(0, 4).Reverse().Sum(b => (a[b] ? 1 : 0) << b));
    }

    public override IEnumerable<int> GetSolutionBoard()
    {
        return solvedDirections.Select(a => Enumerable.Range(0, 4).Reverse().Sum(b => (a[b] ? 1 : 0) << b));
    }
    public override void MimicLogBoard(string formatString, bool logSolutionBoard = false)
    {
        var loggingRefPipes = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        for (var x = 0; x < 4; x++)
        {
            Debug.LogFormat(formatString, accessibleDirections.Skip(4 * x).Take(4).Select(a => loggingRefPipes[Enumerable.Range(0, 4).Select(b => (a[b] ? 1 : 0) << b).Sum()]).Join());
        }
    }
}
