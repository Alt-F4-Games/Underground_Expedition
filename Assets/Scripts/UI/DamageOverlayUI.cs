using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageOverlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image overlayImage;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.03f;
    [SerializeField] private float fadeOutTime = 0.15f;

    [Header("Visual")]
    [SerializeField] private float maxAlpha = 0.5f;
    [SerializeField] private bool randomRotation = true;
    [SerializeField] private bool randomScale = true;
    
    [Header("Color")]
    [SerializeField] private Color lowDamageColor = Color.white;
    [SerializeField] private Color highDamageColor = Color.red;

    private Coroutine _routine;
    private float _currentAlpha = 0f;

    private void Awake()
    {
        SetAlpha(0);
    }
    
    public void PlayEffect(float intensity = 1f)
    {
        intensity = Mathf.Clamp01(intensity);
        
        Color targetColor = Color.Lerp(lowDamageColor, highDamageColor, intensity);
        targetColor.a = 0;
        overlayImage.color = targetColor;
        
        if (randomRotation)
        {
            float rot = Random.Range(0f, 360f);
            overlayImage.rectTransform.rotation = Quaternion.Euler(0, 0, rot);
        }

        if (randomScale)
        {
            float scale = Random.Range(0.95f, 1.1f);
            overlayImage.rectTransform.localScale = Vector3.one * scale;
        }

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(FlashRoutine(intensity));
    }

    private IEnumerator FlashRoutine(float intensity)
    {
        float targetAlpha = maxAlpha * intensity;
        
        float t = 0;
        float startAlpha = _currentAlpha;

        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeInTime);
            SetAlpha(a);
            yield return null;
        }
        
        t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(targetAlpha, 0, t / fadeOutTime);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(0);
        _routine = null;
    }

    private void SetAlpha(float a)
    {
        _currentAlpha = a;

        Color c = overlayImage.color;
        c.a = a;
        overlayImage.color = c;
    }
}