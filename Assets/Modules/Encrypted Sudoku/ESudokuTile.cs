using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESudokuTile : MonoBehaviour {

    public virtual bool requireHorizontalPanel { get { return false; } }
    public virtual bool requireVerticalPanel { get { return false; } }

    [SerializeField]
    protected GameObject hPanelContents, vPanelContents;
    public Renderer centerTileRenderer;

    private bool revealed;

    EncryptedSudokuScript baseScript;
	public void AssignCore(EncryptedSudokuScript mainHandler)
    {
        baseScript = mainHandler;
        SetupCell();
    }
    protected virtual void SetupCell()
    {
        var lettersAssigned = baseScript.GetLetterRefs();
    }
    public virtual IEnumerator HandleCellActivate()
    {
        yield break;
    }
    public void SetRevealState(bool newState)
    {
        revealed = newState;
    }

    public virtual string GetTPCellHelpInfo()
    {
        return "";
    }
}
