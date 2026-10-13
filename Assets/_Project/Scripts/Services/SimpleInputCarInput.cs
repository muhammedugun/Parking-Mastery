using UnityEngine;

namespace ParkingMastery.Services
{
    /// <summary>
    /// ICarInput'un SimpleInput (klavye + ekran joystick/butonları) uygulaması.
    /// Eksen/buton adları ve Space tuşu yalnız burada bilinir.
    /// </summary>
    public sealed class SimpleInputCarInput : ICarInput
    {
        private const string VerticalAxis = "Vertical";
        private const string HorizontalAxis = "Horizontal";
        private const string BrakeButton = "Brake";

        public float Throttle => SimpleInput.GetAxis(VerticalAxis);

        public float Steer => SimpleInput.GetAxis(HorizontalAxis);

        public bool Brake => Input.GetKey(KeyCode.Space) || SimpleInput.GetButton(BrakeButton);

        public bool ResetPressed => Input.GetKeyDown(KeyCode.R);
    }
}
