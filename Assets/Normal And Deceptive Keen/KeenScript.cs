using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KeenScript : MonoBehaviour {
	public KMAudio mAudio;
	public KMSelectable[] squareSelectable;
	public KMSelectable selfSelectable;
	public TextMesh[] gridText, clueText;
	public MeshRenderer[] borderRenderers;
    protected UniqueGridGenerator uniqueGrid;

	protected List<List<int>> groupedPairIdxes;
	protected List<int> operatorIdx, valueGroup;
	static int modIDCnt;
	protected int modID;

	protected enum ValidityStates
    {
		Allowed,
		Specific,
		Forbidden
    }


	protected virtual void QuickLog(string value, params object[] args)
    {
		Debug.LogFormat("[Keen #{0}] {1}", modID, string.Format(value, args));
    }

	// Use this for initialization
	protected virtual void Start () {
		modID = ++modIDCnt;
		uniqueGrid = new UniqueGridGenerator();
		var displayedValues = uniqueGrid.GetGrid();
		groupedPairIdxes = new List<List<int>>();

		var edgeIdxesAll = new List<int[]>();

		var maxWidth = displayedValues.GetLength(0);
		for (var x = 0; x < maxWidth; x++)
        {
			for (var y = 0; y < maxWidth; y++)
			{
				if (x + 1 < maxWidth)
					edgeIdxesAll.Add(new int[] { maxWidth * y + x, maxWidth * y + x + 1 });
				if (y + 1 < maxWidth)
					edgeIdxesAll.Add(new int[] { maxWidth * y + x, maxWidth * (y + 1) + x});
			}
		}
		operatorIdx = new List<int>();
		valueGroup = new List<int>();
		var solvableUnique = true;
		var curIterCount = 0;
		do
		{
			groupedPairIdxes.Clear();
			edgeIdxesAll.Shuffle();
			//Debug.LogFormat("[{0}]", edgeIdxesAll.Select(a => a.Join(",")).Join("];["));
			foreach (int[] edgePair in edgeIdxesAll)
			{
				var firstIdx = edgePair.First();
				var lastIdx = edgePair.Last();

				var firstGroupIdx = -1;
				var lastGroupIdx = -1;
				for (var x = 0; x < groupedPairIdxes.Count && (firstGroupIdx == -1 || lastGroupIdx == -1); x++)
				{
					var curGroup = groupedPairIdxes.ElementAt(x);
					if (curGroup.Contains(firstIdx) && firstGroupIdx == -1)
						firstGroupIdx = x;
					if (curGroup.Contains(lastIdx) && lastGroupIdx == -1)
						lastGroupIdx = x;
				}
				if (firstGroupIdx == -1 && lastGroupIdx == -1)
				{
					var newGroupIdx = new List<int>() { firstIdx, lastIdx };
					groupedPairIdxes.Add(newGroupIdx);
				}
				else if (firstGroupIdx == -1)
				{
					var currentGroup = groupedPairIdxes.ElementAt(lastGroupIdx);
					currentGroup.Add(firstIdx);
				}
				else if (lastGroupIdx == -1)
				{
					var currentGroup = groupedPairIdxes.ElementAt(firstGroupIdx);
					currentGroup.Add(lastIdx);
				}
			}
			operatorIdx.Clear();
			valueGroup.Clear();
			for (var x = 0; x < groupedPairIdxes.Count; x++)
			{
				var _2CellOperatorIdxes = new[] { 2, 3 };
				var AnyCellOperatorIdxes = new[] { 0, 1 };
				var allowedCombinations = groupedPairIdxes[x].Count != 2 ? AnyCellOperatorIdxes : AnyCellOperatorIdxes.Concat(_2CellOperatorIdxes);
				var allValuesInGroup = groupedPairIdxes[x].Select(a => displayedValues[a % maxWidth, a / maxWidth]);
				var pickedOperatorIdx = allowedCombinations.PickRandom();
				switch (pickedOperatorIdx)
				{
					case 0:
						valueGroup.Add(allValuesInGroup.Sum());
						break;
					case 1:
						var product = 1;
						foreach (int value in allValuesInGroup)
							product *= value;
						valueGroup.Add(product);
						break;
					case 2:
						valueGroup.Add(Mathf.Abs(allValuesInGroup.First() - allValuesInGroup.Last()));
						break;
					case 3:
						if (allValuesInGroup.First() % allValuesInGroup.Last() == 0)
						{
							valueGroup.Add(allValuesInGroup.First() / allValuesInGroup.Last());
							break;
						}
						else if (allValuesInGroup.Last() % allValuesInGroup.First() == 0)
						{
							valueGroup.Add(allValuesInGroup.Last() / allValuesInGroup.First());
							break;
						}
						pickedOperatorIdx = allowedCombinations.Where(a => a != 3).PickRandom();
						if (pickedOperatorIdx == 0)
							goto case 0;
						else if (pickedOperatorIdx == 1)
							goto case 1;
						goto case 2;
				}

				operatorIdx.Add(pickedOperatorIdx);
			}

			var allCombinations = new List<int>[uniqueGrid.GetSize()];
			for (var x = 0; x < allCombinations.Length; x++)
			{
				allCombinations[x] = Enumerable.Range(1, 6).ToList();
			}
			var canIterate = false;
			do
			{
				canIterate = false;
				for (var x = 0; x < allCombinations.Length; x++)
				{
					var idxGroupAssociated = Enumerable.Range(0, groupedPairIdxes.Count).Single(a => groupedPairIdxes[a].Contains(x));
					var curX = x % maxWidth; // Actually top to bottom
					var curY = x / maxWidth; // Actually left to right

					var curConfiguration = allCombinations[x];
					var curNumState = ValidityStates.Allowed;
					var specifiedDigits = new List<int>();
					foreach (int num in curConfiguration)
                    {
						var uniqueNumInRow = true;
						// Check if there is only 1 placement for that number in that row.
						for (var delta = 1; delta < 6; delta++)
                        {
							if (allCombinations[(delta + curX) % maxWidth + maxWidth * curY].Contains(num))
								uniqueNumInRow = false;
                        }
						if (uniqueNumInRow)
						{
							curNumState = ValidityStates.Specific;
							specifiedDigits.Add(num);
							break;
						}
						var uniqueNumInCol = true;
                        // Check if there is only 1 placement for that number in that column.
                        for (var delta = 1; delta < 6; delta++)
                        {
							if (allCombinations[curX + maxWidth * ((curY + delta) % maxWidth)].Contains(num))
								uniqueNumInCol = false;
                        }
						if (uniqueNumInCol)
						{
							curNumState = ValidityStates.Specific;
							specifiedDigits.Add(num);
							break;
						}
						var numInOnlyCellInRowCol = false;
                        // Check if there is there is a number that is only present in that row or column.
                        for (var delta = 1; delta < 6; delta++)
                        {
							var curCellDeltaY = allCombinations[curX + maxWidth * ((curY + delta) % maxWidth)];
							var curCellDeltaX = allCombinations[(curX + delta) % maxWidth + maxWidth * curY];
							if ((curCellDeltaY.Contains(num) && curCellDeltaY.Count == 1) || (curCellDeltaX.Contains(num) && curCellDeltaX.Count == 1))
								numInOnlyCellInRowCol = true;
                        }
						if (numInOnlyCellInRowCol)
						{
							curNumState = ValidityStates.Forbidden;
							specifiedDigits.Add(num);
						}
						// Now check based on the operator associated with this, and if it is possible to reach.
						var remainingValue = valueGroup[idxGroupAssociated];
						switch (operatorIdx[idxGroupAssociated])
						{
							case 0:
								foreach (var idxPossiblity in groupedPairIdxes[idxGroupAssociated])
									remainingValue -= allCombinations[idxPossiblity].Count == 1 ? allCombinations[idxPossiblity].Single() : 0;
								if (remainingValue - num < groupedPairIdxes[idxGroupAssociated].Count(a => allCombinations[a].Count == 1) || remainingValue - num > 6 * groupedPairIdxes[idxGroupAssociated].Count(a => allCombinations[a].Count != 1))
								{
									curNumState = ValidityStates.Forbidden;
									specifiedDigits.Add(num);
								}
								break;
							case 1:
								foreach (var idxPossiblity in groupedPairIdxes[idxGroupAssociated])
									remainingValue /= allCombinations[idxPossiblity].Count == 1 ? allCombinations[idxPossiblity].Single() : 1;
								if (remainingValue / num < 1 || remainingValue % num != 0)
								{
									curNumState = ValidityStates.Forbidden;
									specifiedDigits.Add(num);
								}
								break;
							case 2:
								if (!Enumerable.Range(1, maxWidth - remainingValue).Any(a => a == num || a + num == remainingValue))
                                {
									curNumState = ValidityStates.Forbidden;
									specifiedDigits.Add(num);
								}
								break;
							case 3:
								if (!Enumerable.Range(1, maxWidth / remainingValue).Any(a => a == num || a * num == remainingValue))
								{
									curNumState = ValidityStates.Forbidden;
									specifiedDigits.Add(num);
								}
								break;
						}
					}
					if (curNumState == ValidityStates.Specific)
					{
						var lastCnt = curConfiguration.Count;
						curConfiguration.RemoveAll(a => !specifiedDigits.Contains(a));
						canIterate = lastCnt != curConfiguration.Count;
					}
					else if (curNumState == ValidityStates.Forbidden)
                    {
						curConfiguration.RemoveAll(a => specifiedDigits.Contains(a));
						canIterate = true;
					}
				}

			}
			while (canIterate);
			Debug.LogFormat("[{0}]",allCombinations.Select(a => a.OrderBy(b => b).Join(",")).Join("];["));
			solvableUnique = allCombinations.All(a => a.Count == 1);
			curIterCount++;
		}
		while (!solvableUnique && curIterCount < 32);

		Debug.LogFormat("[{0}]",groupedPairIdxes.Select(a => a.OrderBy(b => b).Join(",")).Join("];["));

		
		// Render the solution.
		for (var x = 0; x < gridText.Length; x++)
		{
			var rowIdx = x % 6;
			var colIdx = x / 6;
			gridText[x].text = displayedValues[rowIdx, colIdx].ToString();
		}
		// Render the puzzle.
		for (var x = 0; x < borderRenderers.Length; x++)
        {
            List<int> usableGroup = null;
            for (var n = 0; n < groupedPairIdxes.Count; n++)
            {
                var curGroup = groupedPairIdxes.ElementAt(n);
                if (curGroup.Contains(x))
                {
                    usableGroup = curGroup;
                    break;
                }
            }
            if (usableGroup != null)
            {
				var curX = x % maxWidth; // Actually top to bottom
				var curY = x / maxWidth; // Actually left to right


				var isAdjacent = new[] {

					curX < maxWidth - 1 && usableGroup.Contains(x + 1),
					curY > 0 && usableGroup.Contains(x - maxWidth),
					curY < maxWidth - 1 && curX < maxWidth - 1 && usableGroup.Contains(x + maxWidth) && usableGroup.Contains(x + 1) && usableGroup.Contains(x + 1 + maxWidth),
					curY > 0 && curX < maxWidth - 1 && usableGroup.Contains(x - maxWidth) && usableGroup.Contains(x + 1) && usableGroup.Contains(x + 1 - maxWidth),
					

					curX > 0 && usableGroup.Contains(x - 1),
					curY < maxWidth - 1 && usableGroup.Contains(x + maxWidth),
					curY < maxWidth - 1 && curX > 0 && usableGroup.Contains(x + maxWidth) && usableGroup.Contains(x - 1) && usableGroup.Contains(x - 1 + maxWidth),
					curY > 0 && curX > 0 && usableGroup.Contains(x - maxWidth) && usableGroup.Contains(x - 1) && usableGroup.Contains(x - 1 - maxWidth),
					
				};
                borderRenderers[x].material.mainTextureOffset = new Vector2(
                    Enumerable.Range(0, 4).Where(a => isAdjacent[a]).Select(a => 1 << a).Sum() / 16f,
                    (15 - Enumerable.Range(0, 4).Where(a => isAdjacent[a + 4]).Select(a => 1 << a).Sum()) / 16f);
            }
        }
		for (var x = 0; x < groupedPairIdxes.Count; x++)
        {
			var curGroup = groupedPairIdxes[x].OrderBy(a => a);
			for (var y = 0; y < curGroup.Count(); y++)
			{
				clueText[curGroup.ElementAt(y)].text = y == 0 ? string.Format("{0}{1}", valueGroup[x], "+*-/"[operatorIdx[x]]) : "";
			}
		}
        Debug.LogFormat("[{0}]", groupedPairIdxes.Select(a => a.OrderBy(b => b).Select(b => displayedValues[b % maxWidth, b / maxWidth]).Join(",")).Join("];["));
		Debug.LogFormat("[{0}]", valueGroup.Join("],["));
		Debug.LogFormat("[{0}]", operatorIdx.Select(a => "+*-/"[a]).Join("],["));
	}
	protected int ObtainGCM(int numA, params int[] numsB)
	{
		var output = numA;

		for (var x = 0; x < numsB.Length; x++)
		{
			var pairValues = new List<int>() { output, numsB[x] };
			while (pairValues.Min() != 0)
			{
				if (pairValues[0] < pairValues[1])
					pairValues[1] %= pairValues[0];
				else
					pairValues[0] %= pairValues[1];
			}
			if (pairValues[0] == 0)
				output = pairValues[1];
			else if (pairValues[1] == 0)
				output = pairValues[0];
		}

		return output;
	}
}
