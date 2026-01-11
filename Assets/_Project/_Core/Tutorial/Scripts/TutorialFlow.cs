using LightHouse.Core.Player;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    /// <summary>
    /// Possède une liste ordonée d'étapes.
    /// Active l'étape courante, attend sa complétion, puis passe à la suivante.
    /// Ne contient aucune logique métier "regarder le phare", "ramasser les jumelles", etc.
    /// </summary>
    public sealed class TutorialFlow : MonoBehaviour
    {
        [SerializeField] private TutorialStep[] _steps;

        private int _index = -1;
        private TutorialStep _current;
        private TutorialContext _ctx;

        public void Init(TutorialContext ctx)
        {
            _ctx = ctx;
            Next();
        }

        private void Update()
        {
            if (_current == null) return;

            // (optionnel) refresh view transform si ton joueur/cam peut changer
            _ctx.ViewTransform = PlayerHandlerData.MainPlayer?.PlayerCamera?.transform;

            _current.Tick(_ctx, Time.deltaTime);

            if (_current.IsComplete)
                Next();
        }

        private void Next()
        {
            if (_current != null)
                _current.Exit(_ctx);

            _index++;
            if (_steps == null || _index >= _steps.Length)
            {
                _current = null;
                return;
            }

            _current = _steps[_index];
            _current.Enter(_ctx);
        }
    }
}
