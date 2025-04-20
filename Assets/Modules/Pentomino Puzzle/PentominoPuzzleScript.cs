using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MinoShape {

	public bool[][] configuration;
	public int xCord, yCord;
	public int Width { get { return configuration.Length; } }
	public int Length { get { return configuration.Any() ? configuration.Max(a => a.Length) : 0; } }
	public MinoShape()
    {
		configuration = new bool[0][];
    }
	public MinoShape(string encoding)
    {
		configuration = encoding.Split(';').Select(a => a.Select(b => b == 'O').ToArray()).ToArray();
    }
	public void FlipHorizontally()
    {
		configuration = configuration.Reverse().ToArray();
    }
	public void FlipVertically()
    {
		configuration = configuration.Select(a => a.Reverse().ToArray()).ToArray();
    }
	public void FlipDiagonallyMain()
	{
		var resultingConfiguration = new bool[Length][];
		for (var x = 0; x < Length; x++)
		{
			resultingConfiguration[x] = new bool[Width];
			for (var y = 0; y < Width; y++)
				resultingConfiguration[x][y] = configuration[y][x];
		}
		configuration = resultingConfiguration;
    }
}

public class PentominoPuzzleScript : MonoBehaviour {

	const int boardWidth = 8, boardLength = 10;
	int selectedMinoIdx = -1;

	static readonly Color[] possibleColors = new[] {
		Color.red,
		Color.green,
		Color.yellow,
		Color.magenta,
		Color.white,
		Color.cyan,
		new Color(.3f,.3f,1f),
		new Color(0,.5f,1f),
		new Color(0,1f,.5f),
		new Color(.5f,1f,0),
		new Color(1f,.5f,0),
		new Color(1f,0,.5f),
		new Color(.5f,0,1f),
	};
	Color[] selectedColors;
	MinoShape[] allMinos;
	[SerializeField] KMSelectable[] gridSelectables, modifierSelectables;
	[SerializeField] MeshRenderer[] gridRenderers, hlRenderers, goalRenderers;
	[SerializeField] KMBombModule modSelf;
	[SerializeField] KMAudio mAudio;
	[SerializeField] KMSelectable logButton;
	static readonly string[] pentominoEncodings = new[] {
		"OOOOO",
		"OOOO;OXXX",
		"OOOO;XOXX",
		"OOOX;XXOO",
		"OOO;OOX",
		"OOO;OXO",
		"OOO;OXX;OXX",
		"OOO;XOX;XOX",
		"XOX;OOO;XOX",
		"OXX;OOX;XOO",
		"OXX;OOO;XOX",
		"OXX;OOO;XXO",
	};

