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

	static int modIDCnt;
	int moduleID;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog, args));
    }

	public char[] GetLetterRefs()
    {
		return lettersAssignedAsNumbers;
    }

	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		lettersAssignedAsNumbers = new char[9];
		var serialNumberDigits = bombInfo.GetSerialNumberNumbers();
		lettersAssignedAsNumbers[0] = alphabet[serialNumberDigits.Sum() % 26];
		var lettersInSerialNumber = bombInfo.GetSerialNumberLetters().Distinct();
		for (var x = 0; x < lettersInSerialNumber.Count(); x++)
        {
			var idxLetterCur = alphabet.IndexOf(lettersInSerialNumber.ElementAt(x));
			while (lettersAssignedAsNumbers.Take(x + 1).Contains(alphabet[idxLetterCur]))
				idxLetterCur = (idxLetterCur + 1) % 26;
			lettersAssignedAsNumbers[x + 1] = alphabet[idxLetterCur];
        }
		var curIdxEmpty = Enumerable.Range(0, 9).First(a => !char.IsLetter(lettersAssignedAsNumbers[a]));
		var offsetFrom1stSNDigit = serialNumberDigits.First() == 0 ? 10 : serialNumberDigits.First();
		var lastLetter = lettersAssignedAsNumbers[0];
		while (curIdxEmpty < 9)
        {
			var curIdxAfterOffset = (alphabet.IndexOf(lastLetter) + offsetFrom1stSNDigit) % 26;
			while (lettersAssignedAsNumbers.Take(curIdxEmpty + 1).Contains(alphabet[curIdxAfterOffset]))
				curIdxAfterOffset = (curIdxAfterOffset + 1) % 26;
			lettersAssignedAsNumbers[curIdxEmpty] = alphabet[curIdxAfterOffset];
			lastLetter = alphabet[curIdxAfterOffset];
			curIdxEmpty++;
		}
		QuickLog("The following letters are assigned to these numbers: {0}", Enumerable.Range(0, 9).Select(a => string.Format("[{0}: {1}]", a + 1, lettersAssignedAsNumbers[a])).Join(", "));
	}

	// Update is called once per frame
	void Update() {

	}
}
