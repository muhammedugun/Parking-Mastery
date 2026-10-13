using UnityEngine;

/// <summary>
/// Tek bir tekerlek: WheelCollider'ın dünya pozunu görsel tekerlek modeline aktarır.
/// CarControl, tekerleğin direksiyon/tahrik yeteneğini (steerable / motorized) ve collider'ını buradan okur.
/// </summary>

namespace ParkingMastery.Car
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelControl : MonoBehaviour
    {
        [Tooltip("Collider pozunu takip edecek görsel tekerlek modeli.")]
        [SerializeField] private Transform wheelModel;

        public bool steerable;
        public bool motorized;

        /// <summary>Bu objedeki WheelCollider (Awake'te alınır; RequireComponent sayesinde her zaman vardır).</summary>
        public WheelCollider WheelCollider { get; private set; }

        private void Awake()
        {
            WheelCollider = GetComponent<WheelCollider>();

            if (wheelModel == null)
            {
                Debug.LogWarning("WheelControl: 'wheelModel' atanmamış, bu tekerlek görsel olarak dönmeyecek.", this);
                enabled = false;
            }
        }

        // Poz, fizik ve araç interpolation'ı güncellendikten sonra (kare sonunda) alınır; tekerlek geride kalmaz.
        private void LateUpdate()
        {
            WheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheelModel.SetPositionAndRotation(position, rotation);
        }
    }
}
