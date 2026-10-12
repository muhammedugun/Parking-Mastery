using System.Collections;
using UnityEngine;
using YG;

public class CarControl : MonoBehaviour
{
    [HideInInspector] public bool isGoingForward = true;
    [HideInInspector] public float vInput, hInput;

    [SerializeField] private float motorTorque = 1000;
    [SerializeField] private float brakeTorque = 4000;
    [Header("Gaz bırakma / yavaşlama")]
    [Tooltip("Ters yöne basılınca uygulanan fren torku (Space ile fren etkilenmez).")]
    [SerializeField] private float reverseBrakeTorque = 1500;
    [Tooltip("Gaz bırakılınca düşük hızdaki yavaşlatma torku.")]
    [SerializeField] private float coastBrakeTorqueMin = 150;
    [Tooltip("Gaz bırakılınca maksimum hızdaki yavaşlatma torku (motor freni).")]
    [SerializeField] private float coastBrakeTorqueMax = 400;
    [Tooltip("Bu hızın (m/s) altında araç durma torkuna doğru yumuşakça geçer.")]
    [SerializeField] private float stopHoldSpeed = 1.5f;
    [Tooltip("Araç durmak üzereyken uygulanan tork; sürünmeyi önler.")]
    [SerializeField] private float stopHoldTorque = 800;

    [Header("Bariyer hakkı")]
    [Tooltip("Seviye kaybedilmeden önce bariyerlere kaç kez değmeye izin verilir. Bu sayıya ulaşınca seviye kaybedilir.")]
    [SerializeField, Min(1)] private int maxObstacleHits = 3;
    [Tooltip("İki sayılan çarpma arasındaki en az süre (sn). Aynı temasın çok kez sayılmasını önler.")]
    [SerializeField] private float obstacleHitCooldown = 1f;

    private int _obstacleHitCount;
    private float _lastObstacleHitTime = -10f;

    /// <summary>Kaybetmeden önce kalan bariyer hakkı.</summary>
    public int RemainingHits => Mathf.Max(0, maxObstacleHits - _obstacleHitCount);
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float steeringRange = 30;
    [SerializeField] private float steeringRangeAtMaxSpeed = 10;
    [SerializeField] private float centreOfGravityOffset = -1f;

