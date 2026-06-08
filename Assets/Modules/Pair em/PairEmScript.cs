using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class PairEmScript : MonoBehaviour {

	public KMBombModule modSelf;
	public KMAudio mAudio;
	public KMColorblindMode colorblindMode;

	public PairEmCard[] cards, ancilleryCards;

	public KMSelectable[] cardSelectables;
	public KMSelectable resetSelectable;
	public MeshRenderer resetRender;

	
	List<int> cardIdxes = new List<int>(), initialCardIdxes;
	List<int[]> allowedCardPairs = new List<int[]>(), intendedSolutionPath;

	public Color[] possibleColors;
	public Texture[] possibleCardSuits;
	public Texture backTexture;

	public TextMesh counterMesh;

	bool moduleSolved = false, resetHeld, disableStrike, colorblindEnabled;

	const int boardWidth = 5, cardCount = 25;
	int moduleID;
	static int modIDCnt;
	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	int typesPicked = 8, preferredTypes = 6;
	int idxCardSelected = -1;
	Color[] usedColors;
	Texture[] usedSuits;

	float timeHeld;

	public float startX, deltaX, startY, deltaY;
	void QuickLog(string toLog, params object[] args)
	{
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
	}

	// Use this for initialization
	void Start ()
    {
        try
        {
			colorblindEnabled = colorblindMode.ColorblindModeActive;
        }
        catch
        {
			colorblindEnabled = false;
        }
        moduleID = ++modIDCnt;
		intendedSolutionPath = new List<int[]>();
		// Generate all possible valid pairs that can be produced on the module.
        for (var x = 0; x < cardCount; x++)
        {
			// Adjacent to the right.
			if (x % boardWidth < boardWidth - 1)
				allowedCardPairs.Add(new[] { x, x + 1 });
			// Adjacent below + left.
			if (x % boardWidth > 0 && x + boardWidth < cardCount)
				allowedCardPairs.Add(new[] { x, x + boardWidth - 1 });
			// Adjacent below.
			if (x + boardWidth < cardCount)
				allowedCardPairs.Add(new[] { x, x + boardWidth });
			// Adjacent below + right.
			if (x + boardWidth < cardCount && x % boardWidth < boardWidth - 1)
				allowedCardPairs.Add(new[] { x, x + boardWidth + 1 });
		}
		//Debug.LogFormat("[{0}]",allowedCardPairs.Select(b => b.Join(",")).Join("];["));
		GeneratePuzzle();
		for (var x = 0; x < cardIdxes.Distinct().Count(); x++)
		{
			var cardName = usedSuits[x].name.Replace("Card", "");
			QuickLog("Card #{0} is a{2} {1}.", x + 1, cardName, cardName.StartsWith("E") ? "n" : "");
		}
		QuickLog("Initial state in reading order: {0}", cardIdxes.Select(a => a + 1).Join(","));
		QuickLog("Intended Solution: [{0}]", intendedSolutionPath.Select(a => a.Select(b => QuickCoord(b)).Join(",")).Join("];["));
		UpdateBoard(useCurPositions: true);
		for (var x = 0; x < cardSelectables.Length; x++)
        {
			var y = x;
			cardSelectables[x].OnInteract += delegate {
				HandleCardSelect(y);
				return false;
			};
			/*cardSelectables[x].OnHighlight += delegate {
				if (y < cardIdxes.Count)
					cards[y].backRender.material.color = Color.white;
			};
			cardSelectables[x].OnHighlightEnded += delegate {
				if (y < cardIdxes.Count)
					cards[y].backRender.material.color = usedColors[cardIdxes[y]];
			};*/
        }

		resetSelectable.OnInteract += delegate
		{
			resetSelectable.AddInteractionPunch();
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, resetSelectable.transform);
			resetHeld = true;
			return false;
		};
		resetSelectable.OnInteractEnded += delegate
		{
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, resetSelectable.transform);
			resetHeld = false;
			if (timeHeld >= 1f)
				HandleReset();
		};

	}
	int CountMovesCurState()
    {
		var curCardsLeft = cardIdxes.Count();
		var allowedMatchPairsNew = allowedCardPairs.Where(a => a.All(b => b < curCardsLeft));

		return allowedMatchPairsNew.Count(a => a.Select(b => cardIdxes[b]).Distinct().Count() == 1);
	}
	string QuickCoord(int idx)
    {
		return string.Format("{0}{1}", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[idx % boardWidth], 1 + idx / boardWidth);
    }
	

	void HandleReset()
    {
		if (!cardIdxes.Any())
		{
			GeneratePuzzle();
			UpdateBoard();
			return;
		}
		if (!moduleSolved && CountMovesCurState() > 0 && !cardIdxes.SequenceEqual(initialCardIdxes))
		{
			QuickLog("Resetted from board state in reading order: {0}", cardIdxes.Select(a => a + 1).Join(", "));
			if (!disableStrike)
				modSelf.HandleStrike();
		}
		cardIdxes = initialCardIdxes.ToList();
		idxCardSelected = -1;
		RenderCurCardHL();
		UpdateBoard();
    }

	void UpdateBoard(List<int> tileIdxes = null, bool useCurPositions = false)
    {
		var affectedIdxes = tileIdxes ?? new List<int>();
		var oldCount = affectedIdxes.Count + cardIdxes.Count;

		var idxOldList = Enumerable.Range(0, oldCount).ToList();
		var idxNewList = idxOldList.Except(affectedIdxes).ToList();

        counterMesh.text = CountMovesCurState().ToString();
		counterMesh.color = CountMovesCurState() > 0 ? Color.white : cardIdxes.Any() ? Color.red : Color.green;
		for (var x = 0; x < cards.Length; x++)
		{
			cards[x].gameObject.SetActive(x < cardIdxes.Count);
			cards[x].backRender.material.SetTexture("_MainTex", usedSuits[0]);
			if (x >= cardIdxes.Count) continue;

			var curX = x % boardWidth;
			var curY = x / boardWidth;
			var newPos = new Vector3(startX + deltaX * curX, 0, startY + deltaY * curY);

			var lastX = idxOldList.IndexOf(idxNewList[x]);
			var oldX = lastX % boardWidth;
			var oldY = lastX / boardWidth;
			var oldPos = new Vector3(startX + deltaX * oldX, 0, startY + deltaY * oldY);

			var curIdxCard = cardIdxes[x];
			cards[x].frontRender.material.SetTexture("_MainTex", usedSuits[curIdxCard]);
			cards[x].frontRender.material.color = usedColors[curIdxCard];
			if (cards[x].animHandler != null)
				StopCoroutine(cards[x].animHandler);
			var y = x;
			cards[x].animHandler = HandleShiftAnim(
				startPos: useCurPositions ? cards[y].affectedObject.transform.localPosition : oldPos,
				endPos: newPos, affectedObject: cards[y].affectedObject.transform, speed: 5f);
			StartCoroutine(cards[x].animHandler);
			cards[x].cbTextMesh.text = colorblindEnabled ? alphabet[curIdxCard].ToString() : "";
			//StartCoroutine(HandleFlipCardIdx(x));
		}
	}
	void UpdateBoardInstantly()
    {
		for (var x = 0; x < cardIdxes.Count; x++)
        {
			if (x >= cards.Length) break;
			var curIdxCard = cardIdxes[x];

			var curX = x % boardWidth;
			var curY = x / boardWidth;

			cards[x].gameObject.SetActive(true);
			cards[x].affectedObject.localPosition = new Vector3(startX + deltaX * curX, 0, startY + deltaY * curY);

			//cards[x].affectedObject.localRotation = Quaternion.Euler(Vector3.forward * 180);
			cards[x].frontRender.material.SetTexture("_MainTex",usedSuits[curIdxCard]);
			cards[x].frontRender.material.color = usedColors[curIdxCard];
			cards[x].cbTextMesh.text = colorblindEnabled ? alphabet[curIdxCard].ToString() : "";
			//StartCoroutine(HandleFlipCardIdx(x));
		}
		for (var x = cardIdxes.Count; x < cards.Length; x++)
		{
			cards[x].gameObject.SetActive(false);
			var curX = x % boardWidth;
			var curY = x / boardWidth;
			cards[x].affectedObject.localPosition = new Vector3(startX + deltaX * curX, 0, startY + deltaY * curY);
		}
		counterMesh.text = CountMovesCurState().ToString();
		counterMesh.color = CountMovesCurState() > 0 ? Color.white : cardIdxes.Any() ? Color.red : Color.green;
	}

	void RenderCurCardHL()
    {
		for (var x = 0; x < cards.Length; x++)
		{
			var curCard = cards[x];
			var isCurCard = idxCardSelected == x;
			foreach (var renderer in curCard.border)
				renderer.material.color = isCurCard ? Color.white : Color.black;
		}
	}

	void HandleCardSelect(int idx)
    {
		if (idx >= cardIdxes.Count) return;

		mAudio.PlaySoundAtTransform("Cardflip", cardSelectables[idx].transform);
		if (idxCardSelected == idx)
			idxCardSelected = -1;
		else if (idxCardSelected == -1)
			idxCardSelected = idx;
		else
		{
			var possibleMovesCurIdx = allowedCardPairs.Where(a => a.All(b => b < cardIdxes.Count) && a.Contains(idxCardSelected) && a.Contains(idx));
			if (possibleMovesCurIdx.Count() == 1 &&
				possibleMovesCurIdx.Single(a => a.Contains(idxCardSelected) && a.Contains(idx)).Select(a => cardIdxes[a]).Distinct().Count() == 1)
			{
				var foundPair = possibleMovesCurIdx.Single(a => a.Contains(idxCardSelected) && a.Contains(idx));
				foreach (var idxCard in foundPair.OrderByDescending(a => a))
					cardIdxes.RemoveAt(idxCard);
				UpdateBoard(new List<int> { idxCardSelected, idx });
				if (CountMovesCurState() == 0 && !moduleSolved)
					if (cardIdxes.Any())
					{
						QuickLog("Ran out of moves on the current board state: {0}", cardIdxes.Select(a => a + 1).Join(", "));
						modSelf.HandleStrike();
					}
					else
					{
						QuickLog("Cleared. Boards generated after this solve are not logged.");
						moduleSolved = true;
						modSelf.HandlePass();
					}
				idxCardSelected = -1;
			}
			else
				idxCardSelected = idx;
		}
		RenderCurCardHL();
    }

	IEnumerator HandleShiftAnim(Vector3 startPos, Vector3 endPos, Transform affectedObject, float speed = 2f)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime * speed)
        {
			affectedObject.localPosition = Vector3.Lerp(startPos, endPos, t);
			yield return null;
        }
		affectedObject.localPosition = endPos;
    }

	IEnumerator HandleFlipCardIdx(int idx)
    {
		if (idx >= cards.Length) yield break;
		var lastRotation = cards[idx].affectedObject.localRotation;
		var newRotation = lastRotation * Quaternion.Euler(Vector3.forward * 180);
		for (float t = 0; t < 1f; t += Time.deltaTime)
        {
			cards[idx].affectedObject.localRotation = lastRotation * Quaternion.Euler(Vector3.forward * t * 180);
			yield return null;
		}
		cards[idx].affectedObject.localRotation = newRotation;

	}

	void GeneratePuzzle()
    {
		usedColors = possibleColors.ToArray().Shuffle().Take(typesPicked).ToArray();
		usedSuits = possibleCardSuits.ToArray().Shuffle().Take(typesPicked).ToArray();
		do
		{
			cardIdxes.Clear();
			intendedSolutionPath.Clear();

			var expectedCardAmount = Random.Range(17,25);
			while (cardIdxes.Count() < expectedCardAmount)
			{
				var nextCardAmount = cardIdxes.Count + 2;
				var allowedMatchPairsNew = allowedCardPairs.Where(a => a.All(b => b < nextCardAmount));
				if (!allowedMatchPairsNew.Any()) break;
				var pickedPair = allowedMatchPairsNew.PickRandom();
				var randomIdxPicked = Random.Range(0, typesPicked);

				foreach (var idx in pickedPair)
				{
					if (idx >= cardIdxes.Count)
						cardIdxes.Add(randomIdxPicked);
					else
						cardIdxes.Insert(idx, item: randomIdxPicked);
				}
				if (intendedSolutionPath.Any())
					intendedSolutionPath.Insert(0, pickedPair);
				else
					intendedSolutionPath.Add(pickedPair);
			}
		}
		while (cardIdxes.Distinct().Count() < preferredTypes);
		// Compact idxes so that all are indexed from 0 - # types of cards.
		var curTypes = cardIdxes.Distinct();
		//Debug.Log(curTypes.Distinct().Join());
		var curTypeCnt = curTypes.Count();
		var idxCompact = Enumerable.Range(0, curTypeCnt).ToList();
		idxCompact.Shuffle();
		if (!curTypes.OrderBy(a => a).SequenceEqual(Enumerable.Range(0, curTypeCnt)))
			cardIdxes = cardIdxes.Select(a => idxCompact[curTypes.IndexOf(b => b == a)]).ToList();
		//Debug.Log(cardIdxes.Distinct().Join());
		// Make the initial cards the result of the cards generated from the puzzle.
		initialCardIdxes = cardIdxes.ToList();
    }
	
	// Update is called once per frame
	void Update () {
		if (timeHeld < 2f && resetHeld)
			timeHeld += Time.deltaTime;
		else if (!resetHeld && timeHeld > 0f)
			timeHeld = Mathf.Clamp01(timeHeld - Time.deltaTime * 5);
		resetRender.material.color = Color.Lerp(Color.black, Color.red, Easing.InCirc(Mathf.Clamp01(timeHeld), 0f, 1f, 1f));
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = "\"!{0} A1 B2 C3 D4 E5\" [Presses the specified card in that position, Rows 1-5 top to bottom, Columns A-E left to right] | \"!{0} reset\" [Resets the puzzle, MAY CAUSE STRIKES] | \"!{0} cb/colorblind\" [Toggles colorblind mode]";
#pragma warning restore 414
	readonly string RowIDXScan = "12345", ColIDXScan = "abcde";

	IEnumerator ProcessTwitchCommand(string cmd)
    {
		var regexReset = Regex.Match(cmd, @"^reset$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		var regexCB = Regex.Match(cmd, @"^(colou?rblind|cb)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (regexReset.Success)
        {
			yield return null;
			resetSelectable.OnInteract();
			while (timeHeld < 1f)
				yield return null;
			resetSelectable.OnInteractEnded();
		}
		else if (regexCB.Success)
        {
			yield return null;
			colorblindEnabled ^= true;
			UpdateBoard();
        }
		else
        {
			var intCmd = cmd.ToLowerInvariant().Trim();
			var allIdxesToPress = new List<int>();
			var boardSelectIdxes = Enumerable.Range(0, cardIdxes.Count).ToList();
			if (intCmd.StartsWith("press "))
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
				var expectedIdx = boardWidth * rowIdx + colIdx;
				if (rowIdx != -1 && colIdx != -1 && boardSelectIdxes.Contains(expectedIdx))
					allIdxesToPress.Add(boardSelectIdxes.IndexOf(expectedIdx));
				else
				{
					yield return string.Format("sendtochaterror The command portion \"{0}\" does not correspond to a selectable tile!", portion);
					yield break;
				}
			}
			for (var x = 0; x < allIdxesToPress.Count; x++)
			{
				yield return null;
				cardSelectables[allIdxesToPress[x]].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}

	}

	IEnumerator TwitchHandleForcedSolve()
    {
		disableStrike = true;
		if (!cardIdxes.SequenceEqual(initialCardIdxes))
		{
			resetSelectable.OnInteract();
			while (timeHeld < 1f)
				yield return true;
			resetSelectable.OnInteractEnded();
		}
		foreach (var pair in intendedSolutionPath)
        {
			var pairShuffled = pair.ToArray().Shuffle();
			cardSelectables[pairShuffled[0]].OnInteract();
			yield return new WaitForSeconds(0.1f);
			cardSelectables[pairShuffled[1]].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

    }
}
