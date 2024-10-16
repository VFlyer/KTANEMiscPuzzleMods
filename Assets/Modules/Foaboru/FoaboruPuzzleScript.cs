using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;

public class FoaboruPuzzleScript : MonoBehaviour {
	public KMSelectable[] gridSelectables, submitButtons;
	public MeshRenderer[] gridRenders;
	public KMBombModule modSelf;
	public KMAudio mAudio;
	public AudioClip referTrackLong, referTrackShort;
	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	const int rowCount = 8, colCount = 8;

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
	// This is all converted by using the symbols denoted for filled and into a series of bool arrays.
	// In this case, O is filled, - is empty.

	static Color[] colorsRender = new[] { Color.black, Color.grey, Color.white },
		solveColorsRender = new[] { new Color(0, 0.25f, 0), new Color(0, 0.5f, 0), new Color(0, 1, 0), Color.white };

	static int modIDCnt;
	int moduleID;

	int[,] tileIdxesAll;
	bool[,] selectedTiles;
	static List<KeyValuePair<int, int[]>> allPossiblePlacementsGivenBoard;
	bool checkAmbiguityOnGen = false, interactable = false, moduleSolved;

	FlyersPuzzleSettings.SubmissionType forcedSubmission = FlyersPuzzleSettings.SubmissionType.NA;

