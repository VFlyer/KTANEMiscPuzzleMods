using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TangoPuzzle {
    int squareLength;
    public bool[][] solution;
    public bool[][] initialBoard;
    public int[][] idxHorizClues, idxVertClues;
    private List<bool[]> possibleCanditiatesPerLine;
    public TangoPuzzle(int pairCount = 3)
    {
        squareLength = pairCount * 2;
        // Set up the solution and initial board.
        solution = new bool[squareLength][];
        for (var x = 0; x < squareLength; x++)
            solution[x] = new bool[squareLength];
        initialBoard = new bool[squareLength][];
        for (var x = 0; x < squareLength; x++)
            solution[x] = new bool[squareLength];
        // Set up the horizontal/vertical clues.
        idxHorizClues = new int[squareLength][];
        for (var x = 0; x < squareLength; x++)
            idxHorizClues[x] = new int[squareLength - 1];
        idxVertClues = new int[squareLength - 1][];
        for (var x = 0; x < squareLength - 1; x++)
            idxVertClues[x] = new int[squareLength];
        possibleCanditiatesPerLine = GenerateCandidatesPerLine(pairCount);
        Debug.LogFormat(possibleCanditiatesPerLine.Select(a => a.Select(b => b ? "O" : "X").Join("")).Join(","));
    }
    public void GeneratePuzzle()
    {
        // Generate cell candidates for deductions.
        var cellCandidates = new List<bool>[squareLength][];
        for (var x = 0; x < squareLength; x++)
        {
            cellCandidates[x] = new List<bool>[squareLength];
            for (var y = 0; y < squareLength; y++)
            {
                cellCandidates[x][y] = new List<bool>();
                cellCandidates[x][y].AddRange(new[] { true, false });
            }
        }
        // Generate line candidates for deductions.
    }
    List<bool[]> GenerateCandidatesPerLine(int pairCount = 3)
    {
        var curCombo = new List<int[]> { new int[0] };
        for (var x = 0; x < pairCount; x++)
        {
            var nextCombo = new List<int[]>();
            foreach (var combo in curCombo)
            {
                var possibleItems = Enumerable.Range(0, squareLength).Where(a => !combo.Any() || a > combo.Max());
                foreach (var item in possibleItems)
                    nextCombo.Add(combo.Concat(new[] { item }).ToArray());
            }
            curCombo = nextCombo;
        }
        return curCombo.Select(a => Enumerable.Range(0, squareLength).Select(b => a.Contains(b)).ToArray())
            .Where(a => !Enumerable.Range(0,squareLength - 3).Any(b => a.Skip(b).Take(3).Distinct().Count() == 1)).ToList();
    }

}
