using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialAction
{
    None,
    SelectCard,
    SelectAttackCard,
    SelectBuffCard,
    SelectEnemyTarget,
    SelectPlayerTarget,
    UseHat,
    ToggleSpeed,
    CounsellorContinue,
    CounsellorSkip,
    ChooseCharacter,
    ChooseReward,
    ChooseMapPath,
    OpenBuffDetails,
    ChangeCharacter,
    Custom
}

public enum TutorialVisualMode
{
    None,
    KeepPrevious,
    HighlightOnly,
    MagicHandOnly,
    ArrowOnly,
    MagicHandAndHighlight,
    ArrowAndHighlight
}

public enum TutorialTargetPoint
{
    Center,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public enum TutorialArrowSide
{
    Above,
    Below,
    Left,
    Right
}

[Serializable]
public class TutorialStep
{
    [Header("Step identity")]
    public string stepName;
    public bool onlyShowOnce;
    public string seenPlayerPrefsKey;

    [Header("Dialogue")]
    [TextArea(2, 5)] public List<string> dialogueLines = new List<string>();
    [TextArea(1, 3)] public string wrongActionMessage = "Try the highlighted action first.";

    [Header("Required action")]
    public TutorialAction requiredAction = TutorialAction.None;
    public bool allowNextWithoutRequiredAction;
    public bool disableTapToNextWhileWaitingForAction = true;

    [Header("Visuals")]
    public TutorialVisualMode visualMode = TutorialVisualMode.None;
    public RectTransform focusTarget;
    public string fallbackTargetObjectName;
    public GameObject optionalHighlightObject;

    [Header("Overlay / cutout")]
    public bool showDarkOverlay = true;
    public bool allowClicksThroughCutout = true;
    public bool blockWholeScreenWhenNoTarget = false;

    [Header("Magic hand")]
    public TutorialTargetPoint handTargetPoint = TutorialTargetPoint.Center;
    public Vector2 handExtraOffset;
    public bool playHandTap = true;

    [Header("Arrow")]
    public TutorialArrowSide arrowSide = TutorialArrowSide.Above;
    public float arrowDistanceFromTarget = 80f;
    public Vector2 arrowExtraOffset;
    public float arrowExtraRotationZ;
    public bool bounceArrow = true;

    public bool UsesHighlight
    {
        get
        {
            return visualMode == TutorialVisualMode.HighlightOnly ||
                   visualMode == TutorialVisualMode.MagicHandAndHighlight ||
                   visualMode == TutorialVisualMode.ArrowAndHighlight;
        }
    }

    public bool UsesMagicHand
    {
        get
        {
            return visualMode == TutorialVisualMode.MagicHandOnly ||
                   visualMode == TutorialVisualMode.MagicHandAndHighlight;
        }
    }

    public bool UsesArrow
    {
        get
        {
            return visualMode == TutorialVisualMode.ArrowOnly ||
                   visualMode == TutorialVisualMode.ArrowAndHighlight;
        }
    }
}

[Serializable]
public class TutorialSection
{
    public string sectionName;
    public int startStepIndex;
    public int endStepIndex;
}
