using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class GeneratorSwitch : InteractableSwitch
    {
        private static readonly int EmissiveColorID = Shader.PropertyToID("_EmissiveColor");

        [Header("Emission Intensity")]
        [SerializeField] private float _baseEmissionIntensity = 100.0f;
        [SerializeField] private float _maxEmissionIntensity = 500.0f;
        [SerializeField] private float _blinkDuration = 1.0f; // durée d’un cycle complet
        [SerializeField] private AnimationCurve _blinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Colors")]
        [SerializeField] private Color _disabledColor = Color.red;
        [SerializeField] private Color _enabledColor = Color.green;
        [SerializeField] private Renderer rend;

        [SerializeField] private bool _enableBlink = true;

        private float _blinkTimer = 0f;

        private void Update()
        {
            if (_enableBlink)
            {
                _blinkTimer += Time.deltaTime;

                // Boucle le temps sur la durée du cycle
                float t = (_blinkTimer % _blinkDuration) / _blinkDuration;

                // Récupère la valeur sur la courbe (entre 0 et 1)
                float curveValue = _blinkCurve.Evaluate(t);

                // Interpole l’intensité entre base et max
                float intensity = Mathf.Lerp(_baseEmissionIntensity, _maxEmissionIntensity, curveValue);

                ApplyMaterialProperties(intensity, _disabledColor);
            }

        }

        private void ApplyMaterialProperties(float intensity, Color color)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor(EmissiveColorID, color * intensity);
            rend.SetPropertyBlock(block);
        }

        // Facultatif : pour changer dynamiquement l’intensité de base
        public void SetEmission(float baseIntensity, float maxIntensity)
        {
            _baseEmissionIntensity = baseIntensity;
            _maxEmissionIntensity = maxIntensity;
            _blinkTimer = 0f;
        }

        public override void Interact()
        {
            base.Interact();
            if (_isSwitchOn)
            {
                _enableBlink = false;
                ApplyMaterialProperties(_baseEmissionIntensity, _enabledColor);
            }
            else
            {
                _blinkTimer = 0.0f;
                _enableBlink = true;
            }
        }
    }

}