	FlyersPuzzleSettings puzzleSettings;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }
	void Awake()
    {
		try
		{
			var ModSettings = new ModConfig<FlyersPuzzleSettings>("FlyersMiscPuzzlesSettings");
			puzzleSettings = ModSettings.Settings;
			forcedSubmission = puzzleSettings.FoaboruForceSubmissionType;
		}
		catch
        {
			forcedSubmission = FlyersPuzzleSettings.SubmissionType.NA;
        }
    }
	// Use this for initialization
	void Start () {
		// Entire section for generating an example puzzle.
		/*
		var exampleBoard = (new string[]{
			//"O-O-OOOO","OOOOOO-O", "OOOOOOOO", "-O-O-O--","O-OOOOOO", "OOO-OOO-", "-OOOO-OO","OO--OOO-", "--------", "--------"
			//"OOOOO-O-OO", "-O-O-OOOOO", "OOOOOO-OO-", "OO-OO-OOOO", "-OO-OOO-O-", "OO-OO-OO-O", "OOOOOO-OOO", "O-O-OOOOOO", "OO-OO-OO-O", "OOOOOO-O-O"
			"OOO-","-O-O","OOOO","O-OO"
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
		*/
		moduleID = ++modIDCnt;
		selectedTiles = new bool[rowCount, colCount];
		tileIdxesAll = new int[rowCount, colCount];
		GeneratePuzzle();
        for (var x = 0; x < gridSelectables.Length; x++)
        {
			var y = x;
			gridSelectables[x].OnInteract += delegate {
				if (interactable && !moduleSolved)
					HandleBtnPress(y);
				return false;
			};
        }
        for (var x = 0; x < submitButtons.Length; x++)
        {
			var y = x;
			submitButtons[x].OnInteract += delegate {
				if (interactable && !moduleSolved)
					HandleSubmitPress(y);
				return false;
			};
        }
	}
	IEnumerator SubmitAnimationC()
    {
        var idxDepthAnim = new int[][] {
			new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 15, 16, 23, 24, 31, 32, 39, 40, 47, 48, 55, 56, 57, 58, 59, 60, 61, 62, 63 },
            new[] { 9, 10, 11, 12, 13, 14,  17, 22,  25, 30,  33, 38,  41, 46,  49, 50, 51, 52, 53, 54 },
            new[] { 18, 19, 20, 21,  26, 29,  34, 37,  42, 43, 44, 45 },
            new[] { 27, 28, 35, 36 },
		};
		yield return null;
		mAudio.PlaySoundAtTransform(referTrackShort.name, transform);
		for (var x = 0; x < 4; x++)
		{
			for (float t = 0; t < 1f; t += Time.deltaTime * 4 / referTrackShort.length)
			{
				foreach (var n in idxDepthAnim[x])
					gridRenders[n].material.color = Color.Lerp(Color.white, Color.black, t);
				yield return null;
			}
			foreach (var n in idxDepthAnim[x])
				gridRenders[n].material.color = Color.black;
		}
		QuickLog("Submitted:");
		for (var row = 0; row < rowCount; row++)
        {
			var strLog = "";
			for (var col = 0; col < colCount; col++)
				strLog += selectedTiles[row, col] ? "KAW?"[tileIdxesAll[row, col]] : '-';
			QuickLog(strLog);
        }
		if (SolutionValid())
        {
			QuickLog("Submission valid.");
			mAudio.PlaySoundAtTransform("musCEnd", transform);
			modSelf.HandlePass();
			moduleSolved = true;
			for (float t = 0; t < 1f; t += Time.deltaTime / 4)
			{
				for (var n = 0; n < gridRenders.Length; n++)
				{
					var rowIdx = n / colCount;
					var colIdx = n % colCount;
					//Debug.LogFormat("{0}:{1}{2}", n, rowIdx, colIdx);
					gridRenders[n].material.color = Color.Lerp(Color.green, solveColorsRender[tileIdxesAll[rowIdx, colIdx]], Easing.InOutSine(t, 0, 1f, 1f));
				}
				yield return null;
			}
			for (var n = 0; n < gridRenders.Length; n++)
			{
				var rowIdx = n / colCount;
				var colIdx = n % colCount;
				gridRenders[n].material.color = solveColorsRender[tileIdxesAll[rowIdx, colIdx]];
			}
		}
        else
        {
			QuickLog("Invalid submission. Resetting...");
			modSelf.HandleStrike();
			GeneratePuzzle();
        }
	}
	bool SolutionValid()
    {
        var uncheckedIdxes = Enumerable.Range(0, colCount * rowCount).Where(a => selectedTiles[a / colCount, a % colCount]).ToList();
		var groupIdxes = new List<List<int>>();
		while (uncheckedIdxes.Any())
        {
			var firstIdxUnchecked = uncheckedIdxes.First();
			var targetColorIdx = tileIdxesAll[firstIdxUnchecked / colCount, firstIdxUnchecked % colCount];
			var foundCells = new List<int>();
			var curScanCells = new List<int> { firstIdxUnchecked };
			while (curScanCells.Any())
            {
				var nextScanCells = new List<int>();
				foreach (var curCell in curScanCells)
                {
					foundCells.Add(curCell);
					var colIdxCurCell = curCell % colCount;
					var rowIdxCurCell = curCell / colCount;
					if (colIdxCurCell + 1 < colCount && uncheckedIdxes.Contains(colIdxCurCell + 1 + rowIdxCurCell * colCount) && !foundCells.Contains(colIdxCurCell + 1 + rowIdxCurCell * colCount) && tileIdxesAll[rowIdxCurCell, colIdxCurCell + 1] == targetColorIdx)
						nextScanCells.Add(colIdxCurCell + 1 + rowIdxCurCell * colCount);
					if (colIdxCurCell > 0 && uncheckedIdxes.Contains(colIdxCurCell - 1 + rowIdxCurCell * colCount) && !foundCells.Contains(colIdxCurCell - 1 + rowIdxCurCell * colCount) && tileIdxesAll[rowIdxCurCell, colIdxCurCell - 1] == targetColorIdx)
						nextScanCells.Add(colIdxCurCell - 1 + rowIdxCurCell * colCount);
					if (rowIdxCurCell + 1 < rowCount && uncheckedIdxes.Contains(colIdxCurCell + colCount + rowIdxCurCell * colCount) && !foundCells.Contains(colIdxCurCell + colCount + rowIdxCurCell * colCount) && tileIdxesAll[rowIdxCurCell + 1, colIdxCurCell] == targetColorIdx)
						nextScanCells.Add(colIdxCurCell + colCount + rowIdxCurCell * colCount);
					if (rowIdxCurCell > 0 && uncheckedIdxes.Contains(colIdxCurCell - colCount + rowIdxCurCell * colCount) && !foundCells.Contains(colIdxCurCell - colCount + rowIdxCurCell * colCount) && tileIdxesAll[rowIdxCurCell - 1, colIdxCurCell] == targetColorIdx)
						nextScanCells.Add(colIdxCurCell - colCount + rowIdxCurCell * colCount);
                }
				curScanCells = nextScanCells.Distinct().ToList();
            }
			groupIdxes.Add(foundCells);
			uncheckedIdxes.RemoveAll(a => foundCells.Contains(a));
        }
		foreach (var invalidGroup in groupIdxes.Where(a => a.Count != 4))
			QuickLog("Detected invalid group: {0}", invalidGroup.Select(a => string.Format("{0}{1}", alphabet[a % colCount], a / colCount + 1)).Join(","));
		return !groupIdxes.Any() || groupIdxes.All(a => a.Count == 4);
    }

	void HandleSubmitPress(int idx)
    {
		interactable = false;
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitButtons[idx].transform);
		var curSubmissionIdx = ((int)forcedSubmission) <= 0 ? idx : ((int)forcedSubmission - 1);
		switch (curSubmissionIdx)
        {
			case 1:
				StartCoroutine(SubmitAnimationC());
				break;
			case 2:
			default:
                {
					QuickLog("Submitted:");
					for (var row = 0; row < rowCount; row++)
					{
						var strLog = "";
						for (var col = 0; col < colCount; col++)
							strLog += selectedTiles[row, col] ? "KAW?"[tileIdxesAll[row, col]] : '-';
						QuickLog(strLog);
					}
					if (SolutionValid())
                    {
						interactable = false;
						QuickLog("Submission valid.");
						mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						modSelf.HandlePass();
						moduleSolved = true;
						for (var n = 0; n < gridRenders.Length; n++)
						{
							var rowIdx = n / colCount;
							var colIdx = n % colCount;
							gridRenders[n].material.color = solveColorsRender[tileIdxesAll[rowIdx, colIdx]];
						}
						return;
					}
					else
					{
						QuickLog("Invalid submission. Resetting...");
						modSelf.HandleStrike();
						GeneratePuzzle();
					}
				}
				break;
        }
    }
	void HandleBtnPress(int idx)
    {
		var rowIdx = idx / colCount;
		var colIdx = idx % colCount;
		if (selectedTiles[rowIdx, colIdx])
		{
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, gridSelectables[idx].transform);
			tileIdxesAll[rowIdx, colIdx] = (tileIdxesAll[rowIdx, colIdx] + 1) % 3;
			UpdatePuzzle();
		}
    }
	void GeneratePuzzle()
    {
		var newBoard = new bool[rowCount][];
		for (var x = 0; x < rowCount; x++)
			newBoard[x] = new bool[colCount];
		var boardColorIdx = new int[rowCount][];
		for (var x = 0; x < rowCount; x++)
			boardColorIdx[x] = new int[colCount];
		allPossiblePlacementsGivenBoard = GetAllPossiblePlacements(newBoard, true);
		//Debug.LogFormat(allPossiblePlacementsGivenBoard.Select(a => string.Format("[{0}: {1}]", possiblePiecePlacements[a.Key].Select(b => b.Select(c => c ? "o" : "-").Join("")).Join(";"), string.Format("{0}{1}", alphabet[a.Value.Last()], a.Value.First() + 1))).Join(";"));
		var idxesShuffled = Enumerable.Range(0, allPossiblePlacementsGivenBoard.Count).ToList();
		var firstPlacementIdx = idxesShuffled.PickRandom();
		idxesShuffled.Remove(firstPlacementIdx);

		var firstPlacementCoord = allPossiblePlacementsGivenBoard[firstPlacementIdx].Value;
		var firstPlacementPiece = possiblePiecePlacements[allPossiblePlacementsGivenBoard[firstPlacementIdx].Key];
		var rowIdxFirst = firstPlacementCoord[0];
		var colIdxFirst = firstPlacementCoord[1];
        for (var deltaR = 0; deltaR < firstPlacementPiece.Length; deltaR++)
			for (var deltaC = 0; deltaC < firstPlacementPiece[deltaR].Length; deltaC++)
				if (firstPlacementPiece[deltaR][deltaC])
				{
                    newBoard[rowIdxFirst + deltaR][colIdxFirst + deltaC] = true;
                    boardColorIdx[rowIdxFirst + deltaR][colIdxFirst + deltaC] = 1;
				}
		idxesShuffled.Shuffle();
		while (idxesShuffled.Any())
        {
			var nextIdx = idxesShuffled.PickRandom();
			var removeOption = false;
			var curSetCoord = allPossiblePlacementsGivenBoard[nextIdx].Value;
			var curSetPiece = possiblePiecePlacements[allPossiblePlacementsGivenBoard[nextIdx].Key];
			var curSetRowIdx = curSetCoord[0];
			var curSetColIdx = curSetCoord[1];
			// First check: 
			// If the piece overlaps with another piece on the field, remove it.
			for (var dR = 0; dR < curSetPiece.Length && !removeOption; dR++)
				for (var dC = 0; dC < curSetPiece[dR].Length && !removeOption; dC++)
					removeOption |= newBoard[dR + curSetRowIdx][dC + curSetColIdx] && curSetPiece[dR][dC];
			// Second check (to optimize later on):
			// If the placed piece does not allow a unique solution, remove it.
			if (checkAmbiguityOnGen && !removeOption)
            {
				var theoreticalBoard = newBoard.Select(a => a.ToArray()).ToArray();
				for (var dR = 0; dR < curSetPiece.Length && !removeOption; dR++)
					for (var dC = 0; dC < curSetPiece[dR].Length && !removeOption; dC++)
						theoreticalBoard[dR + curSetRowIdx][dC + curSetColIdx] |= curSetPiece[dR][dC];
				removeOption |= CountSolutions(theoreticalBoard) > 1;
			}
			if (removeOption)
			{
				idxesShuffled.Remove(nextIdx);
				continue;
			}
			// Final check:
			// The piece must be adjacent to another piece on the board, connected to at most 2 colors.
			var distinctColors = new List<int>();
			for (var dR = 0; dR < curSetPiece.Length && !removeOption; dR++)
				for (var dC = 0; dC < curSetPiece[dR].Length && !removeOption; dC++)
				{
					if (!curSetPiece[dR][dC]) continue;
					// For each "mino", check for adjacencies of another piece, that is not itself.
					if (curSetRowIdx + 1 + dR < newBoard.Length && newBoard[dR + curSetRowIdx + 1][dC + curSetColIdx]
						&& (dR + 1 >= curSetPiece.Length || !curSetPiece[dR + 1][dC]))
						distinctColors.Add(boardColorIdx[curSetRowIdx + 1 + dR][curSetColIdx + dC]);
					if (curSetRowIdx + dR > 0 && newBoard[dR + curSetRowIdx - 1][dC + curSetColIdx]
						&& (dR <= 0 || !curSetPiece[dR - 1][dC]))
						distinctColors.Add(boardColorIdx[curSetRowIdx - 1 + dR][curSetColIdx + dC]);
					if (curSetColIdx + 1 + dC < newBoard[curSetRowIdx].Length && newBoard[dR + curSetRowIdx][dC + curSetColIdx + 1]
						&& (dC + 1 >= curSetPiece[dR].Length || !curSetPiece[dR][dC + 1]))
						distinctColors.Add(boardColorIdx[curSetRowIdx + dR][curSetColIdx + dC + 1]);
					if (curSetColIdx + dC > 0 && newBoard[dR + curSetRowIdx][dC + curSetColIdx - 1]
						&& (dC <= 0 || !curSetPiece[dR][dC - 1]))
						distinctColors.Add(boardColorIdx[curSetRowIdx + dR][curSetColIdx + dC - 1]);
				}
			var allowedColors = Enumerable.Range(1, 3).Except(distinctColors).ToList();
			if (allowedColors.Count >= 3) // If all 3 options are possible, reroll it until it doesn't.
				continue;
			else if (allowedColors.Count < 1) // Otherwise if no options are possible, remove it.
				idxesShuffled.Remove(nextIdx);
			else
            {
				var firstColorNotUsed = allowedColors.First();
				for (var deltaR = 0; deltaR < curSetPiece.Length; deltaR++)
					for (var deltaC = 0; deltaC < curSetPiece[deltaR].Length; deltaC++)
						if (curSetPiece[deltaR][deltaC])
						{
							newBoard[curSetRowIdx + deltaR][curSetColIdx + deltaC] = true;
							boardColorIdx[curSetRowIdx + deltaR][curSetColIdx + deltaC] = firstColorNotUsed;
						}
			}
		}
		QuickLog("Generated board:");
		for (var x = 0; x < rowCount; x++)
			QuickLog(newBoard[x].Select(a => a ? "O" : "-").Join(""));
		QuickLog("One possible solution:");
		for (var x = 0; x < rowCount; x++)
			QuickLog(boardColorIdx[x].Select(a => "XKAW"[a]).Join(""));
		for (var row = 0; row < rowCount; row++)
			for (var col = 0; col < colCount; col++)
			{
				selectedTiles[row, col] = newBoard[row][col];
				tileIdxesAll[row, col] = 0;
			}
		UpdatePuzzle();
		interactable = true;
	}
	void UpdatePuzzle()
    {
		for (var x = 0; x < gridRenders.Length; x++)
        {
			var rowIdx = x / colCount;
			var colIdx = x % colCount;
			gridRenders[x].enabled = selectedTiles[rowIdx, colIdx];
			gridRenders[x].material.color = colorsRender[tileIdxesAll[rowIdx, colIdx]];
		}
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
		/* 
		 * To determine if the board is possible in theory:
		 * - Obtain a list of possible placements for that particular board.
		 * - Create a list of coordinates for each pattern that would overlap that given board.
		 * - Flatten and condense the coordinate list to reduce duplicates.
		 * - Check if a coordinate that is not present in the condensed coordinate list.
		 * - If a coordinate is not present, we know that the board is in theory not possible.
		 * - In reality, this does not work well with combinations of piece overlaps.
		 */
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

	List<int[]> PieceIdxesPerGroup(List<KeyValuePair<int, int[]>> determinedPlacements, int colCount)
    {
		var output = new List<int[]>();
		for (var n = 0; n < determinedPlacements.Count(); n++)
        {
			var curGroup = determinedPlacements[n];
			var resultCurGroup = new List<int>();
			var curPieceConfig = possiblePiecePlacements[curGroup.Key];
			var curPieceCoordTL = curGroup.Value; // [row,col]
            for (var row = 0; row < curPieceConfig.Length; row++)
				for (var col = 0; col < curPieceConfig[row].Length; col++)
					if (curPieceConfig[row][col])
						resultCurGroup.Add((row + curPieceCoordTL[0]) * colCount + col + curPieceCoordTL[1]);
			output.Add(resultCurGroup.ToArray());
		}
		return output;
    }

	List<KeyValuePair<int, int[]>> CollapseCombinations(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
    {
		// Known as the "Human Deduction" section

		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		var idxesGroupedPlacement = Enumerable.Range(0, allPossiblePlacements.Count).ToList();
		var altered2DBoard = _2Dboard.Select(a => a.ToArray()).ToArray();
		var boardWidth = _2Dboard.Length;
		var boardLength = boardWidth == 0 ? 0 : _2Dboard[0].Length;
		var knownGroupIdxes = new List<List<int>>();
		var pieceOptionsPerGroup = PieceIdxesPerGroup(allPossiblePlacements, boardWidth);
		for (var x = 0; x < boardWidth; x++)
			for (var y = 0; y < boardLength; y++)
				if (_2Dboard[x][y])
					knownGroupIdxes.Add(new List<int>() { x * boardLength + y });
		// Create a set of adjacent pairs on the grid, that connects another piece.
		var adjacentPairs = new List<int[]>();
        for (var x = 0; x < boardLength * boardWidth; x++)
        {
			if (!altered2DBoard[x / boardLength][x % boardLength]) continue;

			if (x % boardLength + 1 < boardLength && altered2DBoard[x / boardLength][x % boardLength + 1])
				adjacentPairs.Add(new[] { x, x + 1 });
			if (x / boardLength + 1 < boardWidth && altered2DBoard[x / boardLength + 1][x % boardLength])
				adjacentPairs.Add(new[] { x, x + boardLength });
		}
		var collapsable = false;
		var idxesFilled = Enumerable.Range(0, boardLength * boardWidth).Where(x => altered2DBoard[x / boardLength][x % boardLength]).ToList();
		do
		{
			var lastKnownIdxesGroups = idxesGroupedPlacement.ToArray();
			var lastAdjacentPairs = adjacentPairs.ToList();
			foreach (var pair in lastAdjacentPairs) // pair are the two idxes provided.
            {

            }
			collapsable |= lastKnownIdxesGroups.Length != idxesGroupedPlacement.Count;
		}
		while (collapsable);
		return idxesGroupedPlacement.Select(a => allPossiblePlacements[a]).ToList();
    }

	int CountSolutions(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
    {
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		// "Human Deduction" section
		allPossiblePlacements = CollapseCombinations(_2Dboard, allPossiblePlacements);
		if (_2Dboard.Sum(a => a.Count(b => b)) / 4 == allPossiblePlacements.Count)
			return 1;
		// Brute force section
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
			// To try to reduce the time taken, remove the current possibility, and all previous possibilities, then any that would not work with the remaining options.
			if (!new2DBoard.Any(a => a.Any(b => b)))
				return 1; // There is no point continuing to loop if the only option left cleans the entire board.
			else if (newAllowedPlacements.Any() && IsBoardPossibleTheoretical(new2DBoard, newAllowedPlacements))
				countedSolutions += CountSolutions(new2DBoard, newAllowedPlacements);
        }
		return countedSolutions;
    }
	bool IsBoardPossible(bool[][] _2Dboard, List<KeyValuePair<int, int[]>> determinedPlacements = null)
	{
		var allPossiblePlacements = determinedPlacements ?? GetAllPossiblePlacements(_2Dboard);
		// Brute force section
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
			// To try to reduce the time taken, remove the current possibility, and all previous possibilities, then any that would not work with the remaining options.
			if (!new2DBoard.Any(a => a.Any(b => b)))
				return true; // There is no point continuing to loop if the only option left cleans the entire board.
			else if (newAllowedPlacements.Any() && IsBoardPossibleTheoretical(new2DBoard, newAllowedPlacements))
				foundSolution |= IsBoardPossible(new2DBoard, newAllowedPlacements);
        }
		return foundSolution;
    }
	List<KeyValuePair<int, int[]>> GetAllPossiblePlacements(bool[][] _2Dboard, bool checkEmptyVersusFilled = false)
    {
		//Debug.Log(trimmedBoard.Select(a => a.Select(b => b ? "o" : "-").Join("")).Join(";"));
		var validPlacements = new List<KeyValuePair<int, int[]>>();
		for (var pieceIdx = 0; pieceIdx < possiblePiecePlacements.Length; pieceIdx++)
		{
			var curPattern = possiblePiecePlacements[pieceIdx];
			for (var rIdx = 0; rIdx <= _2Dboard.Length - curPattern.Length; rIdx++)
				for (var cIdx = 0; cIdx <= _2Dboard[0].Length - curPattern[0].Length; cIdx++) // Assume the 2D board is rectangular.
					if (DoesPatternFitBoard(_2Dboard, curPattern, rIdx, cIdx, checkEmptyVersusFilled))
						validPlacements.Add(new KeyValuePair<int, int[]>(pieceIdx, new[] { rIdx, cIdx } ));
		}
		return validPlacements; // Valid placements consist of a piece IDx, and a row,col coordinate.
    }
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press the following button in the position A4 with \"!{0} A4\". Columns are labeled A-H from left to right, rows are labeled 1-8 from top to bottom. \"press\" is optional. Button presses may be combined in one command. (\"!{0} A1 B2 C3 D4 E5 F6 G7 H8\") Submit the current batch with \"!{0} submit\", \"!{0} submitfast\", or \"!{0} submitinstant\".";
#pragma warning restore 414
	readonly string RowIDXScan = "12345678", ColIDXScan = "abcdefgh";
	IEnumerator ProcessTwitchCommand(string cmd)
    {
		var regexSubmit = Regex.Match(cmd, @"^submit(fast|instant)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		//var regexSetAll = Regex.Match(cmd, @"^setall\s[KAWX\s]+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (regexSubmit.Success)
        {
			var possibleOnes = new[] { "submit", "submitfast", "submitinstant" };
			var subVal = regexSubmit.Value.ToLowerInvariant();
			yield return null;
			submitButtons[Enumerable.Range(0, 3).FirstOrDefault(a => possibleOnes[a] == subVal)].OnInteract();
			yield return "solve";
			yield return "strike";
			yield break;
        }
		/*else if (regexSetAll.Success)
        {
			yield break;
        }*/
		var intCmd = cmd.ToLowerInvariant().Trim();
		var allIdxesToPress = new List<int>();
		if (intCmd.StartsWith("press"))
			intCmd = intCmd.Replace("press", "").Trim();
		foreach (string portion in intCmd.Split())
		{
			if (!portion.RegexMatch(string.Format(@"^[{1}][{0}]$", RowIDXScan, ColIDXScan)))
			{
				yield return string.Format("sendtochaterror The command portion \"{0}\" does not correspond to a valid coordinate!", portion);
				yield break;
			}
			var rowIdx = RowIDXScan.IndexOf(portion[1]);
			var colIdx = ColIDXScan.IndexOf(portion[0]);
			if (selectedTiles[rowIdx, colIdx])
				allIdxesToPress.Add(colCount * rowIdx + colIdx);
			else
            {
				yield return string.Format("sendtochaterror The command portion \"{0}\" does not correspond to a selectable tile!", portion);
				yield break;
			}
		}
		for (var x = 0; x < allIdxesToPress.Count; x++)
		{
			yield return null;
			gridSelectables[allIdxesToPress[x]].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		yield break;
    }

}
