using TMPro;
using UnityEngine;
using System.Collections;
using LightHouse.Core.Services;

public class BootTextView : MonoBehaviour, IBootView
{
    [SerializeField] private TextMeshProUGUI text;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void Clear()
    {
        text.text = "";
    }

    public IEnumerator TypeLine(BootStep step)
    {
        text.color = step.textColor;

        foreach (char c in step.text)
        {
            // 🎨 légère variation de couleur (instabilité visuelle)
            text.color = Color.Lerp(
                step.textColor,
                new Color(step.textColor.r * 1.2f, step.textColor.g * 0.8f, step.textColor.b * 0.8f),
                Random.Range(0f, 0.15f)
            );

            // 🧠 GLITCH: répétition de caractère
            if (Random.value < 0.05f)
            {
                text.text += c;
                yield return new WaitForSeconds(0.01f);
            }

            text.text += c;

            // 🔊 son typing
            if (step.typingSound != null)
                ServiceLocator.Audio?.PlayAt(step.typingSound, Vector3.zero);

            // 💀 caractère parasite temporaire
            if (Random.value < 0.05f)
            {
                char glitchChar = Random.value > 0.5f ? '#' : '?';
                text.text += glitchChar;

                yield return new WaitForSeconds(0.02f);

                // remove parasite
                text.text = text.text.Remove(text.text.Length - 1);
            }

            // 🧠 effet "correction" (backspace fantôme)
            if (Random.value < 0.03f && text.text.Length > 0)
            {
                text.text = text.text.Remove(text.text.Length - 1);
                yield return new WaitForSeconds(0.02f);
                text.text += c;
            }

            // ⏱ délai instable
            float delay = step.baseCharDelay + Random.Range(0f, step.randomCharDelay);

            // 💀 pause aléatoire (effet bug)
            if (step.randomPauses && Random.value < step.pauseChance)
            {
                yield return new WaitForSeconds(step.pauseDuration);
            }

            yield return new WaitForSeconds(delay);
        }

        // ⚡ GLITCH de ligne complète (rare)
        if (step.glitch && Random.value < 0.3f)
        {
            text.text += "\nERROR";
        }

        text.text += "\n";

        // 🎬 glitch visuel + audio
        if (step.glitch)
        {
            if (step.glitchSound != null)
                ServiceLocator.Audio?.PlayAt(step.glitchSound, Vector3.zero);

            StartCoroutine(GlitchEffect());
        }

        yield return new WaitForSeconds(step.postDelay);
    }

    private IEnumerator GlitchEffect()
    {
        float duration = 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            text.rectTransform.localPosition =
                new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);

            yield return null;
        }

        text.rectTransform.localPosition = Vector3.zero;
    }
}