using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class PuzzlePandemoniumCore : MonoBehaviour {

	public KMBombModule modSelf;
	public KMAudio mAudio;
	[SerializeField]
	private PuzzleGeneric[] allPuzzles;
	public KMSelectable[] gridSelectables;

	public Transform doorHingeL, doorHingeR, puzzlePlatform, doorFrameL, doorFrameR;


	enum PuzzleType
    {
		None = -1,
		Plumbing = 0,
		Sudoku = 1,
		Lights_Out = 2,
		Kakurasu = 3
    }

	static readonly PuzzleType[] allPossiblePuzzleTypes = new[] {
		PuzzleType.Plumbing,
		PuzzleType.Sudoku,
		PuzzleType.Lights_Out,
		PuzzleType.Kakurasu
	};

	List<PuzzleType> alterationInteractionChain;
	PuzzleType currentPuzzle;
	static int modIDCnt;
	int moduleID;
	int movesBeforeSwitching;
	bool interactable = false, moduleSolved;

	const float animSpeed = 2f;

	//List<IEnumerator> storedEnums;

	void QuickLogDebug(string toLog, params object[] args)
    {
		Debug.LogFormat("<{0} #{1}> {2}",modSelf.ModuleDisplayName, moduleID, string.Format(toLog,args));
    }
	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}",modSelf.ModuleDisplayName, moduleID, string.Format(toLog,args));
    }

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		alterationInteractionChain = allPossiblePuzzleTypes.ToList();
		alterationInteractionChain.Shuffle();
		for (var x = 0; x < alterationInteractionChain.Count; x++)
			QuickLog("Interacting the tiles in {0} will also affect the tiles in {1}.", alterationInteractionChain[x].ToString().Replace('_',' '), alterationInteractionChain[(x + 1) % 4].ToString().Replace('_',' '));

		foreach (var puzzle in allPuzzles)
		{
			puzzle.GenerateBoard();
			puzzle.ShuffleCurrentBoard();
			puzzle.HideCurrentBoard();
			//puzzle.DisplayCurrentBoard();
		}

		currentPuzzle = allPossiblePuzzleTypes.PickRandom();
		

        for (var x = 0; x < gridSelectables.Length; x++)
        {
            var y = x;
            gridSelectables[x].OnInteract += delegate
            {
                if (interactable)
                    ProcessPress(y);
                return false;
            };
        }
        for (int idx = 0; idx < allPossiblePuzzleTypes.Length; idx++)
			LogPuzzleBoard(allPossiblePuzzleTypes[idx], allPossiblePuzzleTypes[idx] != PuzzleType.Lights_Out);
		movesBeforeSwitching = Random.Range(3, 15);
		QuickLog("Switching puzzles after {0} move(s) or {1} is solved.", movesBeforeSwitching, currentPuzzle.ToString().Replace("_", " "));
		StartCoroutine(RevealPuzzleAnim());

	}

	void LogPuzzleBoard(PuzzleType puzzleToLog, bool logSolutionBoard = true)
    {
		var puzzle = GetPuzzle(puzzleToLog);
		if (puzzle == null) return;
		if (logSolutionBoard)
		{
			QuickLog("Expected solution for {0}:", puzzleToLog.ToString().Replace("_", " "));
			var solutionBoard = puzzle.GetSolutionBoard();
			for (var x = 0; x < 4; x++)
				QuickLog(solutionBoard.Skip(4 * x).Take(4).Join());
			if (puzzleToLog == PuzzleType.Kakurasu)
			{
				QuickLog("The sums of the columns should add up to from left to right: {0}", Enumerable.Range(0, 4).Select(x => Enumerable.Range(0, 4).Sum(a => solutionBoard.ElementAt(x + 4 * a) == 1 ? a + 1 : 0)).Join());
				QuickLog("The sums of the rows should add up to from top to bottom: {0}", Enumerable.Range(0, 4).Select(x => Enumerable.Range(0, 4).Sum(a => solutionBoard.ElementAt(a + 4 * x) == 1 ? a + 1 : 0)).Join());
			}
		}
		QuickLog("Initial {0} board:", puzzleToLog.ToString().Replace("_", " "));
		var currentBoard = puzzle.GetCurrentBoard();
		for (var x = 0; x < 4; x++)
			QuickLog(currentBoard.Skip(4 * x).Take(4).Join());
	}

	IEnumerator RevealSpecificPuzzleAnim(PuzzleType aPuzzle)
    {
		var nextTransformL = doorHingeL.localRotation * Quaternion.Euler(0, 90, 0);
		var lastTransformL = doorHingeL.localRotation;
		var nextTransformR = doorHingeR.localRotation * Quaternion.Euler(0, 90, 0);
		var lastTransformR = doorHingeR.localRotation;

		var specifiedPuzzle = GetPuzzle(aPuzzle);
		foreach (var puzzle in allPuzzles)
			puzzle.HideCurrentBoard();

		specifiedPuzzle.DisplayCurrentBoard();


		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
        {
			doorHingeL.localRotation = Quaternion.LerpUnclamped(lastTransformL, nextTransformL, t);
			doorHingeR.localRotation = Quaternion.LerpUnclamped(lastTransformR, nextTransformR, t);
			yield return null;
        }
		doorHingeL.localRotation = nextTransformL;
		doorHingeR.localRotation = nextTransformR;

		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
		{
			doorFrameL.localScale = new Vector3(60 * (1f - t), 2, 0.5f);
			doorFrameR.localScale = new Vector3(60 * (1f - t), 2, 0.5f);
			doorFrameL.localPosition = Vector3.right * 30 * (1f - t);
			doorFrameR.localPosition = Vector3.right * 30 * (1f - t);

			puzzlePlatform.transform.localPosition = Vector3.up * 0.015f * t;

			yield return null;
		}
		doorFrameL.localPosition = Vector3.zero;
		doorFrameR.localPosition = Vector3.zero;
		doorFrameL.gameObject.SetActive(false);
		doorFrameR.gameObject.SetActive(false);

		puzzlePlatform.transform.localPosition = Vector3.up * 0.015f;
		yield break;
	}
	IEnumerator RevealPuzzleAnim(bool reenableInteractions = true)
    {
		var nextTransformL = doorHingeL.localRotation * Quaternion.Euler(0, 90, 0);
		var lastTransformL = doorHingeL.localRotation;
		var nextTransformR = doorHingeR.localRotation * Quaternion.Euler(0, 90, 0);
		var lastTransformR = doorHingeR.localRotation;

		var currentPuzzle = GetCurrentPuzzle();
		foreach (var puzzle in allPuzzles)
			puzzle.HideCurrentBoard();

		currentPuzzle.DisplayCurrentBoard();


		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
        {
			doorHingeL.localRotation = Quaternion.LerpUnclamped(lastTransformL, nextTransformL, t);
			doorHingeR.localRotation = Quaternion.LerpUnclamped(lastTransformR, nextTransformR, t);
			yield return null;
        }
		doorHingeL.localRotation = nextTransformL;
		doorHingeR.localRotation = nextTransformR;

		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
		{
			doorFrameL.localScale = new Vector3(60 * (1f - t), 2, 0.5f);
			doorFrameR.localScale = new Vector3(60 * (1f - t), 2, 0.5f);
			doorFrameL.localPosition = Vector3.right * 30 * (1f - t);
			doorFrameR.localPosition = Vector3.right * 30 * (1f - t);

			puzzlePlatform.transform.localPosition = Vector3.up * 0.015f * t;

			yield return null;
		}
		doorFrameL.localPosition = Vector3.zero;
		doorFrameR.localPosition = Vector3.zero;
		doorFrameL.gameObject.SetActive(false);
		doorFrameR.gameObject.SetActive(false);

		puzzlePlatform.transform.localPosition = Vector3.up * 0.015f;
		interactable |= reenableInteractions;
		yield break;
	}
	IEnumerator HidePuzzleAnim()
    {
		doorFrameL.gameObject.SetActive(true);
		doorFrameR.gameObject.SetActive(true);
		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
		{
			doorFrameL.localScale = new Vector3(60 * t, 2, 0.5f);
			doorFrameR.localScale = new Vector3(60 * t, 2, 0.5f);
			doorFrameL.localPosition = Vector3.right * 30 * t;
			doorFrameR.localPosition = Vector3.right * 30 * t;

			puzzlePlatform.transform.localPosition = Vector3.up * 0.015f * (1f - t);

			yield return null;
		}
		doorFrameL.localPosition = Vector3.right * 30;
		doorFrameR.localPosition = Vector3.right * 30;
		doorFrameL.localScale = new Vector3(60, 2, 0.5f);
		doorFrameR.localScale = new Vector3(60, 2, 0.5f);

		puzzlePlatform.transform.localPosition = Vector3.zero;
		var nextTransformL = doorHingeL.localRotation * Quaternion.Euler(0, -90, 0);
		var lastTransformL = doorHingeL.localRotation;
		var nextTransformR = doorHingeR.localRotation * Quaternion.Euler(0, -90, 0);
		var lastTransformR = doorHingeR.localRotation;


		for (float t = 0; t < 1f; t += Time.deltaTime * animSpeed)
        {
			doorHingeL.localRotation = Quaternion.LerpUnclamped(lastTransformL, nextTransformL, t);
			doorHingeR.localRotation = Quaternion.LerpUnclamped(lastTransformR, nextTransformR, t);
			yield return null;
        }
		doorHingeL.localRotation = nextTransformL;
		doorHingeR.localRotation = nextTransformR;


		//interactable = true;
		yield break;
	}
	IEnumerator HiddenBoardSolveAnim(PuzzleType hiddenPuzzleSolved = PuzzleType.None)
    {
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
		yield return HidePuzzleAnim();
		yield return RevealSpecificPuzzleAnim(hiddenPuzzleSolved);
		yield return HandlePuzzleSolveAnim(hiddenPuzzleSolved, false);
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
		yield return HidePuzzleAnim();
		yield return RevealPuzzleAnim();
		var curPuzzleScript = GetCurrentPuzzle();
		if (curPuzzleScript.CheckCurrentBoard())
        {
			interactable = false;
			QuickLog("{0} has also been solved!", currentPuzzle.ToString().Replace("_", " "));
			if (curPuzzleScript.GetCurrentBoard().SequenceEqual(curPuzzleScript.GetSolutionBoard()))
				QuickLogDebug("Solved board for {0} matches expected board.", currentPuzzle.ToString().Replace("_", " "));
			else
				QuickLogDebug("{0}'s solved board: {1}", currentPuzzle.ToString().Replace("_", " "), curPuzzleScript.GetCurrentBoard().Join());
			StartCoroutine(HandlePuzzleSolveAnim(currentPuzzle));
			alterationInteractionChain.Remove(currentPuzzle);
		}
		else if (alterationInteractionChain.Count > 1)
		{
			QuickLog("The new interaction chain is as follows:");
			for (var x = 0; x < alterationInteractionChain.Count; x++)
				QuickLog("Interacting the tiles in {0} will also affect the tiles in {1}.", alterationInteractionChain[x].ToString().Replace('_', ' '), alterationInteractionChain[(x + 1) % alterationInteractionChain.Count].ToString().Replace('_', ' '));
			if (movesBeforeSwitching <= 0)
			{
				interactable = false;
				StartCoroutine(SwitchPuzzleAnim());
			}
		}
		else if (alterationInteractionChain.Any())
			QuickLog("When {0} is solved, the module will disarm.", currentPuzzle.ToString().Replace("_", " "));
		moduleSolved = !alterationInteractionChain.Any();
	}
	IEnumerator SwitchPuzzleAnim()
    {
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, transform);
		yield return HidePuzzleAnim();
		if (alterationInteractionChain.Any())
		{
			currentPuzzle = alterationInteractionChain.Where(a => a != currentPuzzle).PickRandom();
			if (alterationInteractionChain.Count > 1)
			{
				movesBeforeSwitching = Random.Range(3, 15);
				QuickLog("Switching puzzles after {0} move(s) or {1} is solved.", movesBeforeSwitching, currentPuzzle.ToString().Replace("_", " "));
			}
			else
				QuickLog("When {1} is solved, the module will disarm.", movesBeforeSwitching, currentPuzzle.ToString().Replace("_", " "));
			yield return RevealPuzzleAnim();
		}
		else
		{
			QuickLog("No puzzles left. Module disarmed.");
			modSelf.HandlePass();
		}
    }
	IEnumerator HandlePuzzleSolveAnim(PuzzleType currentPuzzle, bool switchToDifferentPuzzle = true)
    {
		var flipHoriz = Random.value < 0.5f;
		var flipVert = Random.value < 0.5f;
		switch (currentPuzzle)
        {
			case PuzzleType.Lights_Out:
				var lightsOutLights = allPuzzles[(int)PuzzleType.Lights_Out].usedRenderers.Take(16);
				for (var x = 0; x < 7; x++)
                {
					yield return new WaitForSeconds(0.05f);
					var selectedItems = Enumerable.Range(0, 16).Where(a => (flipHoriz ? (3 - a / 4) : a / 4) + (flipVert ? (3 - a % 4) : a % 4) <= x);
					foreach (var idx in selectedItems)
					{
						lightsOutLights.ElementAt(idx).material.color = Color.green;
					}
                }
				for (var p = 0; p < 3; p++)
				{
					yield return new WaitForSeconds(0.1f);
					for (var x = 0; x < 16; x++)
						lightsOutLights.ElementAt(x).material.color = p % 2 == 0 ? Color.black : Color.green;
				}
				break;
			case PuzzleType.Sudoku:
				var numbers = allPuzzles[(int)PuzzleType.Sudoku].usedRenderers.Where(a => a.GetComponent<TextMesh>() != null).Select(a => a.GetComponent<TextMesh>());
				var lastColors = numbers.Select(a => a.color).ToArray();
				for (float t = 0; t < 1f; t += Time.deltaTime * 5)
				{
					yield return null;
					for (var x = 0; x < 16; x++)
						numbers.ElementAt(x).color = Color.white * t + lastColors[x] * (1f - t);
				}
				for (var x = 0; x < 16; x++)
					numbers.ElementAt(x).color = Color.white;
				break;
			case PuzzleType.Kakurasu:
				var kakurasuSquares = allPuzzles[(int)PuzzleType.Kakurasu].usedRenderers.Take(16);
				for (var x = 0; x < 7; x++)
				{
					yield return new WaitForSeconds(0.05f);
					var selectedItems = Enumerable.Range(0, 16).Where(a => (flipHoriz ? (3 - a / 4) : a / 4) + (flipVert ? (3 - a % 4) : a % 4) <= x);
					foreach (var idx in selectedItems)
					{
						kakurasuSquares.ElementAt(idx).material.color = Color.black;
					}
				}
				for (var p = 0; p < 3; p++)
				{
					yield return new WaitForSeconds(0.1f);
					for (var x = 0; x < 16; x++)
						kakurasuSquares.ElementAt(x).material.color = p % 2 == 0 ? Color.green : Color.black;
				}
				break;
			case PuzzleType.Plumbing:
				var pipes = allPuzzles[(int)PuzzleType.Plumbing].usedRenderers;
				var idxRelevantOffsetPipes = Enumerable.Range(0, 80).Where(a => (a + 1) % 5 != 0);
				var lastColorPipes = pipes.Select(a => a.material.color).ToArray();
				for (float t = 0; t < 1f; t += Time.deltaTime * 5)
				{
					yield return null;
					for (var x = 0; x < idxRelevantOffsetPipes.Count(); x++)
						pipes.ElementAt(idxRelevantOffsetPipes.ElementAt(x)).material.color = Color.yellow * t + lastColorPipes[x] * (1f - t);
				}
				for (var x = 0; x < idxRelevantOffsetPipes.Count(); x++)
					pipes.ElementAt(idxRelevantOffsetPipes.ElementAt(x)).material.color = Color.yellow;
				break;
        }
		if (switchToDifferentPuzzle)
			StartCoroutine(SwitchPuzzleAnim());
		yield break;
    }

	PuzzleGeneric GetPuzzle(PuzzleType puzzle)
    {
		PuzzleGeneric outputPuzzle = null;
		//Debug.Log("Puzzle" + puzzle.ToString().Replace("_", ""));
		for (var x = 0; x < allPuzzles.Length; x++)
		{
			//Debug.Log(allPuzzles[x].GetType().Name);
			if (allPuzzles[x].GetType().Name == "Puzzle" + puzzle.ToString().Replace("_", ""))
				outputPuzzle = allPuzzles[x];
		}
		return outputPuzzle;
    }
	PuzzleGeneric GetCurrentPuzzle()
    {
		return GetPuzzle(currentPuzzle);
    }


	void ProcessPress(int idx)
    {
		var curPuzzleScript = GetCurrentPuzzle();
		if (curPuzzleScript == null) return;
		
		var lastBoardState = curPuzzleScript.GetCurrentBoard().ToArray();
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, gridSelectables[idx].transform);
		curPuzzleScript.HandleIdxPress(idx);
		if (!curPuzzleScript.GetCurrentBoard().SequenceEqual(lastBoardState))
        {
			curPuzzleScript.DisplayCurrentBoard();
			if (movesBeforeSwitching > 0)
				movesBeforeSwitching--;
			if (alterationInteractionChain.Count > 1)
			{
				var nextAffectedPuzzleType = alterationInteractionChain[(alterationInteractionChain.IndexOf(currentPuzzle) + 1) % alterationInteractionChain.Count];
				var nextAffectedPuzzleScript = GetPuzzle(nextAffectedPuzzleType);
				nextAffectedPuzzleScript.HandleIdxPress(idx);
				if (nextAffectedPuzzleScript.CheckCurrentBoard())
                {
					interactable = false;
					QuickLog("{0} has been solved indirectly!", nextAffectedPuzzleType.ToString().Replace("_", " "));
					if (nextAffectedPuzzleScript.GetCurrentBoard().SequenceEqual(nextAffectedPuzzleScript.GetSolutionBoard()))
						QuickLogDebug("Solved board for {0} matches expected board.", nextAffectedPuzzleType.ToString().Replace("_", " "));
					else
						QuickLogDebug("{0}'s solved board: {1}", nextAffectedPuzzleType.ToString().Replace("_", " "), nextAffectedPuzzleScript.GetCurrentBoard().Join());
					alterationInteractionChain.Remove(nextAffectedPuzzleType);
					StartCoroutine(HiddenBoardSolveAnim(nextAffectedPuzzleType));
					return;
				}
			}
			if (curPuzzleScript.CheckCurrentBoard())
            {
				QuickLog("{0} has been solved!", currentPuzzle.ToString().Replace("_", " "));
				if (curPuzzleScript.GetCurrentBoard().SequenceEqual(curPuzzleScript.GetSolutionBoard()))
					QuickLogDebug("Solved board for {0} matches expected board.", currentPuzzle.ToString().Replace("_", " "));
				else
					QuickLogDebug("{0}'s solved board: {1}", currentPuzzle.ToString().Replace("_", " "), curPuzzleScript.GetCurrentBoard().Join());
				alterationInteractionChain.Remove(currentPuzzle);
				if (alterationInteractionChain.Count > 1)
				{
					QuickLog("The new interaction chain is as follows:");
					for (var x = 0; x < alterationInteractionChain.Count; x++)
						QuickLog("Interacting the tiles in {0} will also affect the tiles in {1}.", alterationInteractionChain[x].ToString().Replace('_', ' '), alterationInteractionChain[(x + 1) % alterationInteractionChain.Count].ToString().Replace('_', ' '));
				}
				else
					moduleSolved = !alterationInteractionChain.Any();
				interactable = false;
				StartCoroutine(HandlePuzzleSolveAnim(currentPuzzle));
			}
			else if (alterationInteractionChain.Count > 1 && movesBeforeSwitching <= 0)
            {
				//QuickLog("Switching puzzles...");
				interactable = false;
				StartCoroutine(SwitchPuzzleAnim());
            }
		}
    }
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press the following button in the position A4 with \"!{0} A4\". Columns are labeled A-D from left to right, rows are labeled 1-4 from top to bottom. \"press\" is optional. Button presses may be combined in one command, and may be interrupted by puzzles being switched. (\"!{0} A1 B2 C3 D4\")";
#pragma warning restore 414
	readonly string RowIDXScan = "1234", ColIDXScan = "abcd";
	IEnumerator ProcessTwitchCommand(string cmd)
    {
		if (moduleSolved)
        {
			yield return "sendtochaterror The module is already solved. Not worth to interact with it again.";
			yield break;
        }
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
			allIdxesToPress.Add(4 * rowIdx + colIdx);
		}
		for (var x = 0; x < allIdxesToPress.Count; x++)
		{
			if (!interactable)
            {
				yield return string.Format("sendtochat {1}, Press #{0} has been interrupted due to the module switching puzzles.", x + 1, "{0}");
				yield break;
            }
			yield return null;
			gridSelectables[allIdxesToPress[x]].OnInteract();
			if (moduleSolved)
				yield return "solve";
			else
				yield return new WaitForSeconds(0.1f);
		}
		yield break;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!moduleSolved)
        {
			if (!interactable)
				yield return true;
			yield return null;
			movesBeforeSwitching += 80;
			var currentPuzzleBoard = GetCurrentPuzzle();
			var solBoard = currentPuzzleBoard.GetSolutionBoard();
			var curBoard = currentPuzzleBoard.GetCurrentBoard();
			if (currentPuzzle == PuzzleType.Lights_Out)
            {
				for (var x = 0; x < 12; x++)
				{
					if (curBoard.ElementAt(x) == 1)
					{
						gridSelectables[x + 4].OnInteract();
						yield return new WaitForSeconds(0.1f);
						curBoard = currentPuzzleBoard.GetCurrentBoard();
					}
				}
			}
			else
            {
				for (var x = 0; x < 16; x++)
                {
					while (curBoard.ElementAt(x) != solBoard.ElementAt(x))
					{
						gridSelectables[x].OnInteract();
						yield return new WaitForSeconds(0.1f);
						curBoard = currentPuzzleBoard.GetCurrentBoard();
					}
				}
            }
		}
		while (!interactable)
			yield return true;
    }

}
