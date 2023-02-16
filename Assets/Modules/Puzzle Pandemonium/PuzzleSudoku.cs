using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleSudoku : PuzzleGeneric {

    int[] finalBoard, currentBoard;
	bool[] lockInputs;

    public TextMesh[] usedTextMeshes;
	IEnumerable<List<int>> CollapseGrid(IEnumerable<List<int>> combinationsCollapse)
	{
		if (combinationsCollapse.Count() != 16) return combinationsCollapse;
		var checkOffsetsX = new[] { 0, 1, 2, 3 }; // Offset checks for X.
		var checkOffsetsY = new[] { 0, 4, 8, 12 }; // Offset checks for Y.
		var checkOffsetsXYBox = new[] { 0, 1, 4, 5 }; // Offset checks for XY box.

		var idxesCheckX = Enumerable.Range(0, 16).Where(a => a % 4 == 0);
		var idxesCheckY = Enumerable.Range(0, 16).Where(a => a < 4);
		var idxesCheckXYBox = Enumerable.Range(0, 16).Where(a => a % 2 == 0 && (a >> 2) % 2 == 0);
		//Debug.Log(idxesCheckX.Join());
		//Debug.Log(idxesCheckY.Join());
		//Debug.Log(idxesCheckXYBox.Join());

		var groupedIdxesX = idxesCheckX.Select(a => checkOffsetsX.Select(b => a + b).ToArray());
		var groupedIdxesY = idxesCheckY.Select(a => checkOffsetsY.Select(b => a + b).ToArray());
		var groupedIdxesXYBox = idxesCheckXYBox.Select(a => checkOffsetsXYBox.Select(b => a + b).ToArray());

		//Debug.Log(groupedIdxesX.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesY.Select(a => a.Join(",")).Join());
		//Debug.Log(groupedIdxesXYBox.Select(a => a.Join(",")).Join());


		var newCombinationsLeft = combinationsCollapse.Select(a => a.ToList()).ToList();
		// Simple elimation. Basically find cells that have only 1 entry and candidates from other cells.
		var singleFilledCellIdxes = Enumerable.Range(0, 16).Where(a => combinationsCollapse.ElementAt(a).Count() == 1);
		foreach (var x in singleFilledCellIdxes)
		{
			var currentPossibilities = combinationsCollapse.ElementAt(x); // The current combination set for that index.

			var CurXGroup = groupedIdxesX.Single(a => a.Contains(x));
			var CurYGroup = groupedIdxesY.Single(a => a.Contains(x));
			var CurXYBox = groupedIdxesXYBox.Single(a => a.Contains(x));
			// Grouped Idxes excluding the current idx in the particular location.
			var RemainingXGroup = CurXGroup.Where(a => a != x);
			var RemainingYGroup = CurYGroup.Where(a => a != x);
			var RemainingXYBox = CurXYBox.Where(a => a != x);

			var mergedCombinationIdxes = RemainingXGroup.Union(RemainingYGroup).Union(RemainingXYBox);
			// A merged index of all the mentioned items.
			foreach (int idx in mergedCombinationIdxes)
				newCombinationsLeft[idx].Remove(currentPossibilities.Single());
		}
		var multiFilledCellsIdxes = Enumerable.Range(0, 16).Where(a => combinationsCollapse.ElementAt(a).Count() > 1); // An idx list of all cells that have more than 1 combination.
																													   // Last possible value within given region, known as Naked Singles.
		foreach (var x in multiFilledCellsIdxes)
		{
			var currentPossibilities = combinationsCollapse.ElementAt(x); // The current combination set for that index.

			var CurXGroup = groupedIdxesX.Single(a => a.Contains(x));
			var CurYGroup = groupedIdxesY.Single(a => a.Contains(x));
			var CurXYBox = groupedIdxesXYBox.Single(a => a.Contains(x));
			// Grouped Idxes excluding the current idx in the particular location.
			var RemainingXGroup = CurXGroup.Where(a => a != x);
			var RemainingYGroup = CurYGroup.Where(a => a != x);
			var RemainingXYBox = CurXYBox.Where(a => a != x);
			foreach (var value in currentPossibilities)
			{
				if (!RemainingXGroup.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingYGroup.Any(a => combinationsCollapse.ElementAt(a).Contains(value)) ||
					!RemainingXYBox.Any(a => combinationsCollapse.ElementAt(a).Contains(value))) // Basically, if there is no other combinations left within any of these regions for that value...
				{
					newCombinationsLeft[x].RemoveAll(a => a != value); // Remove other possibilities of this value...
					break;
				}
			}
		}

		// Naked Pairs.
		var allGroups = groupedIdxesX.Concat(groupedIdxesY).Concat(groupedIdxesXYBox);
		foreach (var grouping in allGroups)
		{
			for (var idx1 = 0; idx1 < 3; idx1++)
			{
				var possibleDigits1 = combinationsCollapse.ElementAt(grouping[idx1]);
				for (var idx2 = idx1 + 1; idx2 < 4; idx2++)
				{
					var exemptIdxValues = Enumerable.Range(0, 4).Where(a => idx1 != a && a != idx2).ToArray();
					var possibleDigits2 = combinationsCollapse.ElementAt(grouping[idx2]);
					var unionedSets = possibleDigits1.Union(possibleDigits2);
					// There are a couple ways to approach this, one way is to check if the union of the 2 sets have 2 numbers, and the sets have 2 possibilities.
					// Another way is to check if those 2 cells have exactly those combinations left.
					if (possibleDigits1.OrderBy(a => a).SequenceEqual(possibleDigits2.OrderBy(b => b)) && possibleDigits1.Count == 2 && possibleDigits2.Count == 2)
					{
						// If it does... Remove the other possibilities from the other tiles within that group.
						foreach (var idxVoid in exemptIdxValues)
							newCombinationsLeft[grouping[idxVoid]].RemoveAll(a => unionedSets.Contains(a));
					}

				}
			}
		}

		return Enumerable.Range(0, 16).All(a => newCombinationsLeft[a].SequenceEqual(combinationsCollapse.ElementAt(a))) ? combinationsCollapse : CollapseGrid(newCombinationsLeft);
		// If all 64 cells have the same possibility after applying the combinations, stop here.
	}
	public override void GenerateBoard()
    {
		var possibleDigitsPrecollapse = new List<int>[16];
		finalBoard = new int[16];
		currentBoard = new int[16];
		lockInputs = new bool[16];

		var idxInputs = new List<int>();
		var digitsPlaced = new List<int>();
		var prioritizedIdxesCollapse = Enumerable.Range(0, 16).ToList();
		var retryCount = 0;
	fullRetryGen:
		idxInputs.Clear();
		digitsPlaced.Clear();
		for (var x = 0; x < possibleDigitsPrecollapse.Length; x++) // Start by filling every single option available on the module.
		{
			if (possibleDigitsPrecollapse[x] == null)
				possibleDigitsPrecollapse[x] = new List<int>();
			possibleDigitsPrecollapse[x].Clear();
			possibleDigitsPrecollapse[x].AddRange(Enumerable.Range(1, 4));
		}
		var iterCount = 0;
		prioritizedIdxesCollapse.Shuffle();
		var possibleDigitsPostCollapse = possibleDigitsPrecollapse.Select(a => a.ToList()).ToArray();
	iterate:
		iterCount++;

		var filteredIdxesToCollapse = prioritizedIdxesCollapse.Where(a => possibleDigitsPostCollapse[a].Count > 1);
		//scan:
		if (!filteredIdxesToCollapse.Any())
		{
			retryCount++;
			goto fullRetryGen;
		}
		else
		{
			var pickedIdx = filteredIdxesToCollapse.First();
			var pickedDigit = possibleDigitsPostCollapse[pickedIdx].PickRandom();

			idxInputs.Add(pickedIdx);
			digitsPlaced.Add(pickedDigit);

			possibleDigitsPostCollapse[pickedIdx].RemoveAll(a => a != pickedDigit);
		}
		possibleDigitsPostCollapse = CollapseGrid(possibleDigitsPostCollapse).ToArray();




		if (possibleDigitsPostCollapse.Any(a => !a.Any()))
		{
			retryCount++;
			goto fullRetryGen;
		}
		// If there are no other options for one of the cells, restart the entire generation.
		else if (!possibleDigitsPostCollapse.All(a => a.Count() == 1) && iterCount < 128)
			goto iterate;
		// If there isn't 1 option left, keep selecting until one of two things happen: An unsolvable board is created, or all of the cells have 1 possibility.
		finalBoard = possibleDigitsPostCollapse.Select(a => a.Single()).ToArray();
		for (var x = 0; x < idxInputs.Count; x++)
		{
			lockInputs[idxInputs[x]] = true;
			currentBoard[idxInputs[x]] = digitsPlaced[x];
		}
	}
    public override void DisplayCurrentBoard()
    {
        base.DisplayCurrentBoard();
		for (var x = 0; x < usedTextMeshes.Length; x++)
		{
			usedTextMeshes[x].text = currentBoard[x] == 0 ? "" : currentBoard[x].ToString();
			usedTextMeshes[x].color = lockInputs[x] ? Color.white : Color.gray;
		}
    }
    public override void HandleIdxPress(int idx)
    {
		if (idx < 0 || idx >= 16 || lockInputs[idx]) return;
		currentBoard[idx] = (currentBoard[idx] + 1) % 5;
	}

    public override void CheckCurrentBoard()
    {
		puzzleSolved = currentBoard.SequenceEqual(finalBoard);
    }

    public override IEnumerable<int> GetCurrentBoard()
    {
		return currentBoard;
    }
    public override IEnumerable<int> GetSolutionBoard()
    {
		return finalBoard;
    }

}
