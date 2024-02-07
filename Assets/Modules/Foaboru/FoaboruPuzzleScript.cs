using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoaboruPuzzleScript : MonoBehaviour {
	public KMSelectable[] gridSelectables, submitButtons;
	public KMBombModule modSelf;
	public KMAudio mAudio;
	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	bool[][][] possiblePiecePlacements = new string[][] { // A list of all possible pieces and their rotations.
		// Rows are denoted by the amount of strings in each, columns are denoted by the length of each string.
		// I
		new[] { "OOOO" },
		new[] { "O","O","O","O" },
		// O
		new[] { "OO","OO" },
		// T
		new[] { "OOO","-O-" },
		new[] { "-O-","OOO" },
		new[] { "O-","OO","O-" },
		new[] { "-O","OO","-O" },
		// J
		new[] { "OOO","--O" },
		new[] { "O--","OOO" },
		new[] { "OO","O-","O-" },
		new[] { "-O","-O","OO" },
		// L
		new[] { "OOO","O--" },
		new[] { "--O","OOO" },
		new[] { "OO","-O","-O" },
		new[] { "O-","O-","OO" },
		// S
		new[] { "-OO","OO-" },
		new[] { "-O","OO","O-" },
		// Z
		new[] { "OO-","-OO" },
		new[] { "O-","OO","-O" },
	}.Select(a => a.Select(b => b.Select(c => c == 'O').ToArray()).ToArray()).ToArray();
	// This is all converted by using the symbols denoted for filled and into a series of bool arrays. In this case, O is filled, - is empty.

	int[,] tileIdxesAll;
	bool[,] selectedTiles;

	// Use this for initialization
	void Start () {
		selectedTiles = new bool[8, 8];
		tileIdxesAll = new int[8, 8];

		var exampleBoard = (new string[]{
			"O-O-OOOO","OOOOOO-O", "OOOOOOOO", "-O-O-O--","O-OOOOOO", "OOO-OOO-", "-OOOO-OO","OO--OOO-", "--------", "--------"
			//"OOOOO-O-OO", "-O-O-OOOOO", "OOOOOO-OO-", "OO-OO-OOOO", "-OO-OOO-O-", "OO-OO-OO-O", "OOOOOO-OOO", "O-O-OOOOOO", "OO-OO-OO-O", "OOOOOO-O-O"
			//"OOOO","-OO-","-OO-"
		}).Select(a => a.Select(b => b == 'O').ToArray()).ToArray();
		Debug.LogFormat(exampleBoard.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));
		var trimmedExample = TrimBoard(exampleBoard);
		Debug.LogFormat(trimmedExample.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));
		var exampleAllPossiblePlacements = GetAllPossiblePlacements(trimmedExample);
		Debug.LogFormat("{0}",exampleAllPossiblePlacements.Count);
		Debug.LogFormat(exampleAllPossiblePlacements.Select(a => string.Format("[{0}: {1}]", possiblePiecePlacements[a.Key].Select(b => b.Select(c => c ? "o" : "-").Join("")).Join(";"), string.Format("{0}{1}", alphabet[a.Value.Last()], a.Value.First() + 1))).Join(";"));
		Debug.LogFormat("Solution? {0}", IsBoardPossible(trimmedExample));
		var solutionCount = CountSolutions(trimmedExample);
		Debug.LogFormat("Solution Type? {0}", new[] { "Impossible", "Unique", "Ambiguous" }[solutionCount]);
		
		GeneratePuzzle();
	}

	void GeneratePuzzle()
    {

    }

	bool DoesPatternFitBoard(bool[][] _2Dboard, bool[][] pattern, int rIdx = 0, int cIdx = 0, bool checkEmptyVsFilled = false)
    {
		// Assume the 2D board and pattern are rectangular.
		// Basically, overlay the pattern on top of the 2D board, and check if the values would match with the pattern shown, skipping over blank tiles in patterns.
		var validFit = rIdx + pattern.Length <= _2Dboard.Length && cIdx + pattern[0].Length <= _2Dboard[0].Length;
		for (var dY = 0; dY < pattern.Length && validFit; dY++)
        {
			for (var dX = 0; dX < pattern[0].Length && validFit; dX++)
				validFit &= !pattern[dY][dX] || (_2Dboard[rIdx + dY][cIdx + dX] ^ checkEmptyVsFilled);
		}
		return validFit;
    }
	bool IsBoardPossibleTheoretical(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
    {
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		var visitedCoords = new List<int[]>();
		foreach (var keyPlacement in allPossiblePlacements)
        {
			var startCoord = keyPlacement.Value;
			var pattern = possiblePiecePlacements[keyPlacement.Key];
			for (var rowPat = 0; rowPat < pattern.Length; rowPat++)
				for (var colPat = 0; colPat < pattern[0].Length; colPat++)
					if (pattern[rowPat][colPat])
					{
						var coordAfterShift = new[] { startCoord[0] + rowPat, startCoord[1] + colPat };
						if (!visitedCoords.Any(a => a.SequenceEqual(coordAfterShift)))
						visitedCoords.Add(coordAfterShift);
					}
		}

		for (var row = 0; row < _2Dboard.Length; row++)
			for (var col = 0; col < _2Dboard[0].Length; col++)
            {
				if (_2Dboard[row][col] && !visitedCoords.Any(a => a[0] == row && a[1] == col))
					return false;
            }
		return true;
	}

	bool[][] TrimBoard(bool[][] _2Dboard)
    {
		if (_2Dboard.Length == 0) return _2Dboard;
		var idxNonEmptyRows = Enumerable.Range(0, _2Dboard.Length).Where(a => _2Dboard[a].Any(b => b));
		var idxNonEmptyCols = Enumerable.Range(0, _2Dboard[0].Length).Where(a => _2Dboard.Select(b => b[a]).Any(b => b));
		if (!idxNonEmptyRows.Any() || !idxNonEmptyCols.Any()) return new bool[0][];
		var minRowIdxNonEmpty = idxNonEmptyRows.Min();
		var maxRowIdxNonEmpty = idxNonEmptyRows.Max();
		var minColIdxNonEmpty = idxNonEmptyCols.Min();
		var maxColIdxNonEmpty = idxNonEmptyCols.Max();

		var newGridRowCnt = 1 + maxRowIdxNonEmpty - minRowIdxNonEmpty;
		var newGridColCnt = 1 + maxColIdxNonEmpty - minColIdxNonEmpty;

		var newGrid = new bool[newGridRowCnt][];
		for (var rowIdx = 0; rowIdx < newGridRowCnt; rowIdx++)
        {
			var newRow = new bool[newGridColCnt];
			for (var colIdx = 0; colIdx < newGridColCnt; colIdx++)
				newRow[colIdx] = _2Dboard[rowIdx + minRowIdxNonEmpty][colIdx + minColIdxNonEmpty];
			newGrid[rowIdx] = newRow;
        }
		return newGrid;
    }
	int CountSolutions(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
    {
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		var countedSolutions = 0;
		for (var x = 0; x < allPossiblePlacements.Count && countedSolutions < 2; x++)
        {
			var curPlacement = allPossiblePlacements[x];
			var new2DBoard = _2Dboard.Select(a => a.ToArray()).ToArray();
			var patternFromPlacement = possiblePiecePlacements[curPlacement.Key];
			var rCFromPlacement = curPlacement.Value;
			for (var r = 0; r < patternFromPlacement.Length; r++)
				for (var c = 0; c < patternFromPlacement[0].Length; c++)
					new2DBoard[rCFromPlacement[0] + r][rCFromPlacement[1] + c] ^= patternFromPlacement[r][c];
			var newAllowedPlacements = allPossiblePlacements.Skip(x + 1)
				.Where(a => DoesPatternFitBoard(new2DBoard, possiblePiecePlacements[a.Key], a.Value[0], a.Value[1])).ToList();
			if (!new2DBoard.Any(a => a.Any(b => b)))
				return 1;
			else if (newAllowedPlacements.Any() && IsBoardPossibleTheoretical(new2DBoard, newAllowedPlacements))
				countedSolutions += CountSolutions(new2DBoard, newAllowedPlacements);
        }
		return countedSolutions;
    }
	bool IsBoardPossible(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
    {
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		var foundSolution = false;
		for (var x = 0; x < allPossiblePlacements.Count && !foundSolution; x++)
        {
			var curPlacement = allPossiblePlacements[x];
			var new2DBoard = _2Dboard.Select(a => a.ToArray()).ToArray();
			var patternFromPlacement = possiblePiecePlacements[curPlacement.Key];
			var rCFromPlacement = curPlacement.Value;
			for (var r = 0; r < patternFromPlacement.Length; r++)
				for (var c = 0; c < patternFromPlacement[0].Length; c++)
					new2DBoard[rCFromPlacement[0] + r][rCFromPlacement[1] + c] ^= patternFromPlacement[r][c];
			var newAllowedPlacements = allPossiblePlacements.Skip(x + 1)
				.Where(a => DoesPatternFitBoard(new2DBoard, possiblePiecePlacements[a.Key], a.Value[0], a.Value[1])).ToList();
			if (!new2DBoard.Any(a => a.Any(b => b)))
				return true;
			else if (newAllowedPlacements.Any() && IsBoardPossibleTheoretical(new2DBoard, newAllowedPlacements))
				foundSolution |= IsBoardPossible(new2DBoard, newAllowedPlacements);
        }
		return foundSolution;
    }
	List<KeyValuePair<int, int[]>> GetAllPossiblePlacements(bool[][] _2Dboard, int startScanIdx = 0)
    {
		var trimmedBoard = TrimBoard(_2Dboard);
		//Debug.Log(trimmedBoard.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));
		var validPlacements = new List<KeyValuePair<int, int[]>>();
		for (var pieceIdx = startScanIdx; pieceIdx < possiblePiecePlacements.Length; pieceIdx++)
		{
			var curPattern = possiblePiecePlacements[pieceIdx];
			for (var rIdx = 0; rIdx <= trimmedBoard.Length - curPattern.Length; rIdx++)
				for (var cIdx = 0; cIdx <= trimmedBoard[0].Length - curPattern[0].Length; cIdx++) // Assume the 2D board is rectangular.
					if (DoesPatternFitBoard(trimmedBoard, curPattern, rIdx, cIdx))
						validPlacements.Add(new KeyValuePair<int, int[]>(pieceIdx, new[] { rIdx, cIdx } ));
		}
		return validPlacements; // Valid placements consist of a piece IDx, and a row,col coordinate.
    }
}
