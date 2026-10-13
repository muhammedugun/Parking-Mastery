namespace ParkingMastery.Services
{
    /// <summary>
    /// Araç sürüş girdisi. CarControl cihaz/tuş/eksen adlarını bilmez, yalnız bu arayüzü okur (AUDIT CC-5).
    /// Başka kaynaklar (mobil joystick, replay, test) bu arayüzü uygulayıp CarControl.SetInput ile verilebilir.
    /// </summary>
    public interface ICarInput
    {
        /// <summary>Gaz/geri girişi: -1 (geri) .. 1 (ileri).</summary>
        float Throttle { get; }

        /// <summary>Direksiyon girişi: -1 (sol) .. 1 (sağ).</summary>
        float Steer { get; }

        /// <summary>El freni (Space / fren butonu) basılı mı? Arka tekerleklere güçlü fren uygular ve arka tutunmayı düşürür; ayak freni gaz girişinin ters yönüdür.</summary>
        bool Brake { get; }

        /// <summary>Araç sıfırlama bu karede istendi mi? (tek kare doğru döner; Update'te okunmalı)</summary>
        bool ResetPressed { get; }
    }
}
