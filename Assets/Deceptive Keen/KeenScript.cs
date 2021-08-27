using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeenScript : MonoBehaviour {
	public KMAudio mAudio;
	public KMSelectable[] squareSelectable;
	public KMSelectable selfSelectable;
	public TextMesh[] gridText;
	public MeshRenderer[] borderRenderers;
	UniqueGridGenerator uniqueGrid;
	// Use this for initialization
	void Start () {
		uniqueGrid = new UniqueGridGenerator();
		var displayedValues = uniqueGrid.GetGrid();
		for (var x = 0; x < gridText.Length; x++)
		{
			var rowIdx = x % 6;
			var colIdx = x / 6;
			gridText[x].text = displayedValues[rowIdx, colIdx].ToString();
		}
	}
}