	static int modIDCnt;
	int moduleID;
	bool moduleSolved;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		selectedColors = possibleColors.ToArray().Shuffle().Take(12).ToArray();
		allMinos = pentominoEncodings.Select(a => new MinoShape(a)).ToArray();
		foreach (var xMino in allMinos)
        {
			var idxTransformList = Enumerable.Range(0, 3).ToList().Shuffle();
			foreach (var idxTransform in idxTransformList)
            {
				var applyTransformation = Random.value < 0.5f;
				if (!applyTransformation) continue;
				switch (idxTransform)
                {
					case 0: xMino.FlipHorizontally(); break;
					case 1: xMino.FlipVertically(); break;
					case 2: xMino.FlipDiagonallyMain(); break;
                }
            }
			xMino.xCord = Random.Range(0, boardLength - xMino.Length);
			xMino.yCord = Random.Range(0, boardWidth - xMino.Width);
        }
		allMinos.Shuffle();
		UpdateGrid();
        for (var x = 0; x < gridSelectables.Length; x++)
        {
			var y = x;
			gridSelectables[x].OnInteract += delegate {
				mAudio.PlaySoundAtTransform("tick", transform);
				HandleIdxSelect(y);
				return false;
			};
        }
        for (var x = 0; x < modifierSelectables.Length; x++)
        {
			if (modifierSelectables[x] == null) continue;
			var y = x;
			modifierSelectables[x].OnInteract += delegate {
				mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, modifierSelectables[y].transform);
				modifierSelectables[y].AddInteractionPunch(0.1f);
				HandleIdxTransformation(y);
				return false;
			};
        }
		if (logButton != null)
        {
			logButton.OnInteract += delegate {
				var obtainedBoard = GetIdxBoard();
				Debug.LogFormat("{0}", obtainedBoard.Take(2).Sum(b => b.Count(c => c.Any())));
				return false;
			};
        }
		QuickLog("The module only logs the board state of when it was solved.");
	}

	List<int>[][] GetIdxBoard()
    {
		var resultingBoard = new List<int>[boardWidth][];
		for (var y = 0; y < boardWidth; y++)
		{
			resultingBoard[y] = new List<int>[boardLength];
			for (var x = 0; x < boardLength; x++)
				resultingBoard[y][x] = new List<int>();
		}
		for (int n = 0; n < allMinos.Length; n++)
		{
			MinoShape xMino = allMinos[n];
			var curX = xMino.xCord;
			var curY = xMino.yCord;
			for (var dy = 0; dy < xMino.Width; dy++)
				for (var dx = 0; dx < xMino.Length; dx++)
					if (xMino.configuration[dy][dx])
						resultingBoard[curY + dy][curX + dx].Add(n);
		}
		return resultingBoard;
	}

	void CheckSolveState()
	{
		if (moduleSolved) return;
		var obtainedBoard = GetIdxBoard();
		/* Check if the top two rows are all clear...
		 * And if the rest of the rows have no overlapping tiles.
		 */
		if (!obtainedBoard.Take(2).Any(a => a.Any(b => b.Any())) &&
			obtainedBoard.Skip(2).All(row => row.All(n => n.Count <= 1)))
		{
			QuickLog("Module disarmed with the following board:");
			foreach (var row in obtainedBoard)
				QuickLog(row.Select(a => !a.Any() ? '-' : a.Count == 1 ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[a.Single()] : '?').Join(""));
			foreach (var renderer in goalRenderers)
				renderer.material.color = Color.green;
			moduleSolved = true;
			modSelf.HandlePass();
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		}
	}

	#region Interaction Methods
	void HandleIdxTransformation(int idx)
    {
		if (selectedMinoIdx == -1) return;
		var currentXMino = allMinos[selectedMinoIdx];
		switch (idx)
        {
			case 0: currentXMino.yCord--; break; // Move Up
			case 1: currentXMino.xCord++; break; // Move Right
			case 2: currentXMino.yCord++; break; // Move Down
			case 3: currentXMino.xCord--; break; // Move Left
			case 4: currentXMino.FlipHorizontally(); break; // Flip Horizontally.
			case 5: currentXMino.FlipVertically(); break; // Flip Vertically.
			case 6: {
					var lengthDelta = currentXMino.Length;
					var widthDelta = currentXMino.Width;
					currentXMino.FlipDiagonallyMain(); currentXMino.FlipVertically();
					lengthDelta -= currentXMino.Length;
					widthDelta -= currentXMino.Width;
					currentXMino.xCord += lengthDelta / 2;
					currentXMino.yCord += widthDelta / 2;
				} break; // Rotate 90 CW
		}

		currentXMino.xCord = Mathf.Clamp(currentXMino.xCord, 0, boardLength - currentXMino.Length);
		currentXMino.yCord = Mathf.Clamp(currentXMino.yCord, 0, boardWidth - currentXMino.Width);
		UpdateGrid();
		UpdateHLGrid();
		CheckSolveState();
    }

	void HandleIdxSelect(int idx)
	{
		var currentBoard = GetIdxBoard();
		var colIdx = idx % boardLength;
		var rowIDx = idx / boardLength;
		var curIdxGroup = currentBoard[rowIDx][colIdx];
		var idxFound = curIdxGroup.IndexOf(selectedMinoIdx);
		if (idxFound == -1)
			selectedMinoIdx = curIdxGroup.Any() ? curIdxGroup.First() : -1;
		else
			selectedMinoIdx = curIdxGroup[(idxFound + 1) % curIdxGroup.Count];
		UpdateHLGrid();
	}
    #endregion

    #region Grid Update Methods

    void UpdateHLGrid()
    {
		for (var x = 0; x < hlRenderers.Length; x++)
			hlRenderers[x].material.color = Color.black;
		if (selectedMinoIdx == -1) return;
		var relevantMino = allMinos[selectedMinoIdx];
		for (var dy = 0; dy < relevantMino.Width; dy++)
            for (var dx = 0; dx < relevantMino.Length; dx++)
				if (relevantMino.configuration[dy][dx])
				{
					var idxToFind = boardLength * (dy + relevantMino.yCord) + (dx + relevantMino.xCord);
					if (idxToFind < hlRenderers.Length)
						hlRenderers[idxToFind].material.color = selectedColors[selectedMinoIdx];
				}

	}

	void UpdateGrid() {
		var resultingBoard = GetIdxBoard();
		for (var y = 0; y < boardWidth; y++)
		{
			for (var x = 0; x < boardLength; x++)
			{
				var minoOverlapCnt = resultingBoard[y][x].Count;
				var idxToFind = boardLength * y + x;
				if (idxToFind < gridRenderers.Length)
					gridRenderers[idxToFind].material.color = minoOverlapCnt > 1 ? Color.gray : minoOverlapCnt == 0 ? Color.black : selectedColors[resultingBoard[y][x].Single()];
			}
		}
	}
    #endregion
}
