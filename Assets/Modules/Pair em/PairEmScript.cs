using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PairEmScript : MonoBehaviour {

	public KMBombModule modSelf;
	public KMAudio mAudio;

	public PairEmCard[] cards, ancilleryCards;

	public KMSelectable[] cardSelectables;
	public KMSelectable resetSelectable;
	public MeshRenderer resetRender;

	
	List<int> cardIdxes = new List<int>(), initialCardIdxes;
	List<int[]> allowedCardPairs = new List<int[]>(), intendedSolutionPath;

	public Color[] possibleColors;
	public Texture[] possibleCardSuits;

	public TextMesh counterMesh;

	bool moduleSolved = false, resetHeld, disableStrike;

	const int boardWidth = 5, cardCount = 25;
	int moduleID;
	static int modIDCnt;

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
	void Start () {
		moduleID = ++modIDCnt;
		intendedSolutionPath = new List<int[]>();
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
		QuickLog("Initial state in reading order: {0}", cardIdxes.Select(a => a + 1).Join(","));
		QuickLog("Intended Solution: [{0}]", intendedSolutionPath.Select(a => a.Select(b => QuickCoord(b)).Join(",")).Join("];["));
		UpdateBoardInstantly();
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
			if (timeHeld >= 1f)
				HandleReset();
			resetHeld = false;
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
			return;
		}
		if (!disableStrike && !moduleSolved && CountMovesCurState() > 0 && !cardIdxes.SequenceEqual(initialCardIdxes))
		{
			QuickLog("Resetted from board state in reading order: {0}", cardIdxes.Select(a => a + 1).Join(", "));
			modSelf.HandleStrike();
		}
		cardIdxes = initialCardIdxes.ToList();
		UpdateBoardInstantly();
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
				UpdateBoardInstantly();
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
    }

	IEnumerator HandleShiftAnim(Vector3 lastPos, Vector3 endPos, Transform affectedObject)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
			affectedObject.localPosition = Vector3.Lerp(lastPos, endPos, t);
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

			var expectedCardAmount = Random.Range(19,25);
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
			cardSelectables[pair[0]].OnInteract();
			yield return new WaitForSeconds(0.1f);
			cardSelectables[pair[1]].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

    }
}
