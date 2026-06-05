using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBase : MonoBehaviour {
    public KMSelectable btnSelectable;
    public MeshRenderer btnRender, clueRender;
    public short btnColorIdx;
    public bool isSafe, truth;
    public string clueEncrypted;
}
