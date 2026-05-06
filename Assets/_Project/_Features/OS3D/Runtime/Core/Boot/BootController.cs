using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BootController : MonoBehaviour
{
    [SerializeField] private BootSequence sequence;
    [SerializeField] private BootTerminalView terminal;

    [SerializeField] private BootStep _glitchErrorBootStep;
    [SerializeField] private BootStep _creepyBootStep;

    private Coroutine _startBootRoutine;

    public void StartBoot(Action onComplete)
    {
        _startBootRoutine = StartCoroutine(BootRoutine(onComplete));
    }

    public void StopBoot()
    {
        if (_startBootRoutine != null)
        {
            StopCoroutine(_startBootRoutine);
            _startBootRoutine = null;
        }
    }

    private IEnumerator BootRoutine(Action onComplete)
    {
        terminal.Show();
        terminal.Clear();

        foreach (var step in sequence.steps)
        {
            var line = terminal.CreateLine();
            yield return line.TypeLine(step);

            // 👁 message parasite (très rare → impact énorme)
            if (UnityEngine.Random.value < 0.1f)
            {
                var creepyLine = terminal.CreateLine();
                yield return creepyLine.TypeLine(_creepyBootStep);
            }

            if (step.glitch && UnityEngine.Random.value < 0.3f)
            {
                var errorLine = terminal.CreateLine();
                yield return errorLine.TypeLine(_glitchErrorBootStep);
            }
        }

        yield return new WaitForSeconds(0.5f);
        onComplete?.Invoke();
    }
}