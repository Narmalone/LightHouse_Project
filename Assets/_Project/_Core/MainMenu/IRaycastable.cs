namespace LightHouse.Core.Interaction
{
    #region ===== Raycast =====

    /// <summary>
    /// Appelé lorsque le raycast entre en contact avec l'objet.
    /// </summary>
    public interface IRaycastEnter
    {
        void OnRaycastEnter();
    }

    /// <summary>
    /// Appelé lorsque le raycast quitte l'objet.
    /// </summary>
    public interface IRaycastExit
    {
        void OnRaycastExit();
    }

    #endregion

    #region ===== Click =====

    /// <summary>
    /// Appelé lors d'un clic (pression initiale).
    /// </summary>
    public interface IClickable
    {
        void OnClicked();
    }

    /// <summary>
    /// Appelé tant que le clic est maintenu.
    /// </summary>
    public interface IClickableHold
    {
        void OnClickHold();
    }

    /// <summary>
    /// Appelé lors du relâchement du clic.
    /// </summary>
    public interface IClickableUp
    {
        void OnClickReleased();
    }

    #endregion
}