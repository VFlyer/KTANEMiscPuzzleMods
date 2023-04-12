using System.Collections.Generic;
using UnityEngine;

public class PuzzleGeneric : MonoBehaviour {

    public MeshRenderer[] usedRenderers;
    protected bool puzzleSolved;

	public virtual bool IsPuzzleSolved()
    {
		return puzzleSolved;
    }
	public virtual void GenerateBoard()
    {

    }
	public virtual void DisplayCurrentBoard()
    {
        foreach (var renderer in usedRenderers)
            renderer.enabled = true;
    }
	public virtual void HideCurrentBoard()
    {
        foreach (var renderer in usedRenderers)
            renderer.enabled = false;
    }

    public virtual void ShuffleCurrentBoard()
    {

    }
    public virtual bool CheckCurrentBoard()
    {
        //puzzleSolved = true;
        return puzzleSolved;
    }

	public virtual void HandleIdxPress(int idx)
    {

    }

    public virtual void MimicLogBoard(string formatString, bool logSolutionBoard = false)
    {
        //Debug.LogFormat(formatString, logSolutionBoard ? GetSolutionBoard() : GetCurrentBoard());
    }

    public virtual IEnumerable<int> GetCurrentBoard()
    {
        return new int[0];
    }
    public virtual IEnumerable<int> GetSolutionBoard()
    {
        return new int[0];
    }
}
