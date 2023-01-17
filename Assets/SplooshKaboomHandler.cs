using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SplooshKaboomHandler : MonoBehaviour {
	public KMSelectable[] gridSelectables;
	public KMSelectable resetSelectable;
	public KMRuleSeedable ruleSeedable;
	public KMBombModule modSelf;
	public KMAudio mAudio;
	public TextMesh displayMesh;
	public MeshRenderer[] boardRenderers;

	int num1S = 100, num2S = 100, num3S = 100;

	static int modIDCnt;
	int moduleID;

	int[][] board;
	int[] countsCurSize = new int[3];
	int maxShotsCleared = 20, boardsCleared = 0;
	List<int> trackedShots;

	WichmannHillRandom randomizer;
	bool interactable = false, cleared = false, holding = false;
	float timeHeld;
	void QuickLog(string value, params object[] args)
	{
		Debug.LogFormat("[Sploosh Kaboom #{0}] {1}", moduleID, string.Format(value, args));
	}
	void QuickLogDebug(string value, params object[] args)
	{
		Debug.LogFormat("<Sploosh Kaboom #{0}> {1}", moduleID, string.Format(value, args));
	}

	// Use this for initialization
	void Start() {
		moduleID = ++modIDCnt;
		var rng = ruleSeedable.GetRNG() ?? new MonoRandom(1);
		if (rng.Seed != 1)
		{
			num1S = rng.Next(0, 30269);
			num2S = rng.Next(0, 30307);
			num3S = rng.Next(0, 30323);
		}
		QuickLog("Using rule seed {0} for this module.", rng.Seed);
		QuickLogDebug("S Values: {0}, {1}, {2}", num1S, num2S, num3S);
		SetupRandom();
		GenerateBoard();
		UpdateBoard();
		for (var x = 0; x < gridSelectables.Length; x++)
        {
			var y = x;
			gridSelectables[x].OnInteract += delegate {
				if (interactable) ProcessShot(y);
				return false;
			};
        }
		resetSelectable.OnInteract += delegate {
			timeHeld = 0f;
			holding = true;
			return false;
		};
		resetSelectable.OnInteractEnded += delegate {
			holding = false;
			if (timeHeld < 2f)
				HandleSoftReset();
			else
            {

            }
		};

		interactable = true;
	}
	void HandleSoftReset()
    {
		if (cleared)
        {
			cleared = false;
			GenerateBoard();
			UpdateBoard();
		}
		else
        {
			var unhitIdxes = Enumerable.Range(0, 64).Except(trackedShots ?? Enumerable.Empty<int>()).ToList();
			unhitIdxes.Shuffle();
			for (var x = 0; x < unhitIdxes.Count && trackedShots.Count < 24; x++)
            {
				ProcessShot(unhitIdxes[x]);
            }
        }
    }
	void ProcessShot(int idx)
	{
		if (!trackedShots.Contains(idx) && !cleared)
        {
			trackedShots.Add(idx);
			var idxRow = idx / 8;
			var idxCol = idx % 8;
			if (board[idxRow][idxCol] != 0)
				countsCurSize[board[idxRow][idxCol] - 1]--;
			var gameEnded = countsCurSize.All(a => a == 0) || trackedShots.Count >= 24;
			UpdateBoard(gameEnded);
			if (gameEnded)
            {
				QuickLog("Current board ended with the following amount of shots taken: {0}", trackedShots.Count);
				QuickLog("Attempt successful? {0}", countsCurSize.All(a => a == 0) ? "YES" : "NO");
				QuickLog("Shots made: {0}", trackedShots.Select(a => "ABCDEFGH"[a % 8] + "" + "12345678"[a / 8]).Join(", "));
				maxShotsCleared = Mathf.Min(trackedShots.Count, maxShotsCleared);
				if (countsCurSize.All(a => a == 0))
					boardsCleared++;
				if (boardsCleared >= 2 && maxShotsCleared < 20)
					modSelf.HandlePass();
				cleared = true;
            }

        }
	}
	void UpdateBoard(bool revealAll = false)
    {
        for (var x = 0; x < boardRenderers.Length; x++)
        {
			var row = x / 8;
			var col = x % 8;
			boardRenderers[x].material.color = ((trackedShots != null && trackedShots.Contains(x)) || revealAll ? board[row][col] > 0 ? Color.white : Color.black : Color.blue) * 0.5f;
        }

    }
	void SetupRandom()
    {
		randomizer = new WichmannHillRandom(num1S, num2S, num3S);
		var skipCounts = Random.Range(100, 301); //300;
		QuickLog("Skipping {0} iterations for this module.", skipCounts);
		for (var x = 0; x < skipCounts; x++)
		{
			randomizer.Next();
			if ((x + 1) % 50 == 0)
				QuickLogDebug("i = {0}, s = [{1}]", x + 1, randomizer.GetNums().Join(","));
		}
	}

	void GenerateBoard()
    {
		if (trackedShots == null)
			trackedShots = new List<int>();
		else
			trackedShots.Clear();
		board = new int[8][]; // Generate a 8x8 board on the module.
        for (int i = 0; i < board.Length; i++)
            board[i] = new int[8];

		for (var x = 0; x < countsCurSize.Length; x++)
			countsCurSize[x] = 2 + x;

        for (var x = 0; x < 3; x++)
        {
			//Debug.Log(x);
			var curLength = 2 + x;
			var isOverlapping = false;
			var horizPlacement = false;
			var coordinateRow = 0;
			var coordinateCol = 0;
			do
			{
				horizPlacement = randomizer.Next() < 0.5f;
				coordinateRow = randomizer.Next(0, 8 - (!horizPlacement ? curLength : 0));
				coordinateCol = randomizer.Next(0, 8 - (horizPlacement ? curLength : 0));

				//Debug.Log(horizPlacement);
				//Debug.Log(coordinateRow);
				//Debug.Log(coordinateCol);

				isOverlapping = horizPlacement ?
					board[coordinateRow].Skip(coordinateCol).Take(curLength).Any(a => a != 0)
					: board.Skip(coordinateRow).Take(curLength).Any(a => a[coordinateCol] != 0);
				//yield return new WaitForSeconds(1);
			}
			while (isOverlapping);
			for (var p = 0; p < curLength; p++)
				board[coordinateRow + (!horizPlacement ? p : 0)][coordinateCol + (horizPlacement ? p : 0)] = x + 1;
			//Debug.Log(board.Select(a => a.Join(" ")).Join("\n"));
		}
		QuickLog("Generated board: ");
        for (var x = 0; x < board.Length; x++)
			QuickLog(board[x].Join(" "));
    }
	void Update()
    {
		if (holding)
			timeHeld += Time.deltaTime;
    }
}
