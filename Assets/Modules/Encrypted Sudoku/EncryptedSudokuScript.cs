using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;

public class EncryptedSudokuScript : MonoBehaviour {

	public KMBombModule modSelf;
	public KMBombInfo bombInfo;
	public ESudokuTile[] listedTiles;
	public KMSelectable[] directionSelectables;

	const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	char[] lettersAssignedAsNumbers;

	public char[] GetLetterRefs()
    {
		return lettersAssignedAsNumbers;
    }

	// Use this for initialization
	void Start () {
		lettersAssignedAsNumbers = new char[9];
		var serialNumberDigits = bombInfo.GetSerialNumberNumbers();
		var lettersInSerialNumber = bombInfo.GetSerialNumberLetters();
		lettersAssignedAsNumbers[0] = alphabet[serialNumberDigits.Sum() % 26];
        for (var x = 0; x < lettersAssignedAsNumbers.Length; x++)
        {
			var idxLetterCur = alphabet.IndexOf(lettersAssignedAsNumbers[x]);
			while (lettersAssignedAsNumbers.Take(x + 1).Contains(alphabet[idxLetterCur]))
				idxLetterCur = (idxLetterCur + 1) % 26;
			lettersAssignedAsNumbers[x + 1] = alphabet[idxLetterCur];
        }

	}

	// Update is called once per frame
	void Update () {

	}
}
