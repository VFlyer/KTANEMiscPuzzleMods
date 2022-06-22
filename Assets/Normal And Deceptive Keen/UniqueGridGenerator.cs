using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniqueGridGenerator{

	private int[,] storedGrid;

	// Use this for initialization
	public UniqueGridGenerator(int width = 6) {
		storedGrid = new int[width, width];
		var firstListValues = Enumerable.Range(1, width);

		for (var x = 0; x < storedGrid.GetLength(1); x++)
		{
			var alteredArray = firstListValues.Skip(x).Concat(firstListValues.Take(x));
			for (var y = 0; y < storedGrid.GetLength(0); y++)
			{
				storedGrid[y, x] = alteredArray.ElementAt(y);
			}
		}

		var idxShuffles = new int[width - 1];
		for (var x = 0; x < idxShuffles.Length; x++)
			idxShuffles[x] = Random.Range(x + 1, width);
		for (var x = 0; x < storedGrid.GetLength(1); x++)
		{
			for (var y = 0; y < storedGrid.GetLength(0) - 1; y++)
			{
				var temVal = storedGrid[y, x];
				storedGrid[y, x] = storedGrid[idxShuffles[y], x];
				storedGrid[idxShuffles[y], x] = temVal;
			}
		}
		for (var x = 0; x < idxShuffles.Length; x++)
			idxShuffles[x] = Random.Range(x + 1, width);
		for (var x = 0; x < storedGrid.GetLength(1) - 1; x++)
		{
			for (var y = 0; y < storedGrid.GetLength(0); y++)
			{
				var temVal = storedGrid[y, x];
				storedGrid[y, x] = storedGrid[y, idxShuffles[x]];
				storedGrid[y, idxShuffles[x]] = temVal;
			}
		}

	}
	public int[,] GetGrid()
    {
		return storedGrid;
    }
	public IEnumerable<int> GetGridAs1DArray()
    {
		var output = new int[storedGrid.GetLength(0) * storedGrid.GetLength(1)];
        for (var x = 0; x < output.Length; x++)
        {
			output[x] = storedGrid[x % storedGrid.GetLength(0), x / storedGrid.GetLength(0)];
        }
		return output;
    }
	public int GetSize()
    {
		return storedGrid.GetLength(0) * storedGrid.GetLength(1);
	}

	public bool Equals(UniqueGridGenerator anotherGrid)
    {
		var gridToCheck = anotherGrid.GetGrid();
		if (gridToCheck.GetLength(0) != storedGrid.GetLength(0))
			return false;
		var isEqual = true;
		for (var y = 0; y < storedGrid.GetLength(0); y++)
		{
			for (var x = 0; x < storedGrid.GetLength(1); x++)
			{
				isEqual &= gridToCheck[y, x] != storedGrid[y, x];
			}
			
		}
		return isEqual;
    }
	public bool Equals(int[,] anotherGrid)
	{
		if (anotherGrid.GetLength(0) != storedGrid.GetLength(0) || anotherGrid.GetLength(1) != storedGrid.GetLength(1))
			return false;
		var isEqual = true;
		for (var y = 0; y < storedGrid.GetLength(0); y++)
		{
			for (var x = 0; x < storedGrid.GetLength(1); x++)
			{
				isEqual &= anotherGrid[y, x] != storedGrid[y, x];
			}

		}
		return isEqual;
	}
	public bool IsBoardDistinct(int[,] givenBoard, IEnumerable<int> ignoredValues = null)
	{
		var isCurBoardValid = true;
		for (var y = 0; y< givenBoard.GetLength(0); y++)
		{
			var curRowValues = new List<int>();
			for (var x = 0; x < givenBoard.GetLength(1); x++)
			{
				curRowValues.Add(givenBoard[x, y]);
			}
			if (ignoredValues != null)
				curRowValues.RemoveAll(a => ignoredValues.Contains(a));
			if (curRowValues.Distinct().Count() != curRowValues.Count())
			{
				isCurBoardValid = false;
			}
		}
		for (var y = 0; y < givenBoard.GetLength(0); y++)
		{
			var curRowValues = new List<int>();
			for (var x = 0; x < givenBoard.GetLength(1); x++)
            {
				curRowValues.Add(givenBoard[y, x]);
            }
			if (ignoredValues != null)
				curRowValues.RemoveAll(a => ignoredValues.Contains(a));
			if (curRowValues.Distinct().Count() != curRowValues.Count())
			{
				isCurBoardValid = false;
			}
		}
		return isCurBoardValid;
	}
}