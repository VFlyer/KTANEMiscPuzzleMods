using System.Collections.Generic;
using System.Linq;

public class UniqueGridGenerator{

	private int[,] storedGrid;

	// Use this for initialization
	public UniqueGridGenerator(int width = 6) {
		storedGrid = new int[width, width];
		var maxIterations = 1000000;
		var curIterationCount = 0;
        for (var x = 0; x < storedGrid.GetLength(1) - 1 && maxIterations > curIterationCount; x++)
		{
			var firstListValues = Enumerable.Range(1, 6).ToArray().Shuffle();
			for (var y = 0; y < firstListValues.Length; y++)
			{
				storedGrid[x, y] = firstListValues[y];
			}
			var isUnique = true;
			for (var y = 0; y < storedGrid.GetLength(0); y++)
            {
				for (var r = x - 1; r >= 0; r--)
				{
					isUnique &= storedGrid[x, y] != storedGrid[r, y];
				}
			}
			if (!isUnique)
			{
				x--;
			}
			curIterationCount++;
		}
		// Log the resulting board.
		
		if (curIterationCount < maxIterations)
			for (var x = 0; x < storedGrid.GetLength(0); x++)
			{
				var curValues = new List<int>();
				for (var r = 0; r < storedGrid.GetLength(0) - 1; r++)
				{
					curValues.Add(storedGrid[r, x]);
				}
				storedGrid[storedGrid.GetLength(0) - 1, x] = Enumerable.Range(1, 6).Except(curValues).Single();
			}
		
	}
	public int[,] GetGrid()
    {
		return storedGrid;
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