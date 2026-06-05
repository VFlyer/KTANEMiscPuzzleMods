using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class DoubtLieBtnPuzzle {
    public string[] allClues;
    public sbyte[] colorIdx;
    public bool[] truthTeller, safeButton;
    public int[] storedPossibleAmounts;
    static readonly string[] clueArgs = new[] {
        "BT", "B0", "B1", "B2", // Amount of buttons, total, or by color
        "CL", "CM", "CR", // Amount in a given column
        "RT", "RM", "RB", // Amount in a given row
        "TL", "TM", "TR", "ML", "MM", "MR", "BL", "BM", "BR", // Position dependent
    },
        comparArgs = new[] {
     "EQ", "LT", "GT", "LE", "GE"},
        amntArgs = new[] {
     "N0", "N1", "N2", "N3", "N4"};
    public DoubtLieBtnPuzzle(int[] possibleAmounts, int buttonCount = 9)
    {
        allClues = new string[buttonCount];
        truthTeller = new bool[buttonCount];
        safeButton = new bool[buttonCount];
        colorIdx = new sbyte[buttonCount];
        storedPossibleAmounts = possibleAmounts.ToArray();
    }
    public bool ClueCorrect(string encryptedClue)
    {
        // {Clue} {Comparasion} {Amount/Clue}
        if (!Regex.IsMatch(encryptedClue, string.Format(@"^{0} {1} {2}$",
            clueArgs.Join("|"),comparArgs.Join("|"),clueArgs.Concat(amntArgs).Join("|"))))
            { return false; }
        var splitParts = encryptedClue.Split();
        var amountsCompare = new int[2];
        for (var x = 0; x < splitParts.Length; x++)
        {// First step: Get values.
            switch (splitParts[2 * x])
            {
                case "BT": amountsCompare[x] = safeButton.Count(a => !a); break;
                case "B0": amountsCompare[x] = Enumerable.Range(0, colorIdx.Length).Where(a => colorIdx[a] == 0).Count(a => !safeButton[a]); break;
                case "B1": amountsCompare[x] = Enumerable.Range(0, colorIdx.Length).Where(a => colorIdx[a] == 1).Count(a => !safeButton[a]); break;
                case "B2": amountsCompare[x] = Enumerable.Range(0, colorIdx.Length).Where(a => colorIdx[a] == 2).Count(a => !safeButton[a]); break;
                case "CL": amountsCompare[x] = Enumerable.Range(0, 3).Count(a => !safeButton[3 * a]); break;
                case "CM": amountsCompare[x] = Enumerable.Range(0, 3).Count(a => !safeButton[3 * a + 1]); break;
                case "CR": amountsCompare[x] = Enumerable.Range(0, 3).Count(a => !safeButton[3 * a + 2]); break;
                case "RT": amountsCompare[x] = safeButton.Take(3).Count(a => !a); break;
                case "RM": amountsCompare[x] = safeButton.Skip(3).Take(3).Count(a => !a); break;
                case "RB": amountsCompare[x] = safeButton.TakeLast(3).Count(a => !a); break;
                case "TL": amountsCompare[x] = safeButton[0] ? 1 : 0; break;
                case "TM": amountsCompare[x] = safeButton[1] ? 1 : 0; break;
                case "TR": amountsCompare[x] = safeButton[2] ? 1 : 0; break;
                case "ML": amountsCompare[x] = safeButton[3] ? 1 : 0; break;
                case "MM": amountsCompare[x] = safeButton[4] ? 1 : 0; break;
                case "MR": amountsCompare[x] = safeButton[5] ? 1 : 0; break;
                case "BL": amountsCompare[x] = safeButton[6] ? 1 : 0; break;
                case "BM": amountsCompare[x] = safeButton[7] ? 1 : 0; break;
                case "BR": amountsCompare[x] = safeButton[8] ? 1 : 0; break;
            }
        }
        return true;
    }

}
