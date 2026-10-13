using TMPro;
using UnityEngine;

namespace ParkingMastery.UI
{
    /// <summary>
    /// Tüm UI ekranlarının ortak görsel kimliği: renk paleti, fontlar, yazı boyutları,
    /// 9-slice sprite'lar ve ikon seti. Ekranlar renk/font/sprite değerlerini doğrudan yazmaz, buradan okur.
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Parking Mastery/UI Theme")]
    public sealed class UITheme : ScriptableObject
    {
        [Header("Palet")]
        [SerializeField] private Color background = new Color32(0x14, 0x20, 0x2E, 0xFF);
        [SerializeField] private Color surface = new Color32(0x1F, 0x30, 0x45, 0xFF);
        [SerializeField] private Color surfaceAlt = new Color32(0x2A, 0x40, 0x5C, 0xFF);
        [SerializeField] private Color primary = new Color32(0xFF, 0xB7, 0x03, 0xFF);
        [SerializeField] private Color primaryText = new Color32(0x14, 0x20, 0x2E, 0xFF);
        [SerializeField] private Color secondary = new Color32(0x3A, 0x86, 0xFF, 0xFF);
        [SerializeField] private Color success = new Color32(0x3D, 0xDC, 0x84, 0xFF);
        [SerializeField] private Color danger = new Color32(0xFF, 0x5A, 0x5F, 0xFF);
        [SerializeField] private Color textPrimary = new Color32(0xF4, 0xF7, 0xFA, 0xFF);
        [SerializeField] private Color textMuted = new Color32(0xA8, 0xB5, 0xC4, 0xFF);
        [SerializeField] private Color starOn = new Color32(0xFF, 0xD1, 0x66, 0xFF);
        [SerializeField] private Color starOff = new Color32(0x4A, 0x5B, 0x70, 0xFF);
        [SerializeField] private Color locked = new Color32(0x5B, 0x6B, 0x7B, 0xFF);
        [SerializeField] private Color overlay = new Color32(0x0B, 0x12, 0x1B, 0xB3);

        [Header("Font (Latin + Türkçe + Kiril)")]
        [Tooltip("Mevcut uGUI Text bileşenleri için (TMP geçişi E2'de).")]
        [SerializeField] private Font legacyFont;
        [SerializeField] private TMP_FontAsset tmpFont;

        [Header("Yazı boyutları (1920x1080 referans)")]
        [SerializeField, Min(1)] private int titleSize = 72;
        [SerializeField, Min(1)] private int headingSize = 48;
        [SerializeField, Min(1)] private int bodySize = 32;
        [SerializeField, Min(1)] private int smallSize = 24;

        [Header("9-slice sprite'lar (beyaz, Image.color ile boyanır)")]
        [SerializeField] private Sprite buttonSprite;
        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Sprite pillSprite;
        [SerializeField] private Sprite circleSprite;

        [Header("İkonlar (beyaz/tek renk, boyanabilir)")]
        [SerializeField] private Sprite iconPlay;
        [SerializeField] private Sprite iconPause;
        [SerializeField] private Sprite iconRestart;
        [SerializeField] private Sprite iconHome;
        [SerializeField] private Sprite iconSettings;
        [SerializeField] private Sprite iconNext;
        [SerializeField] private Sprite iconSkip;
        [SerializeField] private Sprite iconLock;
        [SerializeField] private Sprite iconCheck;
        [SerializeField] private Sprite iconClose;
        [SerializeField] private Sprite iconMusicOn;
        [SerializeField] private Sprite iconMusicOff;
        [SerializeField] private Sprite iconStarOn;
        [SerializeField] private Sprite iconStarOff;
        [SerializeField] private Sprite iconTrophy;
        [SerializeField] private Sprite iconAd;

        public Color Background => background;
        public Color Surface => surface;
        public Color SurfaceAlt => surfaceAlt;
        public Color Primary => primary;
        public Color PrimaryText => primaryText;
        public Color Secondary => secondary;
        public Color Success => success;
        public Color Danger => danger;
        public Color TextPrimary => textPrimary;
        public Color TextMuted => textMuted;
        public Color StarOn => starOn;
        public Color StarOff => starOff;
        public Color Locked => locked;
        public Color Overlay => overlay;

        public Font LegacyFont => legacyFont;
        public TMP_FontAsset TmpFont => tmpFont;

        public int TitleSize => titleSize;
        public int HeadingSize => headingSize;
        public int BodySize => bodySize;
        public int SmallSize => smallSize;

        public Sprite ButtonSprite => buttonSprite;
        public Sprite PanelSprite => panelSprite;
        public Sprite PillSprite => pillSprite;
        public Sprite CircleSprite => circleSprite;

        public Sprite IconPlay => iconPlay;
        public Sprite IconPause => iconPause;
        public Sprite IconRestart => iconRestart;
        public Sprite IconHome => iconHome;
        public Sprite IconSettings => iconSettings;
        public Sprite IconNext => iconNext;
        public Sprite IconSkip => iconSkip;
        public Sprite IconLock => iconLock;
        public Sprite IconCheck => iconCheck;
        public Sprite IconClose => iconClose;
        public Sprite IconMusicOn => iconMusicOn;
        public Sprite IconMusicOff => iconMusicOff;
        public Sprite IconStarOn => iconStarOn;
        public Sprite IconStarOff => iconStarOff;
        public Sprite IconTrophy => iconTrophy;
        public Sprite IconAd => iconAd;
    }
}
