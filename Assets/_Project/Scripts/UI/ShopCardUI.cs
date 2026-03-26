using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI LevelText;
    public Image IconImage;
    public GameObject HighlightFrame;

    public void Setup(UpgradeData data, int currentLevel, int cost)
    {
        if(data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        NameText.text = data.UpgradeName;
        DescText.text = data.Description;
        CostText.text = $"SCORE: {cost}";
        LevelText.text = $"[LV {currentLevel}/{data.MaxLevel}]";

        if (data.CoinIcon != null)
        {
            IconImage.sprite = data.CoinIcon;
            IconImage.enabled = true;
        }
        else
        {
            IconImage.enabled = false;
        }
    }
    public void SetHighlight(bool isSelected)
    {
        if (HighlightFrame != null) HighlightFrame.SetActive(isSelected);
    }
}
