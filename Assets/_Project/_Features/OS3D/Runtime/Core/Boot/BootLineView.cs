using TMPro;
using UnityEngine;
using System.Collections;
using LightHouse.Core.Services;

public interface IBootLineView
{
    IEnumerator TypeLine(BootStep step);
}

public class BootLineView : MonoBehaviour, IBootLineView
{
    [SerializeField] private TextMeshProUGUI text;

    private Material _material;
    private float _pulseTime;

    private void Awake()
    {
        // 🔥 Material unique par ligne (IMPORTANT)
        _material = new Material(text.fontSharedMaterial);
        text.fontMaterial = _material;
        text.fontSharedMaterial = _material;

        _material.EnableKeyword("GLOW_ON");

        // Valeurs de base glow
        _material.SetFloat(ShaderUtilities.ID_GlowPower, 0.6f);
       /* _material.SetFloat(ShaderUtilities.ID_GlowOuter, 0.4f);
        _material.SetFloat(ShaderUtilities.ID_GlowInner, 0.1f);*/

        //text.color = new Color(0.2f, 0.2f, 0.2f); // texte sombre de base
    }

    private void Update()
    {
        // 🔥 Pulse organique (respiration)
        _pulseTime += Time.deltaTime * 2f;

        float pulse = Mathf.Lerp(0.2f, 1.0f, Mathf.Sin(_pulseTime) * 0.5f + 0.5f);
        _material.SetFloat(ShaderUtilities.ID_GlowPower, pulse);
    }

    public IEnumerator TypeLine(BootStep step)
    {
        text.text = "";

        // 🎨 Couleur HDR 
        Color baseColor = step.textColor;

        _material.SetColor(ShaderUtilities.ID_GlowColor, baseColor);
        _material.SetFloat(ShaderUtilities.ID_GlowPower, step.glitch ? 1f : 0.5f);

        foreach (char c in step.text)
        {
            // 🎨 légère instabilité visuelle
            Color unstable = Color.Lerp(
                baseColor,
                baseColor * Random.Range(0.8f, 1.3f),
                Random.Range(0f, 0.2f)
            );

            _material.SetColor(ShaderUtilities.ID_GlowColor, unstable);
            text.color = unstable * 0.3f; // texte plus sombre que le glow

            // 🧠 répétition bug
            if (Random.value < 0.05f)
            {
                text.text += c;
                yield return new WaitForSeconds(0.01f);
            }

            text.text += c;

            // 🔊 typing
            if (step.typingSound != null)
                ServiceLocator.Audio?.PlayAt(step.typingSound, Vector3.zero);

            // 💀 caractère parasite
            if (Random.value < 0.05f)
            {
                char g = Random.value > 0.5f ? '#' : '?';
                text.text += g;

                yield return new WaitForSeconds(0.02f);

                text.text = text.text.Remove(text.text.Length - 1);
            }

            // 🔁 correction fantôme
            if (Random.value < 0.03f && text.text.Length > 0)
            {
                text.text = text.text.Remove(text.text.Length - 1);
                yield return new WaitForSeconds(0.02f);
                text.text += c;
            }

            // 💀 freeze anormal (super important en horreur)
            if (Random.value < 0.03f)
            {
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }

            // ⏱ délai
            float delay = step.baseCharDelay + Random.Range(0f, step.randomCharDelay);

            if (step.randomPauses && Random.value < step.pauseChance)
                yield return new WaitForSeconds(step.pauseDuration);

            yield return new WaitForSeconds(delay);
        }

        // 💀 corruption de ligne (rare mais puissant)
        if (step.glitch && Random.value < 0.25f)
        {
            yield return new WaitForSeconds(0.2f);

            string corrupted = "";
            foreach (char c in text.text)
                corrupted += Random.value < 0.5f ? '#' : c;

            text.text = corrupted;
            _material.SetColor(ShaderUtilities.ID_GlowColor, Color.red);

        }

        // 🎬 glitch visuel
        if (step.glitch)
        {
            if (step.glitchSound != null)
                ServiceLocator.Audio?.PlayAt(step.glitchSound, Vector3.zero);

            StartCoroutine(Glitch());
        }

        yield return new WaitForSeconds(step.postDelay);
    }

    private IEnumerator Glitch()
    {
        float t = 0f;

        while (t < 0.15f)
        {
            t += Time.deltaTime;

            transform.localPosition =
                new Vector3(Random.Range(-4f, 4f), Random.Range(-2f, 2f), 0f);

            _material.SetFloat(ShaderUtilities.ID_GlowPower, Random.Range(0.3f, 2f));

            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }
}