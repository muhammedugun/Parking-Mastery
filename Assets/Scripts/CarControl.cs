using UnityEngine;

public class CarControl : MonoBehaviour
{
    [HideInInspector] public bool isGoingForward = true;

    [SerializeField] private float motorTorque = 1000;
    [SerializeField] private float brakeTorque = 4000;
    [SerializeField] private float decelerationTorque = 25;
    [SerializeField] private float maxSpeed = 10;
    [SerializeField] private float steeringRange = 30;
    [SerializeField] private float steeringRangeAtMaxSpeed = 10;
    [SerializeField] private float centreOfGravityOffset = -1f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    /// <summary>
    /// Arabanın ileri yöndeki hızı
    /// </summary>
    private float forwardSpeed;
    private float vInput, hInput;
    /// <summary>
    /// maxSpeed'e ne kadar yakın olduğunu sıfır ile bir arasında bir sayı ile temsil eder.
    /// </summary>
    private float speedFactor;
    private float currentMotorTorque, currentSteerRange;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();
    }

    void FixedUpdate()
    {
        InputControl();

        // Arabanın ileri yönüne göre mevcut hızı hesapla (geri doğru giderken ederken negatif bir sayı döndürür)
        forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);

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
        bool isBraking = Input.GetKey(KeyCode.Space);

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
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }

            else
            {
                wheel.WheelCollider.brakeTorque = decelerationTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
        }
    }

    private void InputControl()
    {
        vInput = Input.GetAxis("Vertical");
        hInput = Input.GetAxis("Horizontal");
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
        else if (forwardSpeed >= 0f && !isGoingForward)
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

}