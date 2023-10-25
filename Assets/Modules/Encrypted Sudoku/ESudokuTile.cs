using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESudokuTile : MonoBehaviour {


    protected bool hPanelRequired = false;
    protected bool vPanelRequired = false;

    public bool requireHorizontalPanel { get { return hPanelRequired; } }
    public bool requireVerticalPanel { get { return vPanelRequired; } }

    [SerializeField]
    protected GameObject hPanelContents, vPanelContents;
    protected Renderer centerTileRenderer;

    EncryptedSudokuScript baseScript;
	public void AssignCore(EncryptedSudokuScript mainHandler)
    {
        baseScript = mainHandler;
        SetupCell();
    }
    protected virtual void SetupCell()
    {

    }
    public virtual string GetTPCellHelpInfo()
    {
        return "";
    }
}
