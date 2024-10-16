using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OblivMazeEnums;

public class ObliviousMazeryScript : MonoBehaviour
{
    public KMBombModule modSelf;
    public KMSelectable[] wallsSelectable;
    public KMAudio mAudio;
    public MeshRenderer affectedRenderer, colorChangingRender;
    Texture2D usedTexture;
    Color[] allowedColors = new[] { Color.clear, Color.black, Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.gray};
    [SerializeField]
    Color uncheckedColor = Color.white, incorrectColor = Color.red, correctColor = Color.green, hlColor = Color.yellow;

    const int mazeImgWidth = 41, mazeImgHeight = 41;
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
        { ClueTypes.WallComboB, new[] { "+++++++;+------;+------;+------;+------;+------;+------", "+------;+------;+------;+------;+------;+------;+++++++", "------+;------+;------+;------+;------+;------+;+++++++", "+++++++;------+;------+;------+;------+;------+;------+" } },
        { ClueTypes.WallComboE, new[] { "+++++++;+------;+------;+------;+------;+------;+++++++", "+++++++;+-----+;+-----+;+-----+;+-----+;+-----+;+-----+", "+++++++;------+;------+;------+;------+;------+;+++++++", "+-----+;+-----+;+-----+;+-----+;+-----+;+-----+;+++++++" } },
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
        { ClueTypes.WallComboHV, new[] { 5, 10 } },
        { ClueTypes.WallComboB, new[] { 3, 6, 9, 12 } },
        { ClueTypes.WallComboE, new[] { 7, 11, 13, 14 } },
    };

    OblivMazePuzzle usedPuzzle;
    bool moduleSolved, interactable = false;
    [SerializeField]
    bool debugPuzzle = true;

    int moduleID;
    static int modIDCnt;

    int[] curWallTileIdxes = new int[16];

    void QuickLog(string toLog, params object[] args)
    {
        Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName , moduleID, string.Format(toLog, args));
    }
    // Use this for initialization
    void Start()
    {
        moduleID = ++modIDCnt;

        usedTexture = new Texture2D(mazeImgWidth, mazeImgHeight, TextureFormat.RGBA32, true, true);
        usedTexture.alphaIsTransparency = true;
        usedTexture.wrapMode = TextureWrapMode.Clamp;
        usedTexture.filterMode = FilterMode.Point;
        affectedRenderer.material.color = Color.white;
        affectedRenderer.material.mainTexture = usedTexture;
        usedTexture.SetPixels(Enumerable.Repeat(Color.clear, mazeImgWidth * mazeImgHeight).ToArray());

        usedPuzzle = new OblivMazePuzzle();
        if (debugPuzzle)
        {
            usedPuzzle.cluesUsed = new[] {
                new List<ClueTypes> { ClueTypes.WallComboHV },
                new List<ClueTypes> { ClueTypes.WallComboB },
                new List<ClueTypes> { ClueTypes.WallComboE },
                new List<ClueTypes> { ClueTypes.WallComboT },

                new List<ClueTypes> { ClueTypes.WallComboHV },
                new List<ClueTypes> { ClueTypes.WallComboHV },
                new List<ClueTypes> { ClueTypes.WallComboT },
                new List<ClueTypes> { ClueTypes.WallComboB },

                new List<ClueTypes> { ClueTypes.WallComboHV },
                new List<ClueTypes> { ClueTypes.WallComboE },
                new List<ClueTypes> { ClueTypes.WallComboT },
                new List<ClueTypes> { ClueTypes.WallComboHV },

                new List<ClueTypes> { ClueTypes.WallComboE },
                new List<ClueTypes> { ClueTypes.WallComboB },
                new List<ClueTypes> { ClueTypes.WallComboE },
                new List<ClueTypes> { ClueTypes.WallComboE },
            };
            usedPuzzle.solutionWallIdxes = new[] {
                5, 6, 11, 8,
                5, 5, 2, 12,
                5, 7, 8, 5,
                13, 3, 14, 11
            };
            usedPuzzle.clueTypeArgs = new[] {
                "0","1","3","0",
                "0","0","0","1",
                "0","3","0","0",
                "3","1","3","3",
            };
        }
        for (var x = 0; x < wallsSelectable.Length; x++)
        {
            var y = x;
            wallsSelectable[x].OnInteract += delegate {
                if (interactable)
                    HandleWallPress(y);
                return false;
            };
            wallsSelectable[x].OnHighlight += delegate
            {
                if (interactable)
                    HandleWallHL(y);
            };
            wallsSelectable[x].OnHighlightEnded += delegate
            {
                if (interactable)
                    HandleWallHLEnd(y);
            };

        }


        StartCoroutine(HandleRevealPuzzle(usedPuzzle));
        //StartCoroutine(TestTextureModification());
        //StartCoroutine(CycleColors());
    }
    void HandleWallHL(int idxBtnWall)
    {
        var verticalWallIdxes = Enumerable.Range(0, 40).Where(a => a % 9 > 3);
        var horizontalWallIdxes = Enumerable.Range(0, 40).Where(a => a % 9 <= 3);
        if (verticalWallIdxes.Contains(idxBtnWall))
        {
            var idxVerticalWall = verticalWallIdxes.IndexOf(a => a == idxBtnWall);
            var pixelStartRow = 10 * (idxVerticalWall / 5) + 1;
            var pixelStartCol = 10 * (4 - idxVerticalWall % 5);
            for (var d = 0; d < 9; d++)
            {
                if (usedTexture.GetPixel(pixelStartCol, pixelStartRow + d) != Color.clear)
                    usedTexture.SetPixel(pixelStartCol, pixelStartRow + d, hlColor);
            }
        }
        else
        {
            var idxHorizontalWall = horizontalWallIdxes.IndexOf(a => a == idxBtnWall);
            var pixelStartRow = 10 * (idxHorizontalWall / 4);
            var pixelStartCol = 10 * (3 - idxHorizontalWall % 4) + 1;
            for (var d = 0; d < 9; d++)
            {
                if (usedTexture.GetPixel(pixelStartCol + d, pixelStartRow) != Color.clear)
                    usedTexture.SetPixel(pixelStartCol + d, pixelStartRow, hlColor);
            }
        }
        usedTexture.Apply();
    }
    void HandleWallHLEnd(int idxBtnWall)
    {
        var verticalWallIdxes = Enumerable.Range(0, 40).Where(a => a % 9 > 3);
        var horizontalWallIdxes = Enumerable.Range(0, 40).Where(a => a % 9 <= 3);
        if (verticalWallIdxes.Contains(idxBtnWall))
        {
            var idxVerticalWall = verticalWallIdxes.IndexOf(a => a == idxBtnWall);
            var pixelStartRow = 10 * (idxVerticalWall / 5) + 1;
            var pixelStartCol = 10 * (4 - idxVerticalWall % 5);
            for (var d = 0; d < 9; d++)
            {
                if (usedTexture.GetPixel(pixelStartCol, pixelStartRow + d) != Color.clear)
                    usedTexture.SetPixel(pixelStartCol, pixelStartRow + d, uncheckedColor);
            }
        }
        else
        {
            var idxHorizontalWall = horizontalWallIdxes.IndexOf(a => a == idxBtnWall);
            var pixelStartRow = 10 * (idxHorizontalWall / 4);
            var pixelStartCol = 10 * (3 - idxHorizontalWall % 4) + 1;
            for (var d = 0; d < 9; d++)
            {
                if (usedTexture.GetPixel(pixelStartCol + d, pixelStartRow) != Color.clear)
                    usedTexture.SetPixel(pixelStartCol + d, pixelStartRow, uncheckedColor);
            }
        }
        usedTexture.Apply();
    }
    void UpdatePuzzle(OblivMazePuzzle puzzleToRender = null)
    {
        for (var x = 0; x < curWallTileIdxes.Length; x++)
        {
            var cellIdx = Enumerable.Range(0, 4).Select(a => (curWallTileIdxes[x] >> a & 1) == 1).ToArray();

            var rowIdx = x / 4;
            var colIdx = x % 4;
            // Note to self, there is a better way to do this.
            var wallTopPxlColStart = 10 * (3 - colIdx) + 1;
            var wallTopPxlRowStart = 10 * rowIdx;
            for (var d = 0; d < 9; d++)
                usedTexture.SetPixel(wallTopPxlColStart + d, wallTopPxlRowStart, d % 2 == 1 || cellIdx[0] ? uncheckedColor : Color.clear);
            var wallBtmPxlColStart = 10 * (3 - colIdx) + 1;
            var wallBtmPxlRowStart = 10 * (rowIdx + 1);
            for (var d = 0; d < 9; d++)
                usedTexture.SetPixel(wallBtmPxlColStart + d, wallBtmPxlRowStart, d % 2 == 1 || cellIdx[2] ? uncheckedColor : Color.clear);
            var wallRPxlColStart = 10 * (3 - colIdx);
            var wallRPxlRowStart = 10 * rowIdx + 1;
            for (var d = 0; d < 9; d++)
                usedTexture.SetPixel(wallRPxlColStart, wallRPxlRowStart + d, d % 2 == 1 || cellIdx[1] ? uncheckedColor : Color.clear);
            var wallLPxlColStart = 10 * (4 - colIdx);
            var wallLPxlRowStart = 10 * rowIdx + 1;
            for (var d = 0; d < 9; d++)
                usedTexture.SetPixel(wallLPxlColStart, wallLPxlRowStart + d, d % 2 == 1 || cellIdx[3] ? uncheckedColor : Color.clear);
            // End note.
        }
        if (puzzleToRender != null)
        for (var clueIdx = 0; clueIdx < puzzleToRender.cluesUsed.Length; clueIdx++)
        {
            var argsUsed = puzzleToRender.clueTypeArgs.ElementAtOrDefault(clueIdx);
            var clueColStartIdxFill = 2 + 10 * (clueIdx % 4);
            var clueRowStartIdxFill = 2 + 10 * (clueIdx / 4);
            foreach (var clue in puzzleToRender.cluesUsed[clueIdx])
            {
                var usedClueGroup = clueDisplays[clue].Select(a => a.Split(';').Select(b => b.Select(c => c == '+').ToArray()).ToArray()).ToArray();
                bool[][] pickedEncoding;
                switch (argsUsed)
                {
                    case null:
                    default: pickedEncoding = usedClueGroup.PickRandom(); break;
                    case "0": pickedEncoding = usedClueGroup[0]; break;
                    case "1": pickedEncoding = usedClueGroup[1]; break;
                    case "2": pickedEncoding = usedClueGroup[2]; break;
                    case "3": pickedEncoding = usedClueGroup[3]; break;
                }
                for (var r = 0; r < pickedEncoding.Length; r++)
                    for (var c = 0; c < pickedEncoding[r].Length; c++)
                        if (pickedEncoding[r][c])
                            usedTexture.SetPixel(mazeImgWidth - 1 - (c + clueColStartIdxFill), r + clueRowStartIdxFill, uncheckedColor);
            }
        }
        usedTexture.Apply();
    }

    void HandleWallPress(int idxBtnWall)
    {
        var selectableIdxWallsAffect = new int[][] {
            // Order of the walls should be URDL. Tile idxes in reading order.
            new[] { 0, 5, 9, 4 },
            new[] { 1, 6, 10, 5 },
            new[] { 2, 7, 11, 6 },
            new[] { 3, 8, 12, 7 },
            new[] { 9, 14, 18, 13 },
            new[] { 10, 15, 19, 14 },
            new[] { 11, 16, 20, 15 },
            new[] { 12, 17, 21, 16 },
            new[] { 18, 23, 27, 22 },
            new[] { 19, 24, 28, 23 },
            new[] { 20, 25, 29, 24 },
            new[] { 21, 26, 30, 25 },
            new[] { 27, 32, 36, 31 },
            new[] { 28, 33, 37, 32 },
            new[] { 29, 34, 38, 33 },
            new[] { 30, 35, 39, 34 },
        };
        for (int tileIdx = 0; tileIdx < selectableIdxWallsAffect.Length; tileIdx++)
        {
            var tileGroupIdx = selectableIdxWallsAffect[tileIdx].IndexOf(a => a == idxBtnWall);
            if (tileGroupIdx != -1)
                curWallTileIdxes[tileIdx] ^= 1 << tileGroupIdx;
        }
        UpdatePuzzle(usedPuzzle);
    }

    IEnumerator HandleRevealPuzzle(OblivMazePuzzle puzzleToRender = null)
    {
        var renderBits = new bool[mazeImgWidth * mazeImgHeight];
        for (var d = 0; d < mazeImgWidth + mazeImgHeight; d++)
        {
            for (var x = 0; x < mazeImgWidth; x++)
                for (var y = 0; y < mazeImgHeight; y++)
                    if (x + y <= d && ((x % 10 == 0 && y % 2 == 0) || (y % 10 == 0 && x % 2 == 0)))
                        usedTexture.SetPixel(mazeImgWidth - 1 - x, y, uncheckedColor);
            usedTexture.Apply();
            yield return null;
        }
        if (puzzleToRender == null) yield break;
        for (var clueIdx = 0; clueIdx < puzzleToRender.cluesUsed.Length; clueIdx++)
        {
            var argsUsed = puzzleToRender.clueTypeArgs.ElementAtOrDefault(clueIdx);
            var clueColStartIdxFill = 2 + 10 * (clueIdx % 4);
            var clueRowStartIdxFill = 2 + 10 * (clueIdx / 4);
            foreach (var clue in puzzleToRender.cluesUsed[clueIdx])
            {
                var usedClueGroup = clueDisplays[clue].Select(a => a.Split(';').Select(b => b.Select(c => c == '+').ToArray()).ToArray()).ToArray();
                bool[][] pickedEncoding;
                switch (argsUsed)
                {
                    case null:
                    default: pickedEncoding = usedClueGroup.PickRandom(); break;
                    case "0": pickedEncoding = usedClueGroup[0]; break;
                    case "1": pickedEncoding = usedClueGroup[1]; break;
                    case "2": pickedEncoding = usedClueGroup[2]; break;
                    case "3": pickedEncoding = usedClueGroup[3]; break;
                }
                for (var r = 0; r < pickedEncoding.Length; r++)
                    for (var c = 0; c < pickedEncoding[r].Length; c++)
                        if (pickedEncoding[r][c])
                            usedTexture.SetPixel(mazeImgWidth - 1 - (c + clueColStartIdxFill), r + clueRowStartIdxFill, uncheckedColor);
            }
            usedTexture.Apply();
            yield return null;
        }
        interactable = true;
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

}


