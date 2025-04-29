namespace LightHouse.Interactions
{
    /// <summary>
    /// Class to handle datas and UI with the interface <see cref="IItemName"/>
    /// </summary>
    public class RaycastNameDisplayHandler
    {
        #region FIELDS & PROPERTIES
        private CanvasInteraction _interactionCanvas;
        private IItemName _currentItemName;
        public bool HasTarget => _currentItemName != null;
        #endregion

        #region CONSTRUCTOR
        public RaycastNameDisplayHandler(CanvasInteraction interactionCanva)
        {
            _interactionCanvas = interactionCanva;
        }
        #endregion

        #region SET METHODS
        /// <summary>
        /// Mainly used by <see cref="PlayerInteractableManager"/>, to display the current Item seen.
        /// </summary>
        public void SetTarget(IItemName item)
        {
            if (_currentItemName != null)
            {
                _currentItemName.OnNameUpdated -= UpdateName;
                _currentItemName.IsItemRaycasted = false;
            }
            _currentItemName = item;
            if (_currentItemName == null)
            {
                _interactionCanvas.HideItemName();
                return;
            }
            _currentItemName.IsItemRaycasted = true;
            _currentItemName.OnNameUpdated += UpdateName;
            UpdateName(_currentItemName.GetName());
        }
        #endregion

        #region UI
        /// <summary>
        /// Show or hide the name to display on the screen
        /// </summary>
        private void UpdateName(string newvalue)
        {
            if (_currentItemName == null) return;
            if (!_currentItemName.CanBeRaycasted || string.IsNullOrEmpty(_currentItemName.GetName()))
                _interactionCanvas.HideItemName();
            else
            {
                _interactionCanvas.ItemName_TMP.text = _currentItemName.GetName();
                _interactionCanvas.ShowItemName();
            }
        }
        #endregion
    }

}
