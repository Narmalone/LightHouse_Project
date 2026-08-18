using LightHouse.Core.Tutorial;
using UnityEngine;

namespace LightHouse.Core.Tutorial
{
    [CreateAssetMenu(fileName = "Step07_MainFacilityStart", menuName = GlobalAssetsMenuPaths.TutorialAssetsMenuPath + "Step07_MainFacilityStart")]

    public class Step07_MainFacilityStart : TutorialStep
    {
        private TutorialContext _context;

        public override void Enter(TutorialContext context)
        {
            base.Enter(context);

            _context = context;

            ObjectiveManager.Current.SetObjective("Find a way to enter.");
        }

        //trouver la clé ou briser la fenêtre
        //si clé trouvée: objectif, ouvrir la porte
        //si fenêtre brisée:

        public override void Exit(TutorialContext context)
        {

        }
    }
}