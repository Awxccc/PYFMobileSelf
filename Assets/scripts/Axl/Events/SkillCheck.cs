using System;
using System.Collections;
using TMPro;
using UnityEngine;
/** 
@brief Handles skill checks and the content required
*/
public class SkillCheck : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetNumber;
    [SerializeField] private TextMeshProUGUI rolledNumber;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI successOrFailText;
    [SerializeField] private string successDescription;
    [SerializeField] private string failDescription;
    public int target;
    [SerializeField] private GameObject okButton;
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private Animation diceAnimator;
    public bool success = true;
    private string checkSuccess = "Skill Check Success!";
    private string checkFail= "Skill Check Unsuccessful.";
    public Action<bool> OnSkillCheckFinished;

    private void Start()
    {
        targetNumber.text = target.ToString();

        StartCoroutine(RollDice());
    }

    public void SkillCheckOkButton()
    {
        OnSkillCheckFinished?.Invoke(success);
        StartCoroutine(WaitForPlayerHatCoroutine());
    }
    private IEnumerator WaitForPlayerHatCoroutine()
    {
        if (Player_data.instance.addHatCoroutine != null)
        {
            Debug.Log("Stuck");
            yield return null;
        }
        Debug.Log("Set panels inactive");
        eventPanel.SetActive(false);
        gameObject.SetActive(false);
    }
    /** 
    @brief Randomise values while rolling and then get a final rolled value. Compared against targetNumber to see if skill check is successful or not.
    */
    private IEnumerator RollDice()
    {
        float duration = 2f;
        float elapsed = 0f;

        diceAnimator.Play("DiceRoll");

        while (elapsed < duration)
        {
            int randomValue = UnityEngine.Random.Range(1, 21);
            rolledNumber.text = randomValue.ToString();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Final roll result
        int finalRoll = UnityEngine.Random.Range(1, 21);
        rolledNumber.text = finalRoll.ToString();

        if (finalRoll >= target)
        {
            success = true;
            successOrFailText.text = checkSuccess;
            descriptionText.text = successDescription;
        }
        else
        {
            success = false;
            successOrFailText.text = checkFail;
            descriptionText.text = failDescription;
        }

        okButton.SetActive(true);
    }
}
