using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangoCoreScript : MonoBehaviour {

	public KMSelectable[] btnSelectables;
	public MeshRenderer[] clueRenders, btnRenders;

	int squareLength = 6;
	TangoPuzzle usedPuzzle;
	// Use this for initialization
	void Start () {
		usedPuzzle = new TangoPuzzle(squareLength / 2);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
