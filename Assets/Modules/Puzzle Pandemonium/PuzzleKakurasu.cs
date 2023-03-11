using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleKakurasu : PuzzleGeneric {

	public TextMesh[] usedTextMeshes;
    bool[] solutionBoard, currentBoard;
    int[] sumsRows, sumsCols;

    public override void GenerateBoard()
    {
        solutionBoard = new bool[16];
        currentBoard = new bool[16];
        do
            for (var x = 0; x < 16; x++)
                solutionBoard[x] = Random.value < 0.5f;
        while (
        Enumerable.Range(0, 4).Any(a =>
        Enumerable.Range(0, 4).Select(b => solutionBoard[b * 4 + a]).Distinct().Count() == 1 ||
        Enumerable.Range(0, 4).Select(b => solutionBoard[a * 4 + b]).Distinct().Count() == 1));

        sumsCols = new int[4];
        sumsRows = new int[4];

        /* 
         * Diagram for layout:
         * 00 01 02 03
         * 04 05 06 07
         * 08 09 10 11
         * 12 13 14 15
         * 
         */
        for (var x = 0; x < 4; x++)
        {
            sumsCols[x] = Enumerable.Range(0, 4).Sum(a => solutionBoard[x + 4 * a] ? a + 1 : 0);
            sumsRows[x] = Enumerable.Range(0, 4).Sum(a => solutionBoard[a + 4 * x] ? a + 1 : 0);
        }

    }

    public override void ShuffleCurrentBoard()
    {
        for (var x = 0; x < 16; x++)
            currentBoard[x] = Random.value < 0.5f;
    }

    public override bool CheckCurrentBoard()
    {
        puzzleSolved = true;
        for (var x = 0; x < 4; x++)
        {
            puzzleSolved &= sumsCols[x] == Enumerable.Range(0, 4).Sum(a => currentBoard[x + 4 * a] ? a + 1 : 0) && sumsRows[x] == Enumerable.Range(0, 4).Sum(a => currentBoard[a + 4 * x] ? a + 1 : 0);
        }
        return base.CheckCurrentBoard();
    }

    public override IEnumerable<int> GetCurrentBoard()
    {
        return currentBoard.Select(a => a ? 1 : 0);
    }
    public override IEnumerable<int> GetSolutionBoard()
    {
        return solutionBoard.Select(a => a ? 1 : 0);
    }

    public override void DisplayCurrentBoard()
    {
        base.DisplayCurrentBoard();
        var nestedArraySums = sumsRows.Concat(sumsCols);
        for (var x = 0; x < usedTextMeshes.Length; x++)
            usedTextMeshes[x].text = nestedArraySums.ElementAt(x).ToString();
        for (var x = 0; x < 16; x++)
            usedRenderers[x].material.color = currentBoard[x] ? Color.black : Color.gray;
    }
    public override void HandleIdxPress(int idx)
    {
        if (idx < 0 || idx >= 16) return;
        currentBoard[idx] ^= true;
    }
}
