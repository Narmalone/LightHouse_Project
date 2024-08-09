using UnityEngine;

public class ElectricityManager : MonoBehaviour
{
    [SerializeField] private ElectricPannelController electricPannelController;
    [SerializeField] private GeneratorController generatorController;

    private void Awake()
    {
        generatorController.OnGeneratorFuelEmpty += GeneratorController_OnGeneratorFuelEmpty;
        generatorController.OnGeneratorFuelFilledFromEmpty += GeneratorController_OnGeneratorFuelFilledFromEmpty; ;
    }

    private void OnDestroy()
    {
        generatorController.OnGeneratorFuelEmpty -= GeneratorController_OnGeneratorFuelEmpty;
        generatorController.OnGeneratorFuelFilledFromEmpty -= GeneratorController_OnGeneratorFuelFilledFromEmpty; ;
    }

    private void GeneratorController_OnGeneratorFuelFilledFromEmpty()
    {
        electricPannelController.OnFuelFilledFromEmpty();
    }

    private void GeneratorController_OnGeneratorFuelEmpty()
    {
        electricPannelController.OnFuelEmpty();

    }
}
