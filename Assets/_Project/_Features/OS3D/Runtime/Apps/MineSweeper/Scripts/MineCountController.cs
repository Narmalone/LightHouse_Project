using TMPro;
using UnityEngine;

public class MineCountController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _mineCountText;
    private int _mineCount;

    public void Initialize(int initialMineCount)
    {
        _mineCount = initialMineCount;
        UpdateMineCountText();
    }

    public void OnFlagPut()
    {
        _mineCount--;
        UpdateMineCountText();
    }

    public void OnFlagRemoved()
    {
        _mineCount++;
        UpdateMineCountText();
    }

    private void UpdateMineCountText()
    {
        _mineCountText.text = _mineCount.ToString();
    }
}
