using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingStep
{
    public string Name;
    public Func<IEnumerator> Routine;
    public float Weight = 1f;
}

public class LoadingRunner
{
    private readonly LoadingScreen _screen;

    public LoadingRunner(LoadingScreen screen)
    {
        _screen = screen;
    }
    public IEnumerator RunStep(string name, Func<IEnumerator> routine)
    {
        _screen.Show();
        _screen.SetLabel(name);

        IEnumerator r = routine();

        while (r.MoveNext())
        {
            yield return r.Current;
        }

        _screen.Hide();
    }

    public IEnumerator Run(List<LoadingStep> steps)
    {
        _screen.Show();

        float totalWeight = 0f;
        foreach (var step in steps)
            totalWeight += step.Weight;

        float progress = 0f;

        foreach (var step in steps)
        {
            _screen.SetLabel(step.Name);

            float stepProgress = 0f;

            IEnumerator routine = step.Routine();

            while (routine.MoveNext())
            {
                // Progression lissée
                stepProgress += Time.deltaTime; // fallback si pas de vrai progress

                float normalized = Mathf.Clamp01(stepProgress);
                float global = progress + normalized * (step.Weight / totalWeight);

                _screen.SetProgress(global);

                yield return routine.Current;
            }

            progress += step.Weight / totalWeight;
            _screen.SetProgress(progress);
        }

        yield return new WaitForSeconds(0.2f);
        _screen.Hide();
    }
}