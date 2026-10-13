using UnityEngine;

namespace ParkingMastery.Car
{
    /// <summary>
    /// Araç lambalarını sürer. Gövde mesh'i far / stop / geri vites diye ayrı alt mesh'lere bölünmüştür; bu script stop ve geri vites
    /// alt mesh'lerinin emisyonunu MaterialPropertyBlock ile değiştirir (materyal kopyası ya da ek ışık yoktur):
    /// farlar sürekli yanar, stop lambaları normalde loş, frende parlak; geri vites lambaları yalnızca geri viteste yanar.
    /// Yalnızca durum değişince yazar; kare başına tahsis yoktur.
    /// </summary>
    [DisallowMultipleComponent]
    public class CarLights : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Tooltip("Fren / geri vites durumunu veren araç. Boşsa üst objelerde aranır.")]
        [SerializeField] private CarControl car;

        [Tooltip("Lamba alt mesh'lerini taşıyan gövde renderer'ı.")]
        [SerializeField] private Renderer bodyRenderer;

        [Tooltip("Stop lambalarının gövde renderer'ındaki materyal sırası (Body_Lit: 2).")]
        [SerializeField, Min(0)] private int tailMaterialIndex = 2;

        [Tooltip("Geri vites lambalarının gövde renderer'ındaki materyal sırası (Body_Lit: 3).")]
        [SerializeField, Min(0)] private int reverseMaterialIndex = 3;

        [Header("Emisyon (HDR)")]
        [SerializeField, ColorUsage(false, true)] private Color tailIdle = new Color(3f, 3f, 3f);
        [SerializeField, ColorUsage(false, true)] private Color tailBrake = new Color(16f, 16f, 16f);
        [SerializeField, ColorUsage(false, true)] private Color reverseOff = Color.black;
        [SerializeField, ColorUsage(false, true)] private Color reverseOn = new Color(16f, 16f, 16f);

        private MaterialPropertyBlock _tailBlock;
        private MaterialPropertyBlock _reverseBlock;
        private int _materialCount;
        private bool _brakeOn;
        private bool _reverseOn;

        /// <summary>Fren lambaları şu an parlak mı?</summary>
        public bool BrakeLightsOn => _brakeOn;

        /// <summary>Geri vites lambaları şu an açık mı?</summary>
        public bool ReverseLightsOn => _reverseOn;

        private void Awake()
        {
            if (car == null)
                TryGetComponent(out car);

            if (car == null)
                car = GetComponentInParent<CarControl>();

            _tailBlock = new MaterialPropertyBlock();
            _reverseBlock = new MaterialPropertyBlock();
            _materialCount = bodyRenderer != null ? bodyRenderer.sharedMaterials.Length : 0;
        }

        private void OnEnable()
        {
            Apply(car != null && car.IsBraking, car != null && car.IsReversing);
        }

        private void Update()
        {
            if (car == null)
                return;

            bool braking = car.IsBraking;
            bool reversing = car.IsReversing;
            if (braking != _brakeOn || reversing != _reverseOn)
                Apply(braking, reversing);
        }

        private void Apply(bool braking, bool reversing)
        {
            _brakeOn = braking;
            _reverseOn = reversing;

            if (bodyRenderer == null)
                return;

            SetEmission(tailMaterialIndex, _tailBlock, braking ? tailBrake : tailIdle);
            SetEmission(reverseMaterialIndex, _reverseBlock, reversing ? reverseOn : reverseOff);
        }

        private void SetEmission(int materialIndex, MaterialPropertyBlock block, Color color)
        {
            if (block == null || materialIndex >= _materialCount)
                return;

            bodyRenderer.GetPropertyBlock(block, materialIndex);
            block.SetColor(EmissionColorId, color);
            bodyRenderer.SetPropertyBlock(block, materialIndex);
        }
    }
}