using System;
using UnityEngine;

namespace LightHouse.Game.Tutorial
{
    /// <summary>
    /// Chaque Step du tutoriel hérite de cette classe.
    /// Enter on s'abonne aux events, initialise l'état.
    /// Tick est optionnel si on a besoin d'un check continu.
    /// Exit on se désabonne des events.
    /// IsComplete indique si la step est terminée pour passer à la suivante.
    /// </summary>
    public abstract class TutorialStep : ScriptableObject
    {
        public bool IsComplete { get; protected set; }

        public virtual void Enter(TutorialContext ctx) { IsComplete = false; }
        public virtual void Tick(TutorialContext ctx, float dt) { }
        public virtual void Exit(TutorialContext ctx) { }
    }
}
