using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyersPuzzleSettings {
    public enum SubmissionType
    {
        NA,
        Slow,
        Fast,
        Instant
    }
    public bool FoaboruEnsureUniqueSolution = false;
    public bool FoaboruEnsureConnectivity = false;
    public SubmissionType FoaboruForceSubmissionType = SubmissionType.NA;
    public int FoaboruBoardLength = 8;
    public int FoaboruBoardWidth = 8;

}
