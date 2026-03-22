using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private CanvasGroup _fadeGroup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;
    }

    public async Awaitable FadeToBlack(float duration)
    {
        _fadeGroup.blocksRaycasts = true;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            _fadeGroup.alpha = t / duration;
            await Awaitable.NextFrameAsync();
        }
        _fadeGroup.alpha = 1f;
    }

    public async Awaitable FadeToClear(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            _fadeGroup.alpha = 1f - (t / duration);
            await Awaitable.NextFrameAsync();
        }
        _fadeGroup.alpha = 0f;
        _fadeGroup.blocksRaycasts = false;
    }
}