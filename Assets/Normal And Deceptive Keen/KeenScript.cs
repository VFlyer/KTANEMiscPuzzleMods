using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KeenScript : MonoBehaviour {
	public KMAudio mAudio;
	public KMSelectable[] squareSelectable;
	public KMSelectable selfSelectable;
	public TextMesh[] gridText;
	public MeshRenderer[] borderRenderers;
    protected UniqueGridGenerator uniqueGrid;

	protected List<List<int>> groupedPairIdxes;
	protected List<int> operatorIdx;

	// Use this for initialization
	protected virtual void Start () {
		uniqueGrid = new UniqueGridGenerator();
		var displayedValues = uniqueGrid.GetGrid();
		for (var x = 0; x < gridText.Length; x++)
		{
			var rowIdx = x % 6;
			var colIdx = x / 6;
			gridText[x].text = displayedValues[rowIdx, colIdx].ToString();
		}
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
		edgeIdxesAll.Shuffle();
		Debug.Log("[" + edgeIdxesAll.Select(a => a.Join(",")).Join("];[") + "]");
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
		Debug.Log("[" + groupedPairIdxes.Select(a => a.OrderBy(b => b).Join(",")).Join("];[") + "]");

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
                var isAdjacent = new[] {

                x % maxWidth < maxWidth - 1 && usableGroup.Contains(x + 1),
                x >= maxWidth && usableGroup.Contains(x - maxWidth),
                x < maxWidth * (maxWidth - 1) && usableGroup.Contains(x + maxWidth) && x % maxWidth >= 1 && usableGroup.Contains(x - 1),
                x < maxWidth * (maxWidth - 1) && usableGroup.Contains(x + maxWidth) && x % maxWidth < maxWidth - 1 && usableGroup.Contains(x + 1),

                x % maxWidth >= 1 && usableGroup.Contains(x - 1),
                x < maxWidth * (maxWidth - 1) && usableGroup.Contains(x + maxWidth),
                x >= maxWidth && usableGroup.Contains(x - maxWidth) && x % maxWidth < maxWidth - 1 && usableGroup.Contains(x + 1),
                x >= maxWidth && usableGroup.Contains(x - maxWidth) && x % maxWidth >= 1 && usableGroup.Contains(x - 1),
            };
                borderRenderers[x].material.mainTextureOffset = new Vector2(
                    Enumerable.Range(0, 2).Where(a => isAdjacent[a]).Select(a => Mathf.Pow(2, a)).Sum() / 16f,
                    (15 - Enumerable.Range(0, 2).Where(a => isAdjacent[a + 4]).Select(a => Mathf.Pow(2, a)).Sum()) / 16f);
            }
        }
        Debug.Log("[" + groupedPairIdxes.Select(a => a.OrderBy(b => b).Select(b => displayedValues[b / 6, b % 6]).Join(",")).Join("];[") + "]");
	}
}
