using LightHouse.Core.Audio;
using LightHouse.Core.Utilities;
using LightHouse.Features.Boats;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(
        fileName = "Step03_GiveDirection",
        menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step03_GiveDirection"
    )]
    public class Step03_GiveDirection : TutorialStep
    {
        #region ENUM

        private enum StepState
        {
            WaitingForBoat,
            FirstChoice,
            RepairAfterFirstChoice,
            SecondChoice,
            RepairAfterSecondChoice,
            GoodEnding,
            BadEnding,
            Completed
        }

        #endregion


        #region REFERENCES

        private TutorialContext _context;

        #endregion


        #region CONFIGURATION

        [Header("Choice Settings")]
        [SerializeField] private float _timerChoiceDuration = 10f;


        [Header("First Choice")]

        [Tooltip("Premier choix. La bonne réponse est DROITE.")]
        [SerializeField]
        private LocalizedDialogueAudio _choiceIntroduction;


        [Header("Second Choice")]

        [Tooltip("Deuxième choix si aucune erreur n'a été faite.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainGoodWayGuide;

        [Tooltip("Deuxième choix si le joueur a déjà fait une erreur.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainBadWayGuide;


        [Header("Repair")]

        [Tooltip("Dialogue indiquant qu'il faut réparer le bateau.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainBadWayRepairEngine;

        [Tooltip("Dialogue joué lorsque la séquence de réparation est terminée.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainBadWayEndSubTutorial;


        [Header("Ending")]

        [Tooltip("Fin si les deux choix ont été corrects.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainGoodWay;

        [Tooltip("Fin si au moins une erreur a été faite.")]
        [SerializeField]
        private LocalizedDialogueAudio _captainBadWay;

        #endregion


        #region RUNTIME

        private Timer _choiceTimer;

        private StepState _state;

        /// <summary>
        /// Devient true dès qu'au moins une erreur est faite.
        /// Ne repasse jamais à false pendant cette Step.
        /// </summary>
        private bool _hasMadeMistake;

        /// <summary>
        /// Permet de considérer un timeout comme une erreur,
        /// même si ForceChoice sélectionne accidentellement
        /// la bonne direction.
        /// </summary>
        private bool _choiceForcedByTimer;

        private bool _timerActive;

        #endregion


        #region LIFECYCLE

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            _state = StepState.WaitingForBoat;

            _hasMadeMistake = false;
            _choiceForcedByTimer = false;
            _timerActive = false;

            _choiceTimer = new Timer(_timerChoiceDuration);

            ObjectiveManager.Current.SetObjective(
                "Give the captain directions"
            );

            SubscribeEvents();
        }


        public override void Tick(TutorialContext context, float dt)
        {
            if (_timerActive)
            {
                _choiceTimer.Tick(dt);
            }
        }


        public override void Exit(TutorialContext context)
        {
            StopChoiceTimer();
            UnsubscribeEvents();
        }

        #endregion


        #region EVENTS

        private void SubscribeEvents()
        {
            // Boat
            _context.TutoBoat.OnChoiceRequired += OnBoatChoiceRequired;

            // Choices
            _choiceIntroduction.OnChoiceSelected += OnFirstChoiceSelected;

            _captainGoodWayGuide.OnChoiceSelected += OnSecondChoiceSelected;
            _captainBadWayGuide.OnChoiceSelected += OnSecondChoiceSelected;

            // Dialogues
            _context.TalkieManager.OnDialogueFinished += OnDialogueFinished;

            // Timer
            _choiceTimer.OnTimerComplete += OnChoiceTimerFinished;
        }


        private void UnsubscribeEvents()
        {
            if (_context == null)
                return;

            // Boat
            _context.TutoBoat.OnChoiceRequired -= OnBoatChoiceRequired;

            // Choices
            _choiceIntroduction.OnChoiceSelected -= OnFirstChoiceSelected;

            _captainGoodWayGuide.OnChoiceSelected -= OnSecondChoiceSelected;
            _captainBadWayGuide.OnChoiceSelected -= OnSecondChoiceSelected;

            // Dialogues
            _context.TalkieManager.OnDialogueFinished -= OnDialogueFinished;

            // Timer
            if (_choiceTimer != null)
            {
                _choiceTimer.OnTimerComplete -= OnChoiceTimerFinished;
            }
        }

        #endregion


        #region BOAT FLOW

        private void OnBoatChoiceRequired()
        {
            // Le bateau reste arrêté pendant tout le choix.
            _context.TutoBoat.Pause();

            switch (_context.TutoBoat.CurrentChoiceStepIndex)
            {
                // -------------------------------------------------
                // CHOIX 1
                // Bonne réponse = DROITE
                // -------------------------------------------------

                case 0:
                    StartFirstChoice();
                    break;


                // -------------------------------------------------
                // CHOIX 2
                // Bonne réponse = MILIEU
                // -------------------------------------------------

                case 1:
                    StartSecondChoice();
                    break;
            }
        }


        private void StartFirstChoice()
        {
            _state = StepState.FirstChoice;

            _choiceForcedByTimer = false;

            StartChoiceTimer();

            _context.TalkieManager.Enqueue(
                _choiceIntroduction
            );
        }


        private void StartSecondChoice()
        {
            _state = StepState.SecondChoice;

            _choiceForcedByTimer = false;

            StartChoiceTimer();

            /*
             * On peut utiliser deux dialogues différents :
             *
             * - aucun problème auparavant
             * - le capitaine sait qu'on a déjà fait une erreur
             *
             * Les deux doivent contenir les mêmes 3 choix :
             *
             * 0 = Gauche
             * 1 = Milieu
             * 2 = Droite
             */

            if (_hasMadeMistake)
            {
                _context.TalkieManager.Enqueue(
                    _captainBadWayGuide
                );
            }
            else
            {
                _context.TalkieManager.Enqueue(
                    _captainGoodWayGuide
                );
            }
        }

        #endregion


        #region FIRST CHOICE

        private void OnFirstChoiceSelected(TalkieChoice choice)
        {
            // Protection contre un event qui arriverait au mauvais moment.
            if (_state != StepState.FirstChoice)
                return;

            StopChoiceTimer();

            SelectBoatDirection(choice);


            // CHOIX 1 :
            // Droite = Index 2
            bool correctChoice =
                choice.Index == 2 &&
                !_choiceForcedByTimer;


            if (correctChoice)
            {
                OnFirstChoiceCorrect();
            }
            else
            {
                OnFirstChoiceWrong();
            }
        }


        private void OnFirstChoiceCorrect()
        {
            _state = StepState.WaitingForBoat;

            // Rien à réparer.
            // Le bateau peut continuer vers le deuxième choix.
            _context.TutoBoat.Resume();
        }


        private void OnFirstChoiceWrong()
        {
            _hasMadeMistake = true;

            _state = StepState.RepairAfterFirstChoice;

            StartRepairSequence();
        }

        #endregion


        #region SECOND CHOICE

        private void OnSecondChoiceSelected(TalkieChoice choice)
        {
            if (_state != StepState.SecondChoice)
                return;

            StopChoiceTimer();

            SelectBoatDirection(choice);


            // CHOIX 2 :
            // Milieu = Index 1
            bool correctChoice =
                choice.Index == 1 &&
                !_choiceForcedByTimer;


            if (correctChoice)
            {
                OnSecondChoiceCorrect();
            }
            else
            {
                OnSecondChoiceWrong();
            }
        }


        private void OnSecondChoiceCorrect()
        {
            /*
             * Le bateau peut repartir après le choix.
             */
            _context.TutoBoat.Resume();


            /*
             * Si on avait fait une erreur au premier choix,
             * on doit quand même avoir la mauvaise fin.
             */
            if (_hasMadeMistake)
            {
                StartBadEnding();
            }
            else
            {
                StartGoodEnding();
            }
        }


        private void OnSecondChoiceWrong()
        {
            _hasMadeMistake = true;

            _state = StepState.RepairAfterSecondChoice;

            StartRepairSequence();
        }

        #endregion


        #region REPAIR

        private void StartRepairSequence()
        {
            /*
             * Le bateau est déjà en Pause() depuis OnBoatChoiceRequired().
             *
             * IMPORTANT :
             * On ne fait aucun Resume ici.
             * Le bateau reste complètement immobilisé
             * jusqu'à ce que la réparation soit terminée.
             */

            _context.Hammer.SetPickable();

            /*
             * On joue UNIQUEMENT le dialogue qui annonce
             * que le bateau doit être réparé.
             */
            _context.TalkieManager.Enqueue(
                _captainBadWayRepairEngine
            );
        }


        private void OnRepairFinished()
        {
            switch (_state)
            {
                // ---------------------------------------------
                // Erreur au premier choix
                // ---------------------------------------------

                case StepState.RepairAfterFirstChoice:

                    /*
                     * La réparation est terminée.
                     *
                     * On repart vers le deuxième choix.
                     */
                    _state = StepState.WaitingForBoat;

                    _context.TutoBoat.Resume();

                    break;


                // ---------------------------------------------
                // Erreur au deuxième choix
                // ---------------------------------------------

                case StepState.RepairAfterSecondChoice:

                    /*
                     * C'était le dernier choix.
                     *
                     * Après la réparation, on lance directement
                     * la mauvaise fin.
                     */
                    _context.TutoBoat.Resume();

                    StartBadEnding();

                    break;
            }
        }

        #endregion


        #region DIALOGUE FLOW

        private void OnDialogueFinished(
            LocalizedDialogueAudio dialogue
        )
        {
            /*
             * FIN DE LA PARTIE RÉPARATION
             */
            if (dialogue == _captainBadWayEndSubTutorial)
            {
                OnRepairFinished();
                return;
            }


            /*
             * BONNE FIN
             */
            if (dialogue == _captainGoodWay &&
                _state == StepState.GoodEnding)
            {
                CompleteTutorial();
                return;
            }


            /*
             * MAUVAISE FIN
             */
            if (dialogue == _captainBadWay &&
                _state == StepState.BadEnding)
            {
                CompleteTutorial();
            }
        }

        #endregion


        #region ENDINGS

        private void StartGoodEnding()
        {
            StopChoiceTimer();

            _state = StepState.GoodEnding;

            _context.TalkieManager.Enqueue(
                _captainGoodWay
            );
        }


        private void StartBadEnding()
        {
            StopChoiceTimer();

            _state = StepState.BadEnding;

            _context.TalkieManager.Enqueue(
                _captainBadWay
            );
        }


        private void CompleteTutorial()
        {
            if (_state == StepState.Completed)
                return;

            StopChoiceTimer();

            _state = StepState.Completed;

            ObjectiveManager.Current.CompleteObjective();

            IsComplete = true;
        }

        #endregion


        #region BOAT DIRECTION

        private void SelectBoatDirection(TalkieChoice choice)
        {
            switch (choice.Index)
            {
                // GAUCHE
                case 0:
                    _context.TutoBoat.ChooseDirection(
                        BoatDirection.Left
                    );
                    break;


                // MILIEU
                case 1:
                    _context.TutoBoat.ChooseDirection(
                        BoatDirection.Midle
                    );
                    break;


                // DROITE
                case 2:
                    _context.TutoBoat.ChooseDirection(
                        BoatDirection.Right
                    );
                    break;
            }
        }

        #endregion


        #region TIMER

        private void StartChoiceTimer()
        {
            _choiceForcedByTimer = false;

            _timerActive = true;

            _choiceTimer.ResetTimer(false);

            _choiceTimer.StartTimer();
        }


        private void StopChoiceTimer()
        {
            if (_choiceTimer == null)
                return;

            _timerActive = false;

            _choiceTimer.StopTimer();

            _choiceTimer.ResetTimer(false);
        }


        private void OnChoiceTimerFinished()
        {
            if (_state != StepState.FirstChoice &&
                _state != StepState.SecondChoice)
            {
                return;
            }

            _timerActive = false;

            // Un timeout est considéré comme une erreur,
            // même si le choix automatique est le bon chemin.
            _choiceForcedByTimer = true;

            // 0 = Gauche
            // 1 = Milieu
            // 2 = Droite
            _context.TalkieManager.ForceChoice(1);
        }

        #endregion
    }
}