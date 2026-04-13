using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject _uiContainer;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private Image _healthFillImage;


    [Header("Animation")]
    [SerializeField] private float _fillSpeed = 0.5f;

    private float _maxHealth;
    private float _targetFillAmount;
    private float _currentFillAmount;

    private void OnEnable()
    {
        EventBus.OnBossSpawned += HandleBossSpawned;
        EventBus.OnBossHealthChanged += HandleHealthChanged;
        EventBus.OnBossDefeated += HideUI;
        EventBus.OnMainMenuEntered += HideUI;
        EventBus.OnClearArena += HideUI;
    }

    private void OnDisable()
    {
        EventBus.OnBossSpawned -= HandleBossSpawned;
        EventBus.OnBossHealthChanged -= HandleHealthChanged;
        EventBus.OnBossDefeated -= HideUI;
        EventBus.OnMainMenuEntered -= HideUI;
        EventBus.OnClearArena -= HideUI;
    }

    private void Start()
    {
        HideUI();
    }

    private void Update()
    {
        if (_uiContainer == null || !_uiContainer.activeInHierarchy) return;

        if (Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
        {
            _currentFillAmount = Mathf.MoveTowards(_currentFillAmount, _targetFillAmount, _fillSpeed * Time.deltaTime);
            _healthFillImage.fillAmount = _currentFillAmount;
        }
    }

    private void HandleBossSpawned(string bossName, float currentHealth, float maxHealth)
    {
        _maxHealth = maxHealth;
        _targetFillAmount = Mathf.Clamp01(currentHealth / _maxHealth);
        _currentFillAmount = _targetFillAmount;

        if (_healthFillImage != null) _healthFillImage.fillAmount = _currentFillAmount;
        if (_bossNameText != null) _bossNameText.text = bossName.ToUpper();

        if (_uiContainer != null) _uiContainer.SetActive(true);
    }

    private void HandleHealthChanged(float currentHealth)
    {
        if (_maxHealth <= 0) return;

        _targetFillAmount = Mathf.Clamp01(currentHealth / _maxHealth);
    }

    private void HideUI()
    {
        if (_uiContainer != null) _uiContainer.SetActive(false);
    }
}
