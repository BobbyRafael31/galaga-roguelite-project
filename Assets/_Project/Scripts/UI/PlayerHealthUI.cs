using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Dependencies")]
    [Tooltip("Horizontal layout group container")]
    [SerializeField] private Transform _iconContainer;
    [SerializeField] private GameObject _lifeIconPrefab;

    private readonly List<Image> _spawnedIcons = new List<Image>();

    private void OnEnable()
    {
        EventBus.OnPlayerHealthInitialized += SetupHealthUI;
        EventBus.OnPlayerHit += UpdateHealthUI;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerHealthInitialized -= SetupHealthUI;
        EventBus.OnPlayerHit -= UpdateHealthUI;
    }

    private void SetupHealthUI(int currentHealth, int maxHealth, Sprite iconSprite)
    {
        foreach (Image oldIcon in _spawnedIcons)
        {
            if (oldIcon != null) Destroy(oldIcon.gameObject);
        }

        _spawnedIcons.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newIconObj = Instantiate(_lifeIconPrefab, _iconContainer);
            Image img = newIconObj.GetComponent<Image>();

            if(iconSprite != null) img.sprite = iconSprite;

            _spawnedIcons.Add(img);
        }

        UpdateHealthUI(currentHealth);
    }

    private void UpdateHealthUI(int currentHealth)
    {

        for (int i = 0; i < _spawnedIcons.Count; i++)
        {
            _spawnedIcons[i].gameObject.SetActive(i < currentHealth);
        }
    }

}
