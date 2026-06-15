using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoachTutorialSkipManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skipPromptPanel;
    [SerializeField] private TMP_Text coachDialogueText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Settings")]
    [SerializeField] private string firstPromptText = "Do you want to skip the tutorial?";
    [SerializeField] private string areYouSureText = "Are you really sure? I wouldn't recommend it.";
    [SerializeField] private float moveRadius = 50f;

    private RectTransform yesButtonRect;
    private Vector2 originalYesPosition;

    private const string HasTriedToSkipKey = "HasTriedToSkipTutorialTrick";
    public GameObject tutorialCanvas;
    private void Start()
    {
        if (tutorialCanvas == null)
        {
            tutorialCanvas = GameObject.Find("TutorialUICanvasStart");

        }
        tutorialCanvas.SetActive(false);
        yesButtonRect = yesButton.GetComponent<RectTransform>();
        originalYesPosition = yesButtonRect.anchoredPosition;

        skipPromptPanel.SetActive(true);
        coachDialogueText.text = firstPromptText;

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    private void OnYesClicked()
    {
        bool hasSeenTrick = PlayerPrefs.GetInt(HasTriedToSkipKey, 0) == 1;

        if (!hasSeenTrick)
        {
            coachDialogueText.text = areYouSureText;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            yesButtonRect.anchoredPosition = originalYesPosition + (randomDirection * moveRadius);

            PlayerPrefs.SetInt(HasTriedToSkipKey, 1);
            PlayerPrefs.Save();
        }
        else
        {
            skipPromptPanel.SetActive(false);
            if (TutorialSceneScript.Instance != null)
            {
                TutorialSceneScript.Instance.SkipTutorial();
            }
        }
    }

    private void OnNoClicked()
    {
        tutorialCanvas.gameObject.SetActive(true);
        skipPromptPanel.SetActive(false);
        if (TutorialSceneScript.Instance != null)
        {
            TutorialSceneScript.Instance.StartTutorial();
        }
    }
}