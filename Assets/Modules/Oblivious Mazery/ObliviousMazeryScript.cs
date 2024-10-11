using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObliviousMazeryScript : MonoBehaviour {

	public MeshRenderer affectedRenderer, colorChangingRender;
	Texture2D usedTexture;
	Color[] allowedColors = new[] { Color.black, Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.gray, Color.clear };

	int imgWidth = 41, imgHeight = 41;

	enum ClueTypes {
		Empty,
		Any,
		PassU,
		PassR,
		PassL,
		PassD,
		WallU,
		WallR,
		WallL,
		WallD,
		WallCnt0,
		WallCnt1,
		WallCnt2,
		WallCnt3,
		WallComboT, // 3-way Passageways
		WallComboHV, // Horizontal/Vertical Passageways
		WallComboB, // Bending Passageways
		WallComboE, // Dead Ends
		WallComboEHV, // Dead Ends / Horizontal/Vertical Passageways
		WallComboBE, // Dead Ends / Bending Passageways
		WallComboTHV, // 3-way / Horizontal/Vertical Passageways
		WallComboTB, // 3-way / Bending Passageways
	}
	// Used to render clues on this module. Process these in reading order.
    Dictionary<ClueTypes, string[]> clueDisplays = new Dictionary<ClueTypes, string[]> {
        { ClueTypes.Empty, new[] { "-------;-------;-------;-------;-------;-------;-------" } },
        { ClueTypes.Any, new[] { "-+++++-;-+---+-;-----+-;---+++-;---+---;-------;---+---" } },
        { ClueTypes.PassU, new[] { "---+---;--+-+--;-------;-------;-------;-------;-------" } },
        { ClueTypes.PassR, new[] { "-------;-------;-----+-;------+;-----+-;-------;-------" } },
        { ClueTypes.PassD, new[] { "-------;-------;-------;-------;-------;--+-+--;---+---" } },
        { ClueTypes.PassL, new[] { "-------;-------;-+-----;+------;-+-----;-------;-------" } },
        { ClueTypes.WallU, new[] { "---+---;--+++--;-------;-------;-------;-------;-------" } },
        { ClueTypes.WallR, new[] { "-------;-------;-----+-;-----++;-----+-;-------;-------" } },
        { ClueTypes.WallD, new[] { "-------;-------;-------;-------;-------;--+++--;---+---" } },
        { ClueTypes.WallL, new[] { "-------;-------;-+-----;++-----;-+-----;-------;-------" } },
        { ClueTypes.WallCnt0, new[] { "--+++--;-+---+-;-+--++-;-+-+-+-;-++--+-;-+---+-;--+++--" } },
        { ClueTypes.WallCnt1, new[] { "---+---;--++---;-+-+---;---+---;---+---;---+---;-+++++-" } },
        { ClueTypes.WallCnt2, new[] { "--+++--;-+---+-;-----+-;----+--;---+---;--+----;-+++++-" } },
        { ClueTypes.WallCnt3, new[] { "--+++--;-+---+-;-----+-;--+++--;-----+-;-+---+-;--+++--" } },
        { ClueTypes.WallComboT, new[] { "-------;-------;-------;-------;-------;-------;+++++++", "+++++++;-------;-------;-------;-------;-------;-------", "+------;+------;+------;+------;+------;+------;+------", "------+;------+;------+;------+;------+;------+;------+" } },
        { ClueTypes.WallComboHV, new[] { "+++++++;-------;-------;-------;-------;-------;+++++++", "+-----+;+-----+;+-----+;+-----+;+-----+;+-----+;+-----+" } },

    };
	// Use 1 to represent a wall, 0 to represent a passageway. 4 digit binary, LDRU, from most to least significant.
	Dictionary<ClueTypes, int[]> clueWallsIdxAllow = new Dictionary<ClueTypes, int[]> {
		{ ClueTypes.Any, Enumerable.Range(0, 15).ToArray() },
		{ ClueTypes.PassU, Enumerable.Range(0, 15).Where(a => (a & 1) != 1).ToArray() },
		{ ClueTypes.PassR, Enumerable.Range(0, 15).Where(a => (a & 2) != 2).ToArray() },
		{ ClueTypes.PassD, Enumerable.Range(0, 15).Where(a => (a & 4) != 4).ToArray() },
		{ ClueTypes.PassL, Enumerable.Range(0, 15).Where(a => (a & 8) != 8).ToArray() },
		{ ClueTypes.WallU, Enumerable.Range(0, 15).Where(a => (a & 1) == 1).ToArray() },
		{ ClueTypes.WallR, Enumerable.Range(0, 15).Where(a => (a & 2) == 2).ToArray() },
		{ ClueTypes.WallD, Enumerable.Range(0, 15).Where(a => (a & 4) == 4).ToArray() },
		{ ClueTypes.WallL, Enumerable.Range(0, 15).Where(a => (a & 8) == 8).ToArray() },
		{ ClueTypes.WallCnt0, new[] { 0 } },
		{ ClueTypes.WallCnt1, new[] { 1, 2, 4, 8 } },
		{ ClueTypes.WallCnt2, new[] { 3, 5, 6, 9, 10, 12 } },
		{ ClueTypes.WallCnt3, new[] { 7, 11, 13, 14 } },
        { ClueTypes.WallComboT, new[] { 1, 2, 4, 8 } },
	};


	// Use this for initialization
	void Start () {
		usedTexture = new Texture2D(imgWidth, imgHeight, TextureFormat.RGBA32, true, true);
		usedTexture.alphaIsTransparency = true;
		usedTexture.wrapMode = TextureWrapMode.Clamp;
		usedTexture.filterMode = FilterMode.Point;
		affectedRenderer.material.color = Color.white;
		affectedRenderer.material.mainTexture = usedTexture;
		usedTexture.SetPixels(Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray());

		StartCoroutine(TestTextureModification());
		StartCoroutine(CycleColors());
	}
	IEnumerator CycleColors()
    {
		while (enabled)
		{
			var lastColor = colorChangingRender.material.color;
			var nextColor = allowedColors.PickRandom();
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
				colorChangingRender.material.color = Color.Lerp(lastColor, nextColor, t);
				yield return null;
            }
			colorChangingRender.material.color = nextColor;
		}
	}
	IEnumerator TestTextureModification()
    {
		for (var x = 0; x < usedTexture.width; x++)
		{
			for (var y = 0; y < usedTexture.height; y++)
			{
				usedTexture.SetPixel(x, y, allowedColors.PickRandom());
				usedTexture.Apply();
				yield return new WaitForSeconds(0.02f);
			}
		}
    }


	// Update is called once per frame
	void Update () {
		
	}
}
