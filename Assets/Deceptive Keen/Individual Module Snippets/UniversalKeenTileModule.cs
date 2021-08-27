using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalKeenTileModule : MonoBehaviour {
    public int storedValue = -1;
    public KMSelectable modSelfSelectable;
    public KMAudio mAudio;
    public KMBombInfo bombInfo;
    protected bool hasActivated = false;
    public virtual void ActivateTile()
    {
        hasActivated = true;
    }
}
