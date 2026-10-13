using System;
using UnityEngine;

namespace ParkingMastery.Car
{
    /// <summary>
    /// Lastik efektleri: tekerlek kayarken (yanal/boyuna kayma ya da el freni) yerde iz (TrailRenderer) ve lastik dumanı,
    /// hızlıyken hafif toz bırakır. WheelCollider'ın zemin temasından (GetGroundHit) beslenir; kare başına tahsis yoktur.
    /// İz ve parçacık nesneleri prefab'da hazır durur; bu script yalnızca konumlarını ve açık/kapalı durumlarını sürer.
    /// </summary>
    [DisallowMultipleComponent]
    public class CarTireEffects : MonoBehaviour
    {
        [Serializable]
        private class WheelEffect
        {
            [Tooltip("Zemin temasını okuyacağımız WheelCollider.")]
            public WheelCollider collider;

            [Tooltip("Kayarken yerde iz bırakan TrailRenderer (dünya uzayında).")]
            public TrailRenderer skidMark;

            [Tooltip("Lastik dumanı / toz parçacığı (simulationSpace = World).")]
            public ParticleSystem smoke;

            [NonSerialized] public ParticleSystem.EmissionModule Emission;
            [NonSerialized] public Vector3 LastMarkPosition;
            [NonSerialized] public bool HasLastMark;
        }

        [Tooltip("Hız ve el freni bilgisini veren araç. Boşsa üst objelerde aranır.")]
        [SerializeField] private CarControl car;

        [SerializeField] private WheelEffect[] wheels;

        [Header("Kayma (iz + duman)")]
        [Tooltip("Bu hızın (m/sn) altında iz/duman çıkmaz (yerinde dönen tekerlek iz bırakmasın).")]
        [SerializeField, Min(0f)] private float minSkidSpeed = 1.5f;

        [Tooltip("WheelHit yanal/boyuna kayma değeri bu eşiği aşınca tekerlek kayıyor sayılır.")]
        [SerializeField, Min(0.05f)] private float slipThreshold = 0.5f;

        [Tooltip("Kayma sırasında saniyedeki duman parçacığı sayısı (tekerlek başına).")]
        [SerializeField, Min(0f)] private float smokeRate = 22f;

        [Header("Toz (hızlı sürüş)")]
        [Tooltip("Bu hızın (m/sn) üstünde, kaymasa da hafif toz çıkar.")]
        [SerializeField, Min(0f)] private float dustMinSpeed = 4f;

        [Tooltip("Hafif toz için saniyedeki parçacık sayısı (tekerlek başına).")]
        [SerializeField, Min(0f)] private float dustRate = 4f;

        [Header("Yerleşim")]
        [Tooltip("İzin zeminden yukarı kaydırma miktarı (z-fighting olmasın).")]
        [SerializeField, Min(0f)] private float markLift = 0.02f;

        [Tooltip("Tek karede bu kadar metreden fazla sıçrayan iz konumu ışınlanma sayılır ve iz silinir (ResetCar vb.).")]
        [SerializeField, Min(0.5f)] private float teleportDistance = 3f;

        private void Awake()
        {
            if (car == null)
                TryGetComponent(out car);

            if (car == null)
                car = GetComponentInParent<CarControl>();

            if (wheels == null)
                return;

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null && wheels[i].smoke != null)
                    wheels[i].Emission = wheels[i].smoke.emission;
            }
        }

        private void OnDisable()
        {
            if (wheels == null)
                return;

            for (int i = 0; i < wheels.Length; i++)
            {
                WheelEffect w = wheels[i];
                if (w == null)
                    continue;

                if (w.skidMark != null)
                    w.skidMark.emitting = false;

                w.HasLastMark = false;

                if (w.smoke != null)
                {
                    w.Emission.rateOverTime = 0f;
                }
            }
        }

        // Tekerlek pozu ve fizik güncellendikten sonra (WheelControl ile aynı aşamada) çalışır.
        private void LateUpdate()
        {
            if (car == null || wheels == null)
                return;

            float speed = car.CurrentSpeed;
            bool braking = car.IsBraking;

            for (int i = 0; i < wheels.Length; i++)
            {
                WheelEffect w = wheels[i];
                if (w == null || w.collider == null)
                    continue;

                bool grounded = w.collider.GetGroundHit(out WheelHit hit);

                bool skidding = false;
                if (grounded && speed > minSkidSpeed)
                {
                    float slip = Mathf.Max(Mathf.Abs(hit.sidewaysSlip), Mathf.Abs(hit.forwardSlip));
                    skidding = slip > slipThreshold || (braking && Mathf.Abs(hit.sidewaysSlip) > slipThreshold * 0.5f);
                }

                UpdateSkidMark(w, hit, grounded, skidding);
                UpdateSmoke(w, hit, grounded, skidding, speed);
            }
        }

        private void UpdateSkidMark(WheelEffect w, WheelHit hit, bool grounded, bool skidding)
        {
            TrailRenderer trail = w.skidMark;
            if (trail == null)
                return;

            if (grounded)
            {
                Vector3 position = hit.point + hit.normal * markLift;

                // Işınlanma (ResetCar vb.): uzun düz bir çizgi kalmasın.
                if (w.HasLastMark && (position - w.LastMarkPosition).sqrMagnitude > teleportDistance * teleportDistance)
                    trail.Clear();

                // TransformZ hizası: iz yüzeyinin normali objenin ileri ekseni olmalı (yere yatık).
                trail.transform.SetPositionAndRotation(position, Quaternion.LookRotation(hit.normal, car.transform.forward));
                w.LastMarkPosition = position;
                w.HasLastMark = true;
            }

            trail.emitting = grounded && skidding;
        }

        private void UpdateSmoke(WheelEffect w, WheelHit hit, bool grounded, bool skidding, float speed)
        {
            if (w.smoke == null)
                return;

            float rate = 0f;
            if (grounded)
            {
                if (skidding)
                    rate = smokeRate;
                else if (speed > dustMinSpeed)
                    rate = dustRate;

                w.smoke.transform.position = hit.point;
            }

            w.Emission.rateOverTime = rate;
        }
    }
}
