﻿using System.Collections.Generic;
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
    public virtual void CheckCurrentBoard()
    {
        puzzleSolved = true;
    }

	public virtual void HandleIdxPress(int idx)
    {

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