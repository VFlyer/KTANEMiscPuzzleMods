using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class P03PuzzleScript : MonoBehaviour {

	enum ComponentType
    {
		Empty,
		Zero,
		Pos,
		Neg,
		Breaker
    }
	Dictionary<int, List<ComponentType[]>> offsetModifs = new Dictionary<int, List<ComponentType[]>> {
		{ -2, new List<ComponentType[]> { new[] { ComponentType.Neg, ComponentType.Neg} }  },
		{ -1, new List<ComponentType[]> { new[] { ComponentType.Neg, ComponentType.Zero} }  },
		{ 0, new List<ComponentType[]> { new[] { ComponentType.Neg, ComponentType.Pos}, new[] { ComponentType.Zero, ComponentType.Zero} }  },
		{ 1, new List<ComponentType[]> { new[] { ComponentType.Zero, ComponentType.Pos} }  },
		{ 2, new List<ComponentType[]> { new[] { ComponentType.Pos, ComponentType.Pos} }  },

	};


	public KMAudio mAudio;
	public KMBombModule modSelf;
	public KMSelectable[] componentSelectables;
	public P03PuzzleCell[] puzzleCells;
	public TextMesh[] resultTexts, componentTexts;
	public KMSelectable submitBtn, resetBtn;

	int[][] initialPowersEach;
	ComponentType[][] componentsPlaced;
	int[] expectedPowers, amountComponents, initialAmountComponents;

	static int modIDCnt;
	int moduleID;
	int length = 6, boards = 2; // Length is how long the board is, board is how many sets will need to be met to disarm the module.
	bool submitting = false, interactable = false;
	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		GeneratePuzzle();
	}
	void GeneratePuzzle()
    {
		initialPowersEach = new int[boards][];
		componentsPlaced = new ComponentType[boards][];
		expectedPowers = new int[boards];
		for (var x = 0; x < boards; x++)
			initialPowersEach[x] = new int[length];
		//retryGen:
		for (var x = 0; x < boards; x++)
			componentsPlaced[x] = new ComponentType[length];
		initialAmountComponents = new int[4]; // Consists of Zero, Pos, Neg, and Null, in that order.
		var allowedComponents = new[] { ComponentType.Zero, ComponentType.Pos, ComponentType.Neg, ComponentType.Breaker,  };
		for (var x = 0; x < boards; x++)
			for (var y = 0; y < length; y++)
				initialPowersEach[x][y] = Random.Range(-2, 5); // Setup the initial powers for each type.
		var solutionComponentsPlaced = componentsPlaced.Select(a => a.ToArray()).ToArray();
        for (var row = 0; row < boards; row++)
        {
			var cntCompsToPlace = Random.Range(2, 5);
			var idxFocusPos = Enumerable.Range(0, length).ToArray().Shuffle().Take(cntCompsToPlace).ToArray();
			foreach (var idxSelected in idxFocusPos)
			{
				var pickedComponent = allowedComponents.PickRandom();
				solutionComponentsPlaced[row][idxSelected] = pickedComponent;
				initialAmountComponents[(int)pickedComponent - 1]++;
			}
        }
		amountComponents = initialAmountComponents.ToArray();
		expectedPowers = Enumerable.Range(0, boards).Select(a => CalculatePowerFromSpecificBoard(solutionComponentsPlaced[a], initialPowersEach[a])).ToArray();
		QuickLog("One possible solution:");
		for (var x = 0; x < boards; x++)
			QuickLog("Board {0}: Goal = {2}; Layout = [ {1} ]", x + 1, Enumerable.Range(0, length).Select(a => solutionComponentsPlaced[x][a] == ComponentType.Empty ? initialPowersEach[x][a].ToString() : solutionComponentsPlaced[x][a].ToString()).Join(", "), expectedPowers[x]);
		QuickLog("Initial State:");
		for (var x = 0; x < boards; x++)
			QuickLog("Board {0}: [ {1} ]", x + 1, initialPowersEach[x].Join(", "));
        QuickLog("Amount of each components: {0}", Enumerable.Range(0, allowedComponents.Length).Select(a => string.Format("[{0} = {1}]", allowedComponents[a].ToString(), initialAmountComponents[a])).Join());
		interactable = true;
		UpdateBoard();
	}
	void UpdateBoard()
    {
		for (var x = 0; x < puzzleCells.Length; x++)
        {
			var cellIdx = x % length;
			var boardIdx = x / length;
			puzzleCells[x].displayMesh.text = componentsPlaced[boardIdx][cellIdx] != ComponentType.Empty ? "" : initialPowersEach[boardIdx][cellIdx].ToString();
        }
        for (var x = 0; x < resultTexts.Length; x++)
			resultTexts[x].text = string.Format("??\n---\n{0}", expectedPowers[x].ToString("00"));
    }
	void HandleReset()
    {
		if (submitting)
        {
			submitting = false;
        }
		else
        {
			for (var x = 0; x < componentsPlaced.Length; x++)
				for (var y = 0; y < componentsPlaced[x].Length; y++)
					componentsPlaced[x][y] = ComponentType.Empty;
			amountComponents = initialAmountComponents.ToArray();
        }
		UpdateBoard();
	}
	void HandleSubmit()
    {
		if (!submitting)
        {
			submitting = true;
			interactable = false;
			StartCoroutine(HandleSubmitAnim());
        }
	}
	IEnumerator HandleSubmitAnim()
    {
		var duplicatedBoardValues = initialPowersEach.Select(a => a.ToArray()).ToArray();
		for (var boardIdx = 0; boardIdx < boards; boardIdx++)
		{
			var placedComps = componentsPlaced[boardIdx];
			var idxComponentsPlaced = Enumerable.Range(0, length).Where(a => placedComps[a] != ComponentType.Empty).ToArray();
			var idxBreakerComp = idxComponentsPlaced.Where(a => placedComps[a] == ComponentType.Breaker).ToArray();
			var idxModifierComp = idxComponentsPlaced.Where(a => placedComps[a] != ComponentType.Breaker).ToArray();
			for (var i = 0; i < idxModifierComp.Length; i++)
				for (var j = i + 1; j < idxModifierComp.Length; j++)
				{
					var rangeItems = Enumerable.Range(idxModifierComp[i], idxModifierComp[j] + 1 - idxModifierComp[i]);
					if (!rangeItems.Any(a => idxBreakerComp.Contains(a))) // Check if the item in between is a breaker, including the last and first items.
					{
						var powerOffsetObtained = (placedComps[i] == ComponentType.Pos ? 1 : placedComps[i] == ComponentType.Neg ? -1 : 0) + (placedComps[j] == ComponentType.Pos ? 1 : placedComps[j] == ComponentType.Neg ? -1 : 0);
						foreach (var item in rangeItems.Take(rangeItems.Count() - 1).Skip(1))
							duplicatedBoardValues[boardIdx][item] += powerOffsetObtained;
					}
				}
		}
		if (CalculatePowerFromAllBoards().SequenceEqual(expectedPowers))
        {
			modSelf.HandlePass();
        }
		else
        {
			interactable = true;
			modSelf.HandleStrike();
		}
		yield break;
    }
	int CalculatePowerFromSpecificBoard(ComponentType[] placedComps, IEnumerable<int> powers)
    {
		var idxComponentsPlaced = Enumerable.Range(0, length).Where(a => placedComps[a] != ComponentType.Empty).ToArray();
		var idxBreakerComp = idxComponentsPlaced.Where(a => placedComps[a] == ComponentType.Breaker).ToArray();
		var idxModifierComp = idxComponentsPlaced.Where(a => placedComps[a] != ComponentType.Breaker).ToArray();
		var finalPowersEach = powers.ToArray();
		for (var i = 0; i < idxModifierComp.Length; i++)
			for (var j = i + 1; j < idxModifierComp.Length; j++)
			{
				var rangeItems = Enumerable.Range(idxModifierComp[i], idxModifierComp[j] + 1 - idxModifierComp[i]);
				if (!rangeItems.Any(a => idxBreakerComp.Contains(a))) // Check if the item in between is a breaker, including the last and first items.
				{
					var powerOffsetObtained = (placedComps[i] == ComponentType.Pos ? 1 : placedComps[i] == ComponentType.Neg ? -1 : 0) + (placedComps[j] == ComponentType.Pos ? 1 : placedComps[j] == ComponentType.Neg ? -1 : 0);
					foreach (var item in rangeItems.Take(rangeItems.Count() - 1).Skip(1))
						finalPowersEach[item] += powerOffsetObtained;
				}
			}
		return Enumerable.Range(0, length).Except(idxComponentsPlaced).Sum(a => finalPowersEach[a]);
    }
	int[] CalculatePowerFromAllBoards()
    {
		var output = new int[boards];
        for (var x = 0; x < boards; x++)
        {
			var y = x;
			var idxComponentsPlaced = Enumerable.Range(0, length).Where(a => componentsPlaced[y][a] != ComponentType.Empty).ToArray();
			var idxBreakerComp = idxComponentsPlaced.Where(a => componentsPlaced[y][a] == ComponentType.Breaker).ToArray();
			var idxModifierComp = idxComponentsPlaced.Where(a => componentsPlaced[y][a] != ComponentType.Breaker).ToArray();
			var finalPowersEach = initialPowersEach[x].ToArray();
			for (var i = 0; i < idxModifierComp.Length; i++)
				for (var j = i + 1; j < idxModifierComp.Length; j++)
				{
					var rangeItems = Enumerable.Range(idxModifierComp[i], idxModifierComp[j] + 1 - idxModifierComp[i]);
					if (!rangeItems.Any(a => idxBreakerComp.Contains(a))) // Check if the item in between is a breaker, including the last and first items.
					{
						var powerOffsetObtained = offsetModifs.Single(a => a.Value.Any(b => b.SequenceEqual(new[] { componentsPlaced[y][i], componentsPlaced[y][j] }) || b.SequenceEqual(new[] { componentsPlaced[y][j], componentsPlaced[y][i] }))).Key;
						foreach (var item in rangeItems.Take(rangeItems.Count() - 1).Skip(1))
							finalPowersEach[item] += powerOffsetObtained;
					}
				}
			output[x] = Enumerable.Range(0, length).Except(idxComponentsPlaced).Sum(a => finalPowersEach[a]);
		}
		return output;
    }

}
