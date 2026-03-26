using UnityEngine;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI Dependencies")]
    [SerializeField] private GameObject _shopContainer;
    [SerializeField] private ShopCardUI[] _cards = new ShopCardUI[3];

    [Header("Buttons & Highlights")]
    [SerializeField] private TextMeshProUGUI _rerollText;
    [SerializeField] private GameObject _rerollHighlight;
    [SerializeField] private GameObject _continueHighlight;

    [Header("Input")]
    [SerializeField] private InputReader _inputReader;

    private int _selectedIndex = 0; // 0,1,2 = Cards | 3 = Reroll | 4 = Continue
    private float _lastMoveTime;

    private void OnEnable()
    {
        EventBus.OnDraftGenerated += HandleDraftGenerated;

        if (_inputReader != null)
        {
            _inputReader.OnFireEvent += HandleSelect;
        }
    }

    private void OnDisable()
    {
        EventBus.OnDraftGenerated -= HandleDraftGenerated;

        if (_inputReader != null)
        {
            _inputReader.OnFireEvent -= HandleSelect;
        }
    }

    private void Update()
    {
        if (_shopContainer == null || !_shopContainer.activeInHierarchy) return;

        if (Mathf.Abs(_inputReader.MoveVector.x) > 0.5f && Time.unscaledTime > _lastMoveTime + 0.2f)
        {
            int direction = (int)Mathf.Sign(_inputReader.MoveVector.x);

            if (_selectedIndex <= 2)
            {
                _selectedIndex += direction;
                if (_selectedIndex > 2) _selectedIndex = 0;
                if (_selectedIndex < 0) _selectedIndex = 2;
            }
            else
            {
                _selectedIndex += direction;
                if (_selectedIndex > 4) _selectedIndex = 3;
                if (_selectedIndex < 3) _selectedIndex = 4;
            }

            _lastMoveTime = Time.unscaledTime;
            UpdateHighlights();
        }

        if (Mathf.Abs(_inputReader.MoveVector.y) > 0.5f && Time.unscaledTime > _lastMoveTime + 0.2f)
        {
            int direction = (int)Mathf.Sign(_inputReader.MoveVector.y);

            if (direction < 0 && _selectedIndex <= 2) // Pressed DOWN while on a Card
            {

                _selectedIndex = _selectedIndex == 2 ? 4 : 3;
            }
            else if (direction > 0 && _selectedIndex > 2) // Pressed UP while on a Button
            {
                _selectedIndex = 1;
            }

            _lastMoveTime = Time.unscaledTime;
            UpdateHighlights();
        }
    }

    private void HandleDraftGenerated(UpgradeData[] choices)
    {
        _selectedIndex = 0;
        RedrawShopState();
    }

    private void RedrawShopState()
    {
        UpgradeData[] choices = ShopManager.Instance.CurrentDraftChoices;

        for (int i = 0; i < 3; i++)
        {
            if (choices[i] != null)
            {
                int cost = ShopManager.Instance.GetCurrentUpgradeCost(choices[i]);
                int level = ShopManager.Instance.GetCurrentUpgradeLevel(choices[i]);
                _cards[i].Setup(choices[i], level, cost);
            }
            else
            {
                _cards[i].Setup(null, 0, 0); // Hides the card if purchased
            }
        }

        _rerollText.text = $"REROLL ({ShopManager.Instance.CurrentRerollCost})";
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        for (int i = 0; i < 3; i++) _cards[i].SetHighlight(_selectedIndex == i);

        if (_rerollHighlight != null) _rerollHighlight.SetActive(_selectedIndex == 3);
        if (_continueHighlight != null) _continueHighlight.SetActive(_selectedIndex == 4);
    }

    private void HandleSelect()
    {
        if (_shopContainer == null || !_shopContainer.activeInHierarchy) return;

        if (_selectedIndex >= 0 && _selectedIndex <= 2)
        {
            // Attempt Card Purchase
            if (ShopManager.Instance.TryPurchaseUpgrade(_selectedIndex))
            {
                RedrawShopState();
            }
            else
            {
                Debug.LogWarning("[ShopUI] NOT ENOUGH SCORE TO PURCHASE THIS UPGRADE!");
            }
        }
        else if (_selectedIndex == 3)
        {
            // Attempt Reroll
            if (!ShopManager.Instance.TryReroll())
            {
                Debug.LogWarning("[ShopUI] NOT ENOUGH SCORE TO REROLL!");
            }
        }
        else if (_selectedIndex == 4)
        {
            // Continue to Next Stage
            GameStateManager.Instance.OnLeaveShopClicked();
        }
    }

}
