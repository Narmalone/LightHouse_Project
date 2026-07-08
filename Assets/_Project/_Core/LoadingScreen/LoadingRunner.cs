using System;
using System.Collections;

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
}