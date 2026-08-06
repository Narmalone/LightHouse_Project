using LightHouse.Core.Inputs;

public class GameManager : PersistentSingleton<GameManager>
{
    public static bool IsQuitting { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        IsQuitting = false;
    }

    private void Start()
    {
        //InitializePlayerInputs();
    }

    public void InitializePlayerInputs()
    {
        //InputManager.Initialize();
    }

    public void ReleasePlayerInputs()
    {
        InputManager.DisposePlayerInputActions();
    }

    private void OnApplicationQuit()
    {
        IsQuitting = true;
        ReleasePlayerInputs();
    }
}