using System.Collections;

public interface IBootView
{
    void Show();
    void Hide();
    void Clear();
    IEnumerator TypeLine(BootStep step);
}