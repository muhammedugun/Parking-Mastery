using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkingMastery.Services;
using ParkingMastery.Core;

namespace ParkingMastery.Car
{
    /// <summary>Aracın sürüşünü yönetir: girdiyi (ICarInput) okuyup tekerleklere motor, fren ve direksiyon uygular; park başarısından sonra ve ödüllü reklamla yeniden kontrol edilebilirliği denetler.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarControl : MonoBehaviour
    {
        /// <summary>Araç ileri mi gidiyor (yön değişimi CarDirectionChanged olayıyla da bildirilir).</summary>
        public bool IsGoingForward { get; private set; } = true;
        /// <summary>Son okunan gaz girişi (-1..1).</summary>
        public float VInput { get; private set; }
        /// <summary>Son okunan direksiyon girişi (-1..1).</summary>
        public float HInput { get; private set; }
        /// <summary>El freni ya da hareket yönünün tersine basılıyor (kontrol dışıyken de true: park/kayıp sonrası araç tutulur). Fren lambalarını sürer.</summary>
        public bool IsBraking { get; private set; }
        /// <summary>Geri vites devrede (gaz geri yönde ve fren yok). Geri vites lambalarını sürer.</summary>
        public bool IsReversing { get; private set; }

        // Sürüş girdisi kaynağı (AUDIT CC-5). Varsayılan: klavye + SimpleInput; SetInput ile değiştirilebilir.
        private ICarInput _input = new SimpleInputCarInput();

        /// <summary>Girdi kaynağını değiştirir (test, replay, farklı kontrol şeması). null verilirse varsayılana döner.</summary>
        public void SetInput(ICarInput input)
        {
            _input = input ?? new SimpleInputCarInput();
        }

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

        [Header("El freni (Space)")]
        [Tooltip("El freninde arka (direksiyonsuz) tekerleklere uygulanan fren torku.")]
        [SerializeField] private float handbrakeRearTorque = 6000;
        [Tooltip("El freninde ön tekerleklere uygulanan fren torku (ön kilitlenip direksiyonu öldürmesin diye arkadan düşük).")]
        [SerializeField] private float handbrakeFrontTorque = 2000;
        [Tooltip("El freni basılıyken arka tekerlek yanal tutunması çarpanı (1 = değişmez; <1 = arka kayar, araç daha kolay döner).")]
        [SerializeField, Range(0.3f, 1f)] private float handbrakeRearGrip = 0.75f;
        [Tooltip("Arka tutunmanın düşme/geri dönme hızı (1/sn).")]
        [SerializeField, Min(0.1f)] private float handbrakeGripBlendSpeed = 6f;

        [Header("Kurtarma (devrilme / takılma)")]
        [Tooltip("Araç bu süre (sn) boyunca devrik kalırsa otomatik ayağa kaldırılır. 0 = otomatik kurtarma kapalı. Oyuncu ayrıca ResetCar() ile (R tuşu / buton) elle sıfırlayabilir.")]
        [SerializeField, Min(0f)] private float flipRecoveryDelay = 2f;
        [Tooltip("transform.up.y bu değerin altındaysa araç devrik sayılır (1 = dik, 0 = yan, -1 = ters).")]
        [SerializeField, Range(-1f, 0.9f)] private float flippedUpThreshold = 0.3f;
        [Tooltip("Kurtarmada aracın bulunduğu yerden ne kadar yukarı (m) bırakılacağı; zemine gömülmeyi önler.")]
        [SerializeField, Min(0f)] private float recoveryLift = 0.5f;

        private float _flippedTime;

        [Header("Bariyer hakkı")]
        [Tooltip("Seviye kaybedilmeden önce bariyerlere kaç kez değmeye izin verilir. Bu sayıya ulaşınca seviye kaybedilir.")]
        [SerializeField, Min(1)] private int maxObstacleHits = 3;
        [Tooltip("İki sayılan çarpma arasındaki en az süre (sn). Aynı temasın çok kez sayılmasını önler.")]
        [SerializeField] private float obstacleHitCooldown = 1f;
        [Tooltip("Bu çarpma hızında (m/s) çarpma tepkisi (ses, sarsıntı, ekran uyarısı) tam şiddete çıkar; daha yavaş temas orantılı hafif tepki verir.")]
        [SerializeField, Min(0.1f)] private float hitFullIntensitySpeed = 6f;

        private int _obstacleHitCount;
        private float _lastObstacleHitTime = -10f;

        [SerializeField] private float maxSpeed = 10;
        [Tooltip("Geri vites azami hızı, maxSpeed'in oranı olarak (0.4 = %40).")]
        [SerializeField, Range(0.1f, 1f)] private float reverseMaxSpeedRatio = 0.4f;
        [Tooltip("Direksiyon girişinin saniyede ne kadar hızlı hedefe varacağı (1 = tam kilit ~1 sn). Dokunmatik/eğim girişinde ani sıçramayı önler.")]
        [SerializeField, Min(0.1f)] private float steerResponse = 5f;
        [Tooltip("Direksiyon girişi merkeze dönerken ya da karşı yöne geçerken hızı (kilide giderkenkinden hızlı; araç hızla düzelir).")]
        [SerializeField, Min(0.1f)] private float steerReturnResponse = 8f;
        [Tooltip("Hıza bağlı direksiyon açısı eğrisinin üssü: 1 = doğrusal; <1 = hız arttıkça açı daha erken daralır; >1 = geniş açı daha uzun korunur.")]
        [SerializeField, Range(0.3f, 2f)] private float steerSpeedExponent = 0.75f;
        [Tooltip("Bu hızın (m/s) altında araç durağan sayılır; yön kararını giriş verir (durağan hâlden W/S ile kalkış).")]
        [SerializeField, Min(0f)] private float directionDeadZone = 0.5f;
        [SerializeField] private float steeringRange = 30;
        [SerializeField] private float steeringRangeAtMaxSpeed = 10;
        [SerializeField] private float centreOfGravityOffset = -1f;

        public float MaxSpeed => maxSpeed;
        public float ReverseMaxSpeed => maxSpeed * reverseMaxSpeedRatio;
        /// <summary>Aracın anlık hızı (m/s, yönden bağımsız). Kamera hıza bağlı zoom için okur.</summary>
        public float CurrentSpeed => rigidBody != null ? rigidBody.linearVelocity.magnitude : 0f;
        /// <summary>Kaybetmeden önce kalan bariyer hakkı.</summary>
        public int RemainingHits => Mathf.Max(0, maxObstacleHits - _obstacleHitCount);

        /// <summary>LevelLoader: seviyenin bariyer/engel çarpma hakkını ayarlar (LevelData.MaxObstacleHits).</summary>
        public void SetMaxObstacleHits(int hits) { maxObstacleHits = Mathf.Max(1, hits); }

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
        

        private const string ObstacleTag = "Obstacle";
        /// <summary>En hafif sayılan çarpmanın tepki şiddeti (yavaş temas da fark edilsin).</summary>
        private const float MinHitIntensity = 0.25f;
        private const int HighlightRepeatCount = 3;
        private static readonly WaitForSeconds HighlightWait = new WaitForSeconds(0.5f);
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        /// <summary>Engel vurgusu için ortak blok: materyal kopyası üretmez (CC-4).</summary>
        private MaterialPropertyBlock _highlightBlock;
        /// <summary>Renderer başına tek aktif vurgu: art arda çarpmada eskisi durdurulup yenisi başlar, renk kırmızıda kalmaz.</summary>
        private readonly Dictionary<Renderer, Coroutine> _activeHighlights = new Dictionary<Renderer, Coroutine>();
        private float _smoothedSteer;
        private float[] _baseSideStiffness;
        /// <summary>Arka tekerlek yanal tutunma çarpanı (1 = normal; el freninde handbrakeRearGrip'e iner).</summary>
        private float _rearGrip = 1f;

        /// <summary>Ödüllü reklam kimliği: yalnızca bu kimlikle gelen ödül aracı yeniden kontrol edilebilir yapar (bkz. AdManager).</summary>
        private const string ResumeRewardId = AdRewardIds.Resume;

        /// <summary>Park başarıyla tamamlandıysa true; bundan sonra hiçbir reklam ödülü aracı yeniden açmaz.</summary>
        private bool _isLevelCompleted;

        void Start()
        {
            if (!TryGetComponent(out rigidBody))
            {
                Debug.LogError("CarControl: Rigidbody bulunamadı; araç kontrolü devre dışı bırakıldı.", this);
                enabled = false;
                return;
            }

            // Adjust center of mass vertically, to help prevent the car from rolling
            // Önce otomatik ağırlık merkezine dön, sonra ofseti uygula (tekrar çağrılsa da birikmez)
            rigidBody.ResetCenterOfMass();
            rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

            // Find all child GameObjects that have the WheelControl script attached
            wheels = GetComponentsInChildren<WheelControl>();

            // El freninde arka tutunmayı düşürüp geri alabilmek için tekerleklerin başlangıç yanal sertliği saklanır
            _baseSideStiffness = new float[wheels.Length];
            for (int i = 0; i < wheels.Length; i++)
                _baseSideStiffness[i] = wheels[i].WheelCollider.sidewaysFriction.stiffness;
        }

        private void OnEnable()
        {
            EventBus.Subscribe(GameEventType.LevelFailed, MakeUncontrollable);
            EventBus.Subscribe(GameEventType.ParkingSuccessful, OnParkingSuccessful);
            GameServices.Ads.RewardGranted += MakeControllable;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe(GameEventType.LevelFailed, MakeUncontrollable);
            EventBus.Unsubscribe(GameEventType.ParkingSuccessful, OnParkingSuccessful);
            GameServices.Ads.RewardGranted -= MakeControllable;

            // Coroutine'ler devre dışı kalınca durur; yarım kalan vurguları geri al (engel kırmızı kalmasın)
            ClearAllHighlights();
        }

        void Update()
        {
            // Tuşa basılma anı FixedUpdate'te kaçabilir; bu yüzden Update'te okunur.
            if (_input.ResetPressed)
                ResetCar();
        }

        void FixedUpdate()
        {
            UpdateRearGrip(_isControllable && _input.Brake);

            if (_isControllable)
            {
                RecoverIfFlipped();

                InputControl();

                // Arabanın ileri yönüne göre mevcut hızı hesapla (geri doğru giderken ederken negatif bir sayı döndürür)
                forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);

                DirectionControl();

                CalculateSpeedFactor();

                CalculateCurrentMotorTorque();

                CalculateCurrentSteerRange();

                // Yön kararı: durağan araçta (ölü bölge) giriş yönü belirler, hareket halindeyken girişin hız yönüyle aynı olup olmadığına bakılır (CC-1)
                bool isAccelerating = VInput != 0f &&
                    (Mathf.Abs(forwardSpeed) < directionDeadZone || Mathf.Sign(VInput) == Mathf.Sign(forwardSpeed));

                // El freni girişi (Space / fren butonu, ICarInput üzerinden)
                bool isBraking = _input.Brake;

                // Direksiyon girişi hedefe doğru yumuşatılır (CC-10)
                _smoothedSteer = Mathf.MoveTowards(_smoothedSteer, HInput, SteerRate(HInput, _smoothedSteer) * Time.fixedDeltaTime);
                float steerAngle = _smoothedSteer * currentSteerRange;

                // Tork kararı bir kez verilir, aynı değerler tüm tekerleklere uygulanır (CC-9)
                float wheelBrakeTorque;
                float wheelMotorTorque;
                if (isBraking)
                {
                    // El freni (Space / fren butonu): ön tekerleklerde hafif, arka tekerleklerde güçlü fren (tekerlek başına aşağıda ayrılır)
                    wheelBrakeTorque = handbrakeFrontTorque;
                    wheelMotorTorque = 0f;
                }
                else if (isAccelerating)
                {
                    // Hızlanma (ileri ya da geri vites)
                    wheelBrakeTorque = 0f;
                    wheelMotorTorque = VInput * currentMotorTorque;
                }
                else if (Mathf.Abs(VInput) > 0f)
                {
                    // Kullanıcı hareket yönünün tersine basıyor: fren
                    wheelBrakeTorque = Mathf.Abs(VInput) * reverseBrakeTorque;
                    wheelMotorTorque = 0f;
                }
                else
                {
                    // Gaz bırakıldı: motor freni / durma tutma
                    wheelBrakeTorque = CalculateCoastBrakeTorque();
                    wheelMotorTorque = 0f;
                }
                // Dışarıya açılan fren / geri vites durumu (fren ve geri vites lambaları için)
                IsBraking = isBraking || (!isAccelerating && Mathf.Abs(VInput) > 0f);
                IsReversing = !isBraking && isAccelerating && VInput < 0f;


                foreach (var wheel in wheels)
                {
                    // Direksiyon yalnızca "Steerable" tekerleklere, motor torku yalnızca "Motorized" tekerleklere uygulanır
                    if (wheel.Steerable)
                        wheel.WheelCollider.steerAngle = steerAngle;

                    // El freninde arka (direksiyonsuz) tekerlekler güçlü, ön tekerlekler hafif frenlenir
                    wheel.WheelCollider.brakeTorque = (isBraking && !wheel.Steerable) ? handbrakeRearTorque : wheelBrakeTorque;
                    wheel.WheelCollider.motorTorque = wheel.Motorized ? wheelMotorTorque : 0f;
                }
            }
            else
            {
                IsBraking = true;
                IsReversing = false;

                foreach (var wheel in wheels)
                {
                    wheel.WheelCollider.brakeTorque = brakeTorque;
                    wheel.WheelCollider.motorTorque = 0;
                }

            }

        }