    public float MaxSpeed => maxSpeed;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    /// <summary>
    /// Arabanın ileri yöndeki hızı
    /// </summary>
    private float forwardSpeed;
    /// <summary>
    /// maxSpeed'e ne kadar yakın olduğunu sıfır ile bir arasında bir sayı ile temsil eder.
    /// </summary>
    private float speedFactor;
    private float currentMotorTorque, currentSteerRange;
    private bool _isControllable = true;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();
    }

    private void OnEnable()
    {
        EventBus.Subscribe(EventType.CarContactObstacle, MakeUncontrollable);
        EventBus.Subscribe(EventType.ParkingSuccessful, MakeUncontrollable);
        YG2.onRewardAdv += MakeControllable;
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(EventType.CarContactObstacle, MakeUncontrollable);
        EventBus.Unsubscribe(EventType.ParkingSuccessful, MakeUncontrollable);
        YG2.onRewardAdv -= MakeControllable;
    }

    void FixedUpdate()
    {
        if (_isControllable)
        {
            InputControl();

            // Arabanın ileri yönüne göre mevcut hızı hesapla (geri doğru giderken ederken negatif bir sayı döndürür)
            forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);

            DirectionControl();

            CalculateSpeedFactor();

            CalculateCurrentMotorTorque();

            CalculateCurrentSteerRange();

            // İvlemenmeye karar vermek için kullanıcı girişinin aracın hızıyla aynı yönde olup olmadığını kontrol edin.
            bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);
            if (vInput == 0f)
            {
                isAccelerating = false;
            }

            // Space tuşuna basıldığında fren yapma
            bool isBraking = Input.GetKey(KeyCode.Space) || SimpleInput.GetButton("Brake");

            foreach (var wheel in wheels)
            {
                // Apply steering to Wheel colliders that have "Steerable" enabled
                if (wheel.steerable)
                {
                    wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
                }
                // Frene basıldıysa
                if (isBraking)
                {
                    // Eğer Space tuşuna basıldıysa fren torkunu uygula
                    wheel.WheelCollider.brakeTorque = brakeTorque;
                    wheel.WheelCollider.motorTorque = 0;
                }
                // İvlemenme gerçekleşmeliyse
                else if (isAccelerating)
                {
                    // Apply torque to Wheel colliders that have "Motorized" enabled
                    if (wheel.motorized)
                    {
                        wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                    }
                    wheel.WheelCollider.brakeTorque = 0;
                }
                // Yavaşlama(Reverse) gerçekleşmeliyse
                else if (Mathf.Abs(vInput) > 0f)
                {
                    // If the user is trying to go in the opposite direction
                    // apply brakes to all wheels
                    wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * reverseBrakeTorque;
                    wheel.WheelCollider.motorTorque = 0;
                }

                else
                {
                    wheel.WheelCollider.brakeTorque = CalculateCoastBrakeTorque();
                    wheel.WheelCollider.motorTorque = 0;
                }
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.WheelCollider.brakeTorque = brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }

        }

    }

    private void OnCollisionEnter(Collision other)
    {
        ObstacleContactControl(other.transform);
    }

    private void InputControl()
    {
        vInput = SimpleInput.GetAxis("Vertical");
        hInput = SimpleInput.GetAxis("Horizontal");
    }

    /// <summary>
    /// Arabanın ileri mi geri mi gittiğini kontrol eder ve bu durumu event ile publish eder.
    /// </summary>
    private void DirectionControl()
    {
        if (forwardSpeed < -1f && isGoingForward)
        {
            isGoingForward = false;
            EventBus.Publish(EventType.CarDirectionChanged);
        }
        else if (forwardSpeed >= 1f && !isGoingForward)
        {
            isGoingForward = true;
            EventBus.Publish(EventType.CarDirectionChanged);
        }
    }

    /// <summary>
    /// maxSpeed'e ne kadar yakın olduğunu sıfırdan bire kadar bir sayı olarak hesaplar ve speedFactor'e atar
    /// </summary>
    private void CalculateSpeedFactor()
    {
        speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
    }
    /// <summary>
    /// maxSpeed'e ne kadar yakın olduğumuza göre tork ataması yapılır. maxSpeed'e ulaştıysak tork 0 olarak atanır.
    /// </summary>
    private void CalculateCurrentMotorTorque()
    {
        currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
    }

    /// <summary>
    /// maxSpeed'e ne kadar yakın olduğumuza göre direksiyon çevirme oranını atarız. 
    /// Araba en yüksek hızda daha yumuşak bir şekilde direksiyon çeviriyor. (steeringRangeAtMaxSpeed'in oranından dolayı)
    /// </summary>
    private void CalculateCurrentSteerRange()
    {
        currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);
    }

    /// <summary>
    /// Bariyere değmeyi sayar. Son hak da biterse CarContactObstacle olayını yayınlar (kaybetme).
    /// Aynı temasın birden fazla sayılmasını önlemek için iki çarpma arasında kısa bir bekleme vardır.
    /// </summary>
    /// <returns>Çarpma sayıldıysa true.</returns>
    private bool RegisterObstacleHit()
    {
        // Araç zaten kilitliyse (kaybetti ya da park etti) yeni çarpma sayılmaz
        if (!_isControllable)
            return false;

        if (Time.time - _lastObstacleHitTime < obstacleHitCooldown)
            return false;

        _lastObstacleHitTime = Time.time;
        _obstacleHitCount++;

        if (_obstacleHitCount >= maxObstacleHits)
            EventBus.Publish(EventType.CarContactObstacle);

        return true;
    }

    private void ObstacleContactControl(Transform objectTransform)
    {
        if (objectTransform.CompareTag("Obstacle"))
        {
            // Çarpma sayılmadıysa (bekleme süresi, kontrol kapalı) vurgu da yapma
            if (!RegisterObstacleHit())
                return;
            var objectRenderers = objectTransform.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in objectRenderers)
            {
                StartCoroutine(HighlightObstacle(renderer, renderer.material.color));
            }
        }
    }

    /// <summary>
    /// Highlight işlemini gerçekleştiren coroutine
    /// </summary>
    private IEnumerator HighlightObstacle(MeshRenderer meshRenderer, Color defaultColor, int repeatCount = 3, float highlightDuration = 0.5f)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(highlightDuration);

            meshRenderer.material.color = defaultColor;
            yield return new WaitForSeconds(highlightDuration);
        }
    }

    /// <summary>
    /// Gaz bırakıldığında uygulanacak yavaşlatma torkunu hesaplar.
    /// Hız arttıkça tork artar (motor freni), çok düşük hızda ise araç yavaşça tamamen durur.
    /// </summary>
    private float CalculateCoastBrakeTorque()
    {
        float absSpeed = Mathf.Abs(forwardSpeed);
        float coast = Mathf.Lerp(coastBrakeTorqueMin, coastBrakeTorqueMax, Mathf.Clamp01(absSpeed / maxSpeed));

        // Düşük hızda araç sürünmesin diye durma torkuna doğru yumuşak geçiş yap
        float stopBlend = 1f - Mathf.Clamp01(absSpeed / stopHoldSpeed);
        return Mathf.Lerp(coast, stopHoldTorque, stopBlend);
    }

    private void MakeUncontrollable()
    {
        _isControllable = false;
    }

    private void MakeControllable(string id)
    {
        // Reklam izleyen oyuncuya yeniden tam hak ver
        _obstacleHitCount = 0;
        _isControllable = true;
    }

}