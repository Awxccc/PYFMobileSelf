using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private float charactersPerSecond = 35f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private int soundEveryXCharacters = 3;

    private Coroutine typingCoroutine;
    private string fullText = "";
    private bool isTyping;

    public bool IsTyping => isTyping;

    private void Awake()
    {
        if (textBox == null)
            textBox = GetComponent<TMP_Text>();
    }

    public void ShowText(string text)
    {
        fullText = text ?? "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    public void SetTextInstant(string text)
    {
        fullText = text ?? "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = false;
        typingCoroutine = null;

        if (textBox != null)
            textBox.text = fullText;
    }
    
    public void FinishImmediately()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (textBox != null)
            textBox.text = fullText;

        isTyping = false;
        typingCoroutine = null;
    }

    private IEnumerator TypeTextRoutine()
    {
        isTyping = true;

        if (textBox != null)
            textBox.text = "";

        float delay = charactersPerSecond <= 0f ? 0f : 1f / charactersPerSecond;

        for (int i = 0; i < fullText.Length; i++)
        {
            if (textBox != null)
                textBox.text += fullText[i];

            if (typingSound != null && audioSource != null && i % Mathf.Max(1, soundEveryXCharacters) == 0 && !char.IsWhiteSpace(fullText[i]))
                audioSource.PlayOneShot(typingSound);

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            else
                yield return null;
        }

        isTyping = false;
        typingCoroutine = null;
    }
}
