using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialSceneScript : MonoBehaviour
{
    public static TutorialSceneScript Instance { get; private set; }

    [Header("Scene names")]
    [SerializeField] private string tutorialSelectionSceneName = "TutorialSelectionScene";
    [SerializeField] private string tutorialCombatSceneName = "TutorialCombatV2";
    [SerializeField] private string firstRealGameSceneName = "Selection_Scrn";

    [Header("Tutorial flow")]
    [SerializeField] private bool autoStartOnSceneLoad = true;
    [SerializeField] private bool loadSceneWhenTutorialEnds;
    [SerializeField] private string sceneToLoadWhenTutorialEnds = "Selection_Scrn";

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text fallbackDialogueText;
    [SerializeField] private TypewriterText typewriterText;


    [Header("Tutorial visuals")]
    [SerializeField] private TutorialPointer tutorialPointer;
    [SerializeField] private TutorialCutoutBlocker cutoutBlocker;
    [SerializeField] private GameObject darkOverlayObject;

    [Header("Tap / click to advance")]
    [SerializeField] private Button tapToNextButton;

    [Header("Steps and sections")]
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();
    [SerializeField] private List<TutorialSection> sections = new List<TutorialSection>();

    [Header("Optional references")]
    [SerializeField] private handui handUI;
    [SerializeField] private combat_manager combatManager;

    [Header("Debug")]
    [SerializeField] private int currentStepIndex = -1;
    [SerializeField] private int currentDialogueLineIndex;
    [SerializeField] private bool isRunning;

    public bool IsRunning => isRunning;
    public int CurrentStepIndex => currentStepIndex;

    private TutorialStep CurrentStep
    {
        get
        {
            if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
                return null;

            return steps[currentStepIndex];
        }
    }

    private const string SelectionTutorialSkippedKey = "TutorialSelectionSkipped";
    private const string SelectionTutorialCompletedKey = "TutorialSelectionCompleted";
    private const string CombatTutorialSkippedKey = "TutorialCombatSkipped";
    private const string CombatTutorialCompletedKey = "TutorialCombatCompleted";
    private const string CombatTutorialSkipMockShownKey = "TutorialCombatSkipMockShown";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
        
        if (handUI == null)
            handUI = FindAnyObjectByType<handui>();

        if (combatManager == null)
            combatManager = FindAnyObjectByType<combat_manager>();

        if (cutoutBlocker == null)
            cutoutBlocker = FindAnyObjectByType<TutorialCutoutBlocker>();

        if (typewriterText == null && fallbackDialogueText != null)
            typewriterText = fallbackDialogueText.GetComponent<TypewriterText>();

        if (tapToNextButton != null)
        {
            tapToNextButton.onClick.RemoveListener(TutorialNextPressed);
            tapToNextButton.onClick.AddListener(TutorialNextPressed);
        }

        if (IsCombatTutorialScene())
        {
            if (handUI != null)
                handUI.tutorialSceneBool = true;

            if (combatManager != null)
                combatManager.tutorialBool = true;
        }

        HideAllVisuals();

        if (autoStartOnSceneLoad)
            StartTutorial();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (tapToNextButton != null)
            tapToNextButton.onClick.RemoveListener(TutorialNextPressed);
    }

    public void StartTutorial()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("TutorialSceneScript has no tutorial steps assigned.");
            return;
        }
        isRunning = true;
        currentStepIndex = 0;
        currentDialogueLineIndex = 0;

        if (dialoguePanel != null)
        {
            
            dialoguePanel.SetActive(true);
        }

            

        ShowCurrentStep();
    }

    public void StartTutorialAtStep(int stepIndex)
    {
        if (steps == null || steps.Count == 0)
            return;

        currentStepIndex = Mathf.Clamp(stepIndex, 0, steps.Count - 1);
        currentDialogueLineIndex = 0;
        isRunning = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentStep();
    }

    public void StartTutorialSection(string sectionName)
    {
        TutorialSection section = FindSection(sectionName);

        if (section == null)
        {
            Debug.LogWarning("Tutorial section not found: " + sectionName);
            return;
        }

        StartTutorialAtStep(section.startStepIndex);
    }

    // Keep this old method name so existing buttons/scripts do not break.
    public void NextButtonPressed()
    {
        TutorialNextPressed();
    }

    public void TutorialNextPressed()
    {
        if (!isRunning)
            return;

        if (typewriterText != null && typewriterText.IsTyping)
        {
            typewriterText.FinishImmediately();
            return;
        }

        TutorialStep step = CurrentStep;

        if (step == null)
        {
            EndTutorial();
            return;
        }

        if (HasMoreDialogueLines(step))
        {
            currentDialogueLineIndex++;
            ShowCurrentDialogueLine();
            return;
        }

        if (step.requiredAction != TutorialAction.None && !step.allowNextWithoutRequiredAction)
        {
            ShowCorrection(step);
            return;
        }

        CompleteCurrentStep();
    }

    public void PreviousButtonPressed()
    {
        if (!isRunning)
            return;

        if (typewriterText != null && typewriterText.IsTyping)
            typewriterText.FinishImmediately();

        if (currentStepIndex <= 0)
        {
            currentDialogueLineIndex = 0;
            ShowCurrentStep();
            return;
        }

        currentStepIndex--;
        currentDialogueLineIndex = 0;
        ShowCurrentStep();
    }

    public void SkipCurrentTutorialPart()
    {
        if (!isRunning)
            return;

        TutorialSection section = FindSectionContainingStep(currentStepIndex);

        if (section == null)
        {
            CompleteCurrentStep();
            return;
        }

        currentStepIndex = section.endStepIndex + 1;
        currentDialogueLineIndex = 0;

        if (currentStepIndex >= steps.Count)
            EndTutorial();
        else
            ShowCurrentStep();
    }

    public void SkipTutorial()
    {
        PlayerPrefs.SetInt(SelectionTutorialSkippedKey, 1);
        PlayerPrefs.SetInt(SelectionTutorialCompletedKey, 0);
        
        PlayerPrefs.SetInt(CombatTutorialSkippedKey, 1);
        PlayerPrefs.SetInt(CombatTutorialCompletedKey, 0);
        PlayerPrefs.SetInt(CombatTutorialSkipMockShownKey, 0);
        
        PlayerPrefs.Save();

        HideTutorialUI();

        LoadSceneIfNameExists(firstRealGameSceneName);
    }

    public void ReportAction(TutorialAction action)
    {
        if (!isRunning)
            return;

        TutorialStep step = CurrentStep;

        if (step == null || step.requiredAction == TutorialAction.None)
            return;

        if (ActionMatches(step.requiredAction, action))
        {
            CompleteCurrentStep();
        }
        else
        {
            ShowCorrection(step);
        }
    }

    public void GoToCombatTutorialScene()
    {
        LoadSceneIfNameExists(tutorialCombatSceneName);
    }

    public void GoToFirstRealGameScene()
    {
        LoadSceneIfNameExists(firstRealGameSceneName);
    }

    private void ShowCurrentStep()
    {
        if (!isRunning)
            return;

        if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = CurrentStep;

        if (ShouldSkipStep(step))
        {
            currentStepIndex++;
            ShowCurrentStep();
            return;
        }

        currentDialogueLineIndex = Mathf.Clamp(currentDialogueLineIndex, 0, Mathf.Max(0, step.dialogueLines.Count - 1));

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ApplyStepVisuals(step);
        ShowCurrentDialogueLine();
        UpdateTapToNextState(step);
    }

    private void ShowCurrentDialogueLine()
    {
        TutorialStep step = CurrentStep;

        if (step == null)
            return;

        string text = "";

        if (step.dialogueLines != null && step.dialogueLines.Count > 0)
            text = step.dialogueLines[Mathf.Clamp(currentDialogueLineIndex, 0, step.dialogueLines.Count - 1)];

        if (typewriterText != null)
            typewriterText.ShowText(text);
        else if (fallbackDialogueText != null)
            fallbackDialogueText.text = text;
    }

    private void ApplyStepVisuals(TutorialStep step)
    {
        if (step.visualMode == TutorialVisualMode.KeepPrevious)
        {
            ApplyOverlayOnly(step);
            return;
        }

        HideAllVisuals();
        ApplyOverlayOnly(step);

        RectTransform target = ResolveTarget(step);

        if (step.UsesHighlight)
            ShowHighlight(step, target);
        else if (step.showDarkOverlay && step.blockWholeScreenWhenNoTarget && cutoutBlocker != null)
            cutoutBlocker.ClearHole();

        if (tutorialPointer == null || target == null)
            return;

        if (step.UsesMagicHand)
        {
            tutorialPointer.ShowMagicHand(
                target,
                step.handTargetPoint,
                step.handExtraOffset,
                step.playHandTap
            );
        }
        else if (step.UsesArrow)
        {
            tutorialPointer.ShowArrow(
                target,
                step.arrowSide,
                step.arrowDistanceFromTarget,
                step.arrowExtraOffset,
                step.arrowExtraRotationZ,
                step.bounceArrow
            );
        }
    }

    private void ApplyOverlayOnly(TutorialStep step)
    {
        if (darkOverlayObject != null)
            darkOverlayObject.SetActive(step.showDarkOverlay);

        if (cutoutBlocker != null)
            cutoutBlocker.gameObject.SetActive(step.showDarkOverlay);
    }

    private void ShowHighlight(TutorialStep step, RectTransform target)
    {
        if (step.optionalHighlightObject != null)
            step.optionalHighlightObject.SetActive(true);

        if (cutoutBlocker == null)
            return;

        if (target != null)
            cutoutBlocker.SetHole(target, !step.allowClicksThroughCutout);
        else
            cutoutBlocker.ClearHole();
    }

    private RectTransform ResolveTarget(TutorialStep step)
    {
        if (step.focusTarget != null)
            return step.focusTarget;

        if (!string.IsNullOrWhiteSpace(step.fallbackTargetObjectName))
        {
            GameObject found = GameObject.Find(step.fallbackTargetObjectName);
            if (found != null)
                return found.GetComponent<RectTransform>();
        }

        if (step.optionalHighlightObject != null)
            return step.optionalHighlightObject.GetComponent<RectTransform>();

        return null;
    }

    private void CompleteCurrentStep()
    {
        TutorialStep step = CurrentStep;

        if (step != null && step.onlyShowOnce)
        {
            string key = GetStepSeenKey(step);
            if (!string.IsNullOrWhiteSpace(key))
                PlayerPrefs.SetInt(key, 1);
        }

        PlayerPrefs.Save();

        currentStepIndex++;
        currentDialogueLineIndex = 0;

        if (currentStepIndex >= steps.Count)
            EndTutorial();
        else
            ShowCurrentStep();
    }

    private void EndTutorial()
    {
        isRunning = false;

        if (IsSelectionTutorialScene())
        {
            PlayerPrefs.SetInt(SelectionTutorialCompletedKey, 1);
            PlayerPrefs.SetInt(SelectionTutorialSkippedKey, 0);
        }
        else if (IsCombatTutorialScene())
        {
            PlayerPrefs.SetInt(CombatTutorialCompletedKey, 1);
            PlayerPrefs.SetInt(CombatTutorialSkippedKey, 0);
        }

        PlayerPrefs.Save();
        HideTutorialUI();

        if (loadSceneWhenTutorialEnds)
            LoadSceneIfNameExists(string.IsNullOrWhiteSpace(sceneToLoadWhenTutorialEnds) ? firstRealGameSceneName : sceneToLoadWhenTutorialEnds);
    }

    private void HideTutorialUI()
    {
        HideAllVisuals();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        EnableTapToNext(false);
    }

    private void HideAllVisuals()
    {
        if (tutorialPointer != null)
            tutorialPointer.HideAll();

        if (cutoutBlocker != null)
        {
            cutoutBlocker.ClearHole();
            cutoutBlocker.gameObject.SetActive(false);
        }

        if (darkOverlayObject != null)
            darkOverlayObject.SetActive(false);
        HideAllOptionalHighlightObjects();
    }

    private void HideAllOptionalHighlightObjects()
    {
        if (steps == null)
            return;

        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i] != null && steps[i].optionalHighlightObject != null)
                steps[i].optionalHighlightObject.SetActive(false);
        }
    }

    private void UpdateTapToNextState(TutorialStep step)
    {
        bool shouldDisable = step.requiredAction != TutorialAction.None &&
                             step.disableTapToNextWhileWaitingForAction &&
                             !step.allowNextWithoutRequiredAction;

        EnableTapToNext(!shouldDisable);
    }

    private void EnableTapToNext(bool enabled)
    {
        if (tapToNextButton == null)
            return;

        tapToNextButton.interactable = enabled;

        Graphic[] graphics = tapToNextButton.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = enabled;
        }
    }

    private bool HasMoreDialogueLines(TutorialStep step)
    {
        if (step.dialogueLines == null)
            return false;

        return currentDialogueLineIndex < step.dialogueLines.Count - 1;
    }

    private void ShowCorrection(TutorialStep step)
    {
        string message = string.IsNullOrWhiteSpace(step.wrongActionMessage)
            ? "Try the highlighted action first."
            : step.wrongActionMessage;

        if (typewriterText != null)
            typewriterText.ShowText(message);
        else if (fallbackDialogueText != null)
            fallbackDialogueText.text = message;
    }

    private bool ActionMatches(TutorialAction expected, TutorialAction actual)
    {
        if (expected == actual)
            return true;

        if (expected == TutorialAction.SelectCard)
            return actual == TutorialAction.SelectAttackCard || actual == TutorialAction.SelectBuffCard;

        return false;
    }

    private bool ShouldSkipStep(TutorialStep step)
    {
        if (step == null || !step.onlyShowOnce)
            return false;

        string key = GetStepSeenKey(step);

        if (string.IsNullOrWhiteSpace(key))
            return false;

        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private string GetStepSeenKey(TutorialStep step)
    {
        if (!string.IsNullOrWhiteSpace(step.seenPlayerPrefsKey))
            return step.seenPlayerPrefsKey;

        if (!string.IsNullOrWhiteSpace(step.stepName))
            return "TutorialStepSeen_" + SceneManager.GetActiveScene().name + "_" + step.stepName;

        return "TutorialStepSeen_" + SceneManager.GetActiveScene().name + "_" + currentStepIndex;
    }

    private TutorialSection FindSectionContainingStep(int stepIndex)
    {
        if (sections == null)
            return null;

        for (int i = 0; i < sections.Count; i++)
        {
            TutorialSection section = sections[i];

            if (section == null)
                continue;

            if (stepIndex >= section.startStepIndex && stepIndex <= section.endStepIndex)
                return section;
        }

        return null;
    }

    private TutorialSection FindSection(string sectionName)
    {
        if (sections == null)
            return null;

        for (int i = 0; i < sections.Count; i++)
        {
            TutorialSection section = sections[i];

            if (section == null)
                continue;

            if (section.sectionName == sectionName)
                return section;
        }

        return null;
    }

    private bool IsSelectionTutorialScene()
    {
        return SceneManager.GetActiveScene().name == tutorialSelectionSceneName;
    }

    private bool IsCombatTutorialScene()
    {
        return SceneManager.GetActiveScene().name == tutorialCombatSceneName || SceneManager.GetActiveScene().name == "TutorialCombatScene";
    }

    private void LoadSceneIfNameExists(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Cannot load scene because the scene name is empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
