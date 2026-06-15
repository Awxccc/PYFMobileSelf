using TMPro;
using UnityEngine;
using UnityEngine.UI;
/** 
@brief Efficient handling of the tutorial content
*/
public class Tutorial : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI pageNumber;
    [SerializeField] private GameObject welcomeText;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TutorialContent[] contents;
    private int currentPageNumber = 1;

    private void Start()
    {
        tutorialImage.gameObject.SetActive(false);
        pageNumber.text = currentPageNumber.ToString();
    }

    public void NextButton()
    {
        /** 
        @brief Turn off welcome text, turn on tutorial content image
        */
        if (currentPageNumber == 1)
        {
            welcomeText.gameObject.SetActive(false);
            tutorialImage.gameObject.SetActive(true);
        }
        /** 
        @brief Turn off the whole game object when tutorial is done
        */
        else if (currentPageNumber == 7)
        {
            gameObject.SetActive(false);
            return;
        }
        /** 
        @brief Switch tutorial content based on page number
        */
        currentPageNumber++;
        pageNumber.text = currentPageNumber.ToString();
        title.text = contents[currentPageNumber - 1].titleText;
        tutorialImage.sprite = contents[currentPageNumber - 1].currentImage;
    }
    /** 
    @brief Skip tutorial entirely before last page
    */
    public void SkipButton()
    {
        gameObject.SetActive(false);
    }
}