        private void OnCollisionEnter(Collision other)
        {
            // Collision.transform Rigidbody'nin transform'unu verebilir; etiket için asıl collider'dan başlanır (CC-8)
            ObstacleContactControl(other.collider.transform, other.relativeVelocity.magnitude);
        }

        private void InputControl()
        {
            VInput = _input.Throttle;
            HInput = _input.Steer;
        }

        /// <summary>
        /// Arabanın ileri mi geri mi gittiğini kontrol eder ve bu durumu event ile publish eder.
        /// </summary>
        private void DirectionControl()
        {
            if (forwardSpeed < -1f && IsGoingForward)
            {
                IsGoingForward = false;
                EventBus.Publish(GameEventType.CarDirectionChanged);
            }
            else if (forwardSpeed >= 1f && !IsGoingForward)
            {
                IsGoingForward = true;
                EventBus.Publish(GameEventType.CarDirectionChanged);
            }
        }

        /// <summary>
        /// maxSpeed'e ne kadar yakın olduğunu sıfırdan bire kadar bir sayı olarak hesaplar ve speedFactor'e atar
        /// </summary>
        private void CalculateSpeedFactor()
        {
            // Geri giderken ayrı (daha düşük) hız sınırı kullanılır; hız mutlak değerle ölçülür
            float speedLimit = forwardSpeed >= 0f ? maxSpeed : ReverseMaxSpeed;
            speedFactor = Mathf.InverseLerp(0f, speedLimit, Mathf.Abs(forwardSpeed));
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
            // Üs < 1: hız arttıkça açı erken daralır (sabit yanal ivmeye yakın), düşük hızda park için geniş kalır
            currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, Mathf.Pow(speedFactor, steerSpeedExponent));
        }

        /// <summary>
        /// Bariyere değmeyi sayar. Son hak da biterse LevelFailed olayını yayınlar (kaybetme).
        /// Aynı temasın birden fazla sayılmasını önlemek için iki çarpma arasında kısa bir bekleme vardır.
        /// </summary>
        /// <returns>Çarpma sayıldıysa true.</returns>
        private bool RegisterObstacleHit(float impactSpeed)
        {
            // Araç zaten kilitliyse (kaybetti ya da park etti) yeni çarpma sayılmaz
            if (!_isControllable)
                return false;

            if (Time.time - _lastObstacleHitTime < obstacleHitCooldown)
                return false;

            _lastObstacleHitTime = Time.time;
            _obstacleHitCount++;

            // Her sayılan çarpmada kalan hakla birlikte bildir (CC-7, B5: tipli olay); UI/ses ObstacleHitEvent'e abone olur
            float intensity = Mathf.Lerp(MinHitIntensity, 1f, Mathf.Clamp01(impactSpeed / Mathf.Max(0.1f, hitFullIntensitySpeed)));
            EventBus.Publish(new ObstacleHitEvent(RemainingHits, intensity));

            // Son hak da bittiyse seviye kaybedilir
            if (_obstacleHitCount >= maxObstacleHits)
                EventBus.Publish(GameEventType.LevelFailed);

            return true;
        }

        private void ObstacleContactControl(Transform contactTransform, float impactSpeed)
        {
            // Etiket collider'da değil üst objedeyse de çarpma kaçmasın (CC-8): collider'dan yukarı doğru ilk 'Obstacle' etiketini bul
            Transform obstacle = FindObstacle(contactTransform);
            if (obstacle == null)
                return;

            // Çarpma sayılmadıysa (bekleme süresi, kontrol kapalı) vurgu da yapma
            if (!RegisterObstacleHit(impactSpeed))
                return;

            foreach (var meshRenderer in obstacle.GetComponentsInChildren<MeshRenderer>())
            {
                StartHighlight(meshRenderer);
            }
        }

        /// <summary>Verilen transform'dan başlayıp kökü doğru 'Obstacle' etiketli ilk objeyi döndürür; yoksa null.</summary>
        private static Transform FindObstacle(Transform start)
        {
            for (Transform current = start; current != null; current = current.parent)
            {
                if (current.CompareTag(ObstacleTag))
                    return current;
            }
            return null;
        }

        /// <summary>
        /// Highlight işlemini gerçekleştiren coroutine
        /// </summary>
        private IEnumerator HighlightObstacle(Renderer meshRenderer, int colorId)
        {
            if (_highlightBlock == null)
                _highlightBlock = new MaterialPropertyBlock();

            for (int i = 0; i < HighlightRepeatCount; i++)
            {
                if (meshRenderer == null)
                    yield break;

                // Materyal kopyalamadan yalnız renk özelliği ezilir (CC-4)
                _highlightBlock.Clear();
                _highlightBlock.SetColor(colorId, Color.red);
                meshRenderer.SetPropertyBlock(_highlightBlock);
                yield return HighlightWait;

                if (meshRenderer == null)
                    yield break;

                // Bloğu kaldırmak orijinal materyal görünümünü aynen geri getirir
                meshRenderer.SetPropertyBlock(null);
                yield return HighlightWait;
            }

            _activeHighlights.Remove(meshRenderer);
        }

        /// <summary>Renderer için vurguyu başlatır; zaten süren bir vurgu varsa önce durdurup temizler.</summary>
        private void StartHighlight(Renderer meshRenderer)
        {
            Material shared = meshRenderer.sharedMaterial;
            if (shared == null)
                return;

            // Standard/Built-in: _Color, URP tarzı shader'lar: _BaseColor. İkisi de yoksa vurgu yapılamaz.
            int colorId;
            if (shared.HasProperty(ColorId))
                colorId = ColorId;
            else if (shared.HasProperty(BaseColorId))
                colorId = BaseColorId;
            else
                return;

            if (_activeHighlights.TryGetValue(meshRenderer, out Coroutine running))
            {
                if (running != null)
                    StopCoroutine(running);
                meshRenderer.SetPropertyBlock(null);
            }

            _activeHighlights[meshRenderer] = StartCoroutine(HighlightObstacle(meshRenderer, colorId));
        }

        /// <summary>Süren tüm vurguları temizler ve renderer'ları orijinal haline döndürür.</summary>
        private void ClearAllHighlights()
        {
            foreach (var pair in _activeHighlights)
            {
                if (pair.Key != null)
                    pair.Key.SetPropertyBlock(null);
            }
            _activeHighlights.Clear();
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

        /// <summary>
        /// Aracı bulunduğu yerde, aynı yöne bakacak şekilde ayağa kaldırır ve hızını sıfırlar.
        /// Bariyer hakkı sıfırlanmaz (ceza korunur). Kontrol kilitliyken (seviye kaybedildi/kazanıldı) çalışmaz.
        /// R tuşundan, mobil butondan veya devrilme algılanınca çağrılır.
        /// </summary>
        public void ResetCar()
        {
            if (!_isControllable || rigidBody == null)
                return;

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            Quaternion upright = forward.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(forward.normalized, Vector3.up)
                : Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            Vector3 position = transform.position + Vector3.up * recoveryLift;

            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.position = position;
            rigidBody.rotation = upright;
            transform.SetPositionAndRotation(position, upright);

            // Teker torkları sıfırlanır; bir sonraki FixedUpdate normal hesabı yapar
            if (wheels != null)
            {
                foreach (var wheel in wheels)
                {
                    wheel.WheelCollider.motorTorque = 0f;
                    wheel.WheelCollider.brakeTorque = brakeTorque;
                }
            }

            _flippedTime = 0f;
            _smoothedSteer = 0f;
        }

        /// <summary>Araç flipRecoveryDelay süresince devrik kalırsa otomatik ayağa kaldırır.</summary>
        private void RecoverIfFlipped()
        {
            if (flipRecoveryDelay <= 0f)
                return;

            if (transform.up.y < flippedUpThreshold)
            {
                _flippedTime += Time.fixedDeltaTime;
                if (_flippedTime >= flipRecoveryDelay)
                    ResetCar();
            }
            else
            {
                _flippedTime = 0f;
            }
        }

        /// <summary>Direksiyon girişi kilide doğru artarken yavaş (steerResponse), merkeze dönerken ya da karşı yöne geçerken hızlı (steerReturnResponse) yaklaşır; araç hızla düzelir, kilide yumuşak girer.</summary>
        private float SteerRate(float target, float current)
        {
            bool towardsLock = Mathf.Abs(target) > Mathf.Abs(current) && (current == 0f || Mathf.Sign(target) == Mathf.Sign(current));
            return towardsLock ? steerResponse : steerReturnResponse;
        }

        /// <summary>El freni basılıyken arka tekerleklerin yanal tutunmasını yumuşakça düşürür (bırakılınca normale döner); araç daha kolay döner/kayar.</summary>
        private void UpdateRearGrip(bool handbrake)
        {
            if (wheels == null || _baseSideStiffness == null)
                return;

            float target = handbrake ? handbrakeRearGrip : 1f;
            float next = Mathf.MoveTowards(_rearGrip, target, handbrakeGripBlendSpeed * Time.fixedDeltaTime);
            if (Mathf.Approximately(next, _rearGrip))
                return;
            _rearGrip = next;

            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].Steerable)
                    continue;

                WheelFrictionCurve friction = wheels[i].WheelCollider.sidewaysFriction;
                friction.stiffness = _baseSideStiffness[i] * _rearGrip;
                wheels[i].WheelCollider.sidewaysFriction = friction;
            }
        }

        private void MakeUncontrollable()
        {
            _isControllable = false;
        }

        private void OnParkingSuccessful()
        {
            // Seviye kazanıldı: kilit kalıcıdır, reklam ödülü bunu açamaz
            _isLevelCompleted = true;
            MakeUncontrollable();
        }

        private void MakeControllable(string id)
        {
            // Yalnızca 'resume' ödülü devam hakkı verir; başka bir ödül kimliği ya da kazanılmış seviye aracı açamaz
            if (id != ResumeRewardId || _isLevelCompleted)
                return;

            // Reklam izleyen oyuncuya yeniden tam hak ver
            _obstacleHitCount = 0;
            _isControllable = true;
        }

    }
}
