using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootoverSolveAnim : MonoBehaviour {

    public MeshRenderer[] affectedRenders;
    public TextMesh[] affectedMeshes;
    public KMAudio usedAudio;

	public IEnumerator StartSolveAnim()
    {
        usedAudio.PlaySoundAtTransform("Angel (Drop)", usedAudio.transform);
        yield break;
    }
}
