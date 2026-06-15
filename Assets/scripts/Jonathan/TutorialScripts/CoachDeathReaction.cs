using TMPro;
using UnityEngine;

public class CoachDeathReaction : MonoBehaviour
{
    [Header("Existing game over UI")]
    [SerializeField] private GameObject coachMockPanel;
    [SerializeField] private TMP_Text coachMockText;

    [Header("Settings")]
    [SerializeField] private bool onlyMockOnce = true;

    private const string CombatTutorialSkippedKey = "TutorialCombatSkipped";
    private const string CombatTutorialSkipMockShownKey = "TutorialCombatSkipMockShown";

    private void OnEnable()
    {
        Player_data.Player_died += ShowMockIfNeeded;
    }

    private void OnDisable()
    {
        Player_data.Player_died -= ShowMockIfNeeded;
    }

    public void ShowMockIfNeeded()
    {
        bool skippedCombatTutorial = PlayerPrefs.GetInt(CombatTutorialSkippedKey, 0) == 1;
        bool alreadyMocked = PlayerPrefs.GetInt(CombatTutorialSkipMockShownKey, 0) == 1;

        if (!skippedCombatTutorial)
            return;

        if (onlyMockOnce && alreadyMocked)
            return;

        if (coachMockPanel != null)
            coachMockPanel.SetActive(true);

        if (coachMockText != null)
            coachMockText.text = GetMockLine();

        PlayerPrefs.SetInt(CombatTutorialSkipMockShownKey, 1);
        PlayerPrefs.Save();
    }

    private string GetMockLine()
    {
        // Notice the dialogue options remain intact but will now be delivered by the "Coach"
        string[] lines =
        {
            "Skipping the combat lesson and falling in battle? A daring academic choice.",
            "I prepared a tutorial. You prepared a demonstration of why it exists.",
            "The lesson was optional. Apparently, so was survival.",
            "Perhaps next time we listen before becoming a cautionary tale.",
            "You skipped my advice. The enemy, tragically, did not skip practice."
        };

        return lines[Random.Range(0, lines.Length)];
    }
}