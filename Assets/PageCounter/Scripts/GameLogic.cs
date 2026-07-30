using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class GameLogic : MonoBehaviour
{
    public GameObject counter;
    public int pageCount;
    private TextMeshProUGUI counterText;

    void Start()
    {
        pageCount = 0;
        counterText = counter.GetComponent<TextMeshProUGUI>();
        UpdateUI();
    }

    public void AddPage()
    {
        pageCount++;
        UpdateUI();
    }

    void UpdateUI()
    {
        counterText.text = pageCount + "/8";
    }
}
