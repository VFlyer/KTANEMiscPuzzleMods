using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeceptiveKeenScript : KeenScript {

	public UniversalKeenTileModule[] individualKeenTileModules;
	int idxShown = -1;
	// Use this for initialization
	protected override void Start () {
		uniqueGrid = new UniqueGridGenerator();
		var displayedValues = uniqueGrid.GetGrid();
		for (var x = 0; x < gridText.Length; x++)
        {
			var rowIdx = x % 6;
			var colIdx = x / 6;
			gridText[x].text = displayedValues[rowIdx, colIdx].ToString();
        }
        for (var x = 0; x < individualKeenTileModules.Length; x++)
        {
			int y = x;
			individualKeenTileModules[x].modSelfSelectable.OnFocus += delegate {
				idxShown = y;
			};
			individualKeenTileModules[x].modSelfSelectable.OnDefocus += delegate {
				idxShown = -1;
			};
		}
	}

	// Update is called once per frame
	void Update () {
		for (var x = 0; x < individualKeenTileModules.Length; x++)
        {
			individualKeenTileModules[x].transform.localScale = Vector3.one * (x == idxShown ? 1 : 0.5f);
        }
	}
}
