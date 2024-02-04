using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoaboruPuzzleScript : MonoBehaviour {
	public KMSelectable[] gridSelectables, submitButtons;

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
		selectedTiles = new bool[10, 10];
		tileIdxesAll = new int[10, 10];

		var exampleBoard = (new string[]{  "OOOOO-O-OO", "-O-O-OOOOO" , "OOOOOO-OO-" , "OO-OO-OOOO", "-OO-OOO-O-" , "OO-OO-OO-O" , "OOOOOO-OOO", "O-O-OOOOOO" , "OO-OO-OO-O" , "OOOOOO-O-O" }).Select(a => a.Select(b => b == 'O').ToArray()).ToArray();
		Debug.LogFormat(exampleBoard.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));
		Debug.LogFormat(GetAllPossiblePlacements(exampleBoard).Select(a => string.Format("[{0}: {1}]", possiblePiecePlacements[a.Key].Select(b => b.Select(c => c ? "o" : "-").Join("")).Join(";"), string.Format("{0}{1}", alphabet[a.Value.Last()], a.Value.First() + 1))).Join(";"));
		Debug.LogFormat("{0} found",CountSolutions(exampleBoard));
		//var trimmedEmpty = TrimBoard(new bool[][] { new bool[10], new bool[10], new bool[10], new bool[10], new bool[10], new bool[10], new bool[10], new bool[10], new bool[10], new bool[10] });
		//Debug.LogFormat(trimmedEmpty.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));

	}

	void GeneratePuzzle()
    {

    }

	bool DoesPatternFitBoard(bool[][] _2Dboard, bool[][] pattern, int rIdx = 0, int cIdx = 0, bool invertCondition = false)
    {
		// Assume the 2D board and pattern are rectangular.
		// Basically, overlay the pattern on top of the 2D board, and check if the values would match with the pattern shown, skipping over blank tiles in patterns.
		var validFit = rIdx + pattern.Length <= _2Dboard.Length && cIdx + pattern[0].Length <= _2Dboard[0].Length;
		for (var dY = 0; dY < pattern.Length && validFit; dY++)
        {
			for (var dX = 0; dX < pattern[0].Length && validFit; dX++)
				validFit &= !pattern[dY][dX] || (_2Dboard[rIdx + dY][cIdx + dX] ^ invertCondition);
		}
		return validFit;
    }
	bool IsBoardPossibleTheoretical(bool[][] _2Dboard)
    {
		var visitedCoords = new List<int[]>();
		for (var row = 0; row < _2Dboard.Length; row++)
			for (var col = 0; col < _2Dboard[0].Length; col++)
            {
				var coordLoop = new[] { row, col };
				if (_2Dboard[row][col] && !visitedCoords.Any(a => a.SequenceEqual(coordLoop)))
                {
					var visitedCellsCurBatch = new List<int[]> { coordLoop };
					var visitedCellsCurGroup = new List<int[]>();
					while (visitedCellsCurBatch.Any())
                    {
						var newBatch = new List<int[]>();
						foreach (int[] coord in visitedCellsCurBatch)
                        {
							visitedCellsCurGroup.Add(coord);
							var offsetModifs = new int[][] { new[] { 0, 1 }, new[] { 0, -1 }, new[] { 1, 0 }, new[] { -1, 0 }, };
                            foreach (var offset in offsetModifs)
                            {
								var coordAfterOffset = new[] { coord[0] + offset[0], coord[1] + offset[1] };
								if (coordAfterOffset[0] >= 0 && coordAfterOffset[0] < _2Dboard.Length &&
									coordAfterOffset[1] >= 0 && coordAfterOffset[1] < _2Dboard[0].Length &&
									!newBatch.Any(a => a.SequenceEqual(coordAfterOffset)) && !visitedCellsCurGroup.Any(a => a.SequenceEqual(coordAfterOffset)))
									newBatch.Add(coordAfterOffset);
                            }
                        }
						visitedCellsCurBatch = newBatch;
                    }
					if (visitedCellsCurGroup.Count % 4 != 0)
						return false;
					else
						visitedCoords.AddRange(visitedCellsCurGroup);
				}
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
	int CountSolutions(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null, int idxStartSearch = 0)
    {
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		var countedSolutions = 0;
		for (var x = idxStartSearch; x < allPossiblePlacements.Count && countedSolutions < 2; x++)
        {
			var curPlacement = allPossiblePlacements[x];
			var new2DBoard = _2Dboard.Select(a => a.ToArray()).ToArray();
			var patternFromPlacement = possiblePiecePlacements[curPlacement.Key];
			var rCFromPlacement = curPlacement.Value;
			for (var r = 0; r < patternFromPlacement.Length; r++)
				for (var c = 0; c < patternFromPlacement[0].Length; c++)
					new2DBoard[rCFromPlacement[0] + r][rCFromPlacement[1] + c] ^= patternFromPlacement[r][c];
			if (TrimBoard(new2DBoard).Length == 0)
				countedSolutions++;
			else if (IsBoardPossibleTheoretical(new2DBoard))
			{ 
				var countedSolutionsRecursive = CountSolutions(new2DBoard, allPossiblePlacements, x + 1);
				countedSolutions += countedSolutionsRecursive;
			}
        }
		return countedSolutions;
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
		return validPlacements;
    }
}
