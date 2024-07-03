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
	public KMSelectable[] gridsSelectable, componentSelectables;
	public KMSelectable submitBtn, resetBtn;

	int[][] initialPowersEach;
	ComponentType[][] componentsPlaced;
	int[] expectedPowers, amountComponents, initialAmountComponents;

	static int modIDCnt;
	int moduleID;
	int length = 6, boards = 2; // Length is how long the board is, board is how many sets will need to be met to disarm the module.

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
		retryGen:
		for (var x = 0; x < boards; x++)
			componentsPlaced[x] = new ComponentType[length];
		amountComponents = new int[4]; // Consists of Neg, Zero, Pos, and Null, in that order.
		for (var x = 0; x < boards; x++)
			for (var y = 0; y < length; y++)
				initialPowersEach[x][y] = Random.Range(-2, 5); // Setup the initial powers for each type.
		var solutionComponentsPlaced = componentsPlaced.Select(a => a.ToArray()).ToArray();
	}


	int CalculatePowerFromSpecificBoard(ComponentType[] placedComps, int[] powers)
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
					var powerOffsetObtained = offsetModifs.Single(a => a.Value.Any(b => b.SequenceEqual(new[] { placedComps[i], placedComps[j] }) || b.SequenceEqual(new[] { placedComps[j], placedComps[i] }))).Key;
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
