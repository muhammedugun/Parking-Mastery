using UnityEngine;

namespace ParkingMastery.UI
{
    /// <summary>
    /// UI efekt sesleri için ortak 2B ses çalıcı (tek gizli AudioSource). <see cref="Volume"/> ileride
    /// Ayarlar'daki SFX sürgüsüne (E7) bağlanacak; şimdilik 1.
    /// </summary>
    public static class UISfx
    {
        private static AudioSource _source;

        /// <summary>Tüm UI efektlerinin ana ses seviyesi (0–1).</summary>
        public static float Volume { get; set; } = 1f;

        /// <summary>Klibi tek seferlik çalar; klip yoksa ya da ses kapalıysa sessizce çıkar.</summary>
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
                return;

            float level = Mathf.Clamp01(Volume) * Mathf.Clamp01(volume);
            if (level <= 0f)
                return;

            if (_source == null)
            {
                var go = new GameObject("UISfx") { hideFlags = HideFlags.HideAndDontSave };
                _source = go.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
            }

            _source.PlayOneShot(clip, level);
        }

        // Domain reload kapalıyken statik durumun bayat kalmasını önler.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            _source = null;
            Volume = 1f;
        }
    }
}
