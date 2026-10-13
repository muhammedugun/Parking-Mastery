# Parking Mastery: Teknik Yapı ve Yandex Games Kuralları

*Bu belge oyunun nasıl kodlandığını, hangi sınırlar içinde yayınlanacağını ve varlıkların (model, ses, texture) nereden geleceğini anlatır. Oyun tasarımı `01_oyun_tasarimi.md` içindedir, yapılacaklar `PROJE_DURUMU.md` içindedir.*

## 1. Mimari [KARAR]

### 1.1 Klasör yapısı (`Assets/_Project/`)

```
Art/        Models, Materials, Textures, Shaders, VFX
Audio/      Music, SFX, Mixer
Data/       LevelData ve ayar ScriptableObject'leri
Prefabs/    Car, Environment, Obstacles, UI, Systems
Scenes/     Boot, MainMenu, LevelSelect, Gameplay
Scripts/    Core, Gameplay, Car, UI, Audio, Services, Editor, Platform/Yandex
```

Proje belgeleri `Assets/Documents` altındadır.

### 1.2 Sahneler ve açılış

- Oyunda yalnız birkaç sahne vardır: **Boot** (build index 0), **MainMenu**, **LevelSelect**, **Gameplay**. Level içeriği sahnede değil `LevelData` (ScriptableObject) + prefab düzeninde durur, `LevelLoader` Gameplay sahnesine yükler.
- **Boot** sahnesi `Systems` prefab'ını (kalıcı, tek `SystemsRoot`: müzik yöneticisi, sahne yükleyici) ve `GameBootstrap`'i barındırır. `GameBootstrap` servisleri başlatır, kayıt hazır olana kadar en fazla 5 sn bekler, sonra `MainMenu`'yü yükler.
- Editörde menü `Parking-Mastery ▸ Play From Boot` Play'i her zaman Boot'tan başlatır.

### 1.3 Kod katmanları

- 6 runtime asmdef + 1 Editor asmdef. Bağımlılık yönü (döngü yok): `Core` ← `Services` ← `Car`, `Audio`, `Gameplay` ← `UI`. `Editor` → `Gameplay`.
- Yandex eklentisine bağlı kod yalnız `Scripts/Platform/Yandex/` içindedir. Oyun kodu Yandex'i doğrudan bilmez, `GameServices` üzerinden konuşur.
- Servisler: `ISaveService` (Yandex kaydı + yerel yedek, sürümlü), `IAdService`, `ILocalizationService`, `IAudioService`.
- Oyun akışı: `LevelController` (durum makinesi), `GameState` (Loading/Playing/Paused/Won/Failed), tipli `EventBus`, `ParkingZone` (hiza/açı/bekleme), `LevelScoring` (LevelData'dan yıldız).
- UI: `UIManager` (ekranları tipe göre açar/kapatır, geri tuşu), `ScreenBase` (fade/scale), `UIButtonFeedback`, `UISfx`, `UITheme`.

### 1.4 Kod kuralları

- Değerler koda gömülmez, ScriptableObject veya Inspector'de durur. Sahne adı, ödül kimliği gibi string'ler sabit olarak tanımlanır.
- Oyun mantığı tek yerde yazılır (ör. yıldız hesabı, bekleme süresi). Aynı kural iki yerde kopyalanmaz.
- Olaylar tipli olur. Statik olay listeleri Play'ler arası temizlenir.
- `Update` içinde `Find...` çağrısı ve gereksiz bellek ayırma (GC) olmaz, efekt ve sesler havuzdan gelir.
- WebGL'de `Application.targetFrameRate` zorlanmaz.
- Ses, reklam ve sekme kesintilerinde oyunu durdurma tek bir `PauseService` üzerinden yapılır (sayaçlı, iç içe duraklatmayı destekler).
- Kayıt verisinde sürüm alanı bulunur, yıldız değeri 0-3 arasına sıkıştırılır, seviye sayısı tek yerde (`LevelCatalog`/`LevelData` listesi) tanımlıdır, `int.Parse` ile sahne adından level numarası okunmaz.
- Son level bitince oyuncu menüye döner, olmayan bir sonraki levele gitmez.

## 2. Yandex Games Kısıtları

*Kaynak: Yandex Games resmi gereksinimleri (numaralar resmi madde numaralarıdır). Moderasyon 3-5 iş günü sürer, testler gerçek kullanıcı koşullarını taklit eder.*

### 2.1 Boyut ve performans hedefleri

| Kalem | Hedef |
| --- | --- |
| Yandex sınırı | Toplam dosya ≤ 100 MB (sıkıştırmasız) |
| Kendi hedefimiz | Build ≤ ~40 MB, Brotli indirme ≤ ~15 MB, ses ≤ ~8 MB |
| Performans | Mobil 30 FPS, masaüstü 60 FPS, draw call ≤ ~150, level başı ≤ ~100k üçgen |
| Texture | Çoğu 512, en çok 1024, tek palet atlası |
| Render | Built-in pipeline |
| Son ölçüm | Sıkıştırmasız ~52 MB, Brotli ~13 MB. `wasm` 38,7 MB ile asıl şişkinlik (kod kırpma ve paket temizliği gerekli). Ses kaynakları (2 büyük WAV, toplam 42,7 MB) küçültülecek. |

### 2.2 Zorunlu gereksinimler

Her madde yayın öncesi test edilir (test yöntemi parantezde).

**Teknik**

- **1.1 / 1.19** SDK doğru başlatılır. `LoadingAPI.ready()` oyun ilk oynanabilir olduğu anda çağrılır. `GameplayAPI.start/stop` level başında/sonunda/pause'da çağrılır. Pause/resume olayları dinlenir. (Yandex taslak linkinde konsol logu.) Not: eklentideki otomatik ready (`autoGRA`) kapatılıp elle çağrılması önerilir.
- **1.2** Misafir olarak tam oynanır. Giriş yalnız oyuncu bilinçli butona basarsa istenir ve yalnız Yandex ID ile olur. (Girişsiz tüm levelleri oyna.)
- Reklam ve ödeme yalnız Yandex SDK üzerinden. Başka oyunlara bağlantı yok. Portal para birimi yok.
- **1.7 / 1.18** Kodda ve şablonda Yandex S3'e mutlak URL yok. Oyun hangi adreste açılırsa açılsın çalışır. (Build'de `https://` / `yandex` taraması + farklı adreste açma.)
- **1.20** Desteklenen tarayıcılar: Yandex Browser, Chrome, Firefox, Opera, Safari, Yandex mobil uygulaması. Unity 6 yalnız WebGL2 destekler, bu yüzden eski cihazlar dışarıda kalır. Taslakta yalnız gerçek cihazda doğrulanan platformlar seçilir. (Orta/düşük uç telefonda ve Safari/Firefox'ta aç-oyna.)
- **1.21** Toplam ≤ 100 MB. **1.22** `index.html` arşiv kökünde, dosya/klasör adında boşluk ve Rusça karakter yok. (Zip içeriği listesi.)
- **1.23** Çalışma anında etkileşimli yapay zeka yok.

**Kullanım**

- **1.3 / 4.7** Sekme gizlenince/odak kaybında ve reklam sırasında oyun ve ses durur. (Sekme değiştir, reklam oynat.)
- **1.6.1 Mobil:** tam ekran, uzun basışta menü veya metin seçimi yok, WebGL uyarısı yok, tamamen dokunmatik. (Gerçek telefon.)
- **1.6.2 Masaüstü:** oyun alana sığar, uzun kenar ≤ 2× kısa kenar, sağ tık menüsü yok, klavye düzeninden bağımsız, OS kısayolu yok. (Farklı pencere boyutları + AZERTY/Rusça düzen.)
- **1.9** İlerleme eylemden hemen sonra kaydedilir, yenileme veya ekran döndürmede kaybolmaz. (Level bitir, F5.)
- **1.10** Yeniden boyutlandırma ve döndürmede doğru çizim, sayfa kaymaz, pull-to-refresh yok, **tek elle oynanabilir** (ihlal = ret). (Telefonda dönüş + tek el.)
- **1.14 / 1.15** Hata, donma, çökme yok, oyun bitmiş görünür. (Tam tur, F12 konsol temiz.)
- **2.2** Kontroller oyunda veya taslaktaki "How to play" alanında anlatılır.
- **2.8 / 2.9** Zorluk kademeli, ana içerik 10 dakikadan uzun.
- **2.14** Dil `ysdk.environment.i18n.lang` ile otomatik. (Tarayıcı dilini değiştir.)

**İçerik ve mağaza**

- **3.4** Siyaset, savaş, din, büyü, gerçekçi şiddet yok.
- **3.5 / 3.6** Tüm materyalin hakkı bizde, katalogdaki oyunlardan kopya yok.
- **5.x / 8.2.2** Taslak alanları doldurulur, oyun adı benzersiz olur (free/top/best/game kelimeleri yok), ekran görüntüleri gerçek oynanıştan alınır.
- **Kitle kuralı:** Oyunun puanı (rating) 30'un üstünde olmalı. Yeni oyunda puan yaklaşık 2 hafta sonra oluşur, 3 hafta boyunca ≤ 30 kalırsa oyun yayından kalkar. Bu yüzden ilk dakika ve hatasız çalışma kritiktir.

### 2.3 WebGL şablonu

`Assets/WebGLTemplates/YandexGames/` altındaki şablon hazırdır: sayfa kaymaz, pull-to-refresh/zoom/çift dokunma/uzun basış menüsü/metin seçimi/sağ tık engelli, masaüstünde uzun kenar 2× kısa kenarla sınırlı, WebGL2 yoksa RU/EN/TR kısa mesaj gösterilir. Tarayıcı yakınlaştırması (Ctrl+tekerlek) bilerek **engellenmez** (moderasyon %80-125 yakınlaştırmayı dener). Şablona dokunulursa yukarıdaki kullanım maddeleri yeniden test edilir.

## 3. Varlık (Asset) Stratejisi

Hepsi ücretsiz, abonelik gerektirmez. **Hiçbir adım yapay zeka sağlayıcı anahtarına bağlı değildir.**

| İhtiyaç | Yöntem (öncelik sırasıyla) |
| --- | --- |
| Ses efekti | C# ile Editor'de prosedürel WAV üretimi. Olmazsa CC0 paket (Kenney, OpenGameArt). |
| Müzik | Prosedürel/jeneratif kısa döngüler. Olmazsa CC0 müzik. |
| 3D model | ProBuilder / Unity primitifleri, CC0 paket (Kenney, Quaternius) veya Blender (aşağıdaki tabloya göre). |
| Texture / materyal | Prosedürel texture, tek palet atlası, CC0 (ambientCG, Poly Haven). |

**Model yöntemi seçimi:** Her işte en uygun yöntem seçilir, seçim ve gerekçesi `PROJE_DURUMU.md`'deki ilgili adımın notuna yazılır.

| İş türü | Yöntem |
| --- | --- |
| Basit geometrik şekiller (duvar, kolon, bariyer, çit, kaldırım, rampa, tümsek, zemin, işaret) | Unity primitifleri / ProBuilder |
| Stile uyan hazır CC0 karşılığı olanlar (araba, ağaç, konteyner, çöp kutusu, lamba) | CC0 paket (poligon fazlaysa veya stile uymazsa elenir) |
| Organik/karmaşık şekiller, özgün prop, CC0 model düzenleme, çok sayıda varyant | Blender (Blender Bridge ile) |

Seçim ölçütleri sırayla: stil uyumu (temiz low-poly, tek palet) → poligon/WebGL bütçesi → emek → lisans. Blender kapalıysa hiçbir adım bloklanmaz, ProBuilder/CC0 yoluna düşülür.

### 3.1 Lisans ve telif [KARAR]

- **Kabul edilen kaynaklar:** kendi üretimimiz, CC0 veya önceden üretilmiş yapay zeka içeriği. Atıf isteyen (CC-BY), "ticari kullanım yok" veya "paylaşım-benzeri" lisanslar kabul edilmez.
- Üçüncü taraf her dosya `CREDITS.md` tablosuna satır eklenmeden projeye girmez (kaynak, lisans). Yapay zeka içeriği "YZ-önceden-üretilmiş" olarak araç adı ve tarihle yazılır.
- Araba gerçek bir marka/model kopyası olmaz. Plaka, tabela, billboard, konteyner yazıları kurgusaldır.
- Müzik ve seste kayıtlı şarkı veya film/oyun sesi örneği yok.
- Oyun içi krediler Ayarlar > Hakkında ekranında düz metin olarak gösterilir, metin `CREDITS.md`'den üretilir.
- **Oyun adı:** "Parking Mastery". "Parking Master" benzeri adlar yaygın olduğu için yayından önce Yandex Games kataloğunda ad araması yapılır. Çakışma varsa ayırt edici alt başlık veya farklı ad kullanılır (RU/EN/TR aynı kök).
- **Farklılaşma (3.6):** mobilde tek başparmak joystick, 3 tematik dünya, baştan tasarlanmış engel sistemi, level içinde reklam yok, tam RU/EN/TR. Yayından önce katalogdan en az 3 benzer park oyunu incelenir, görsel/isim/ikon/level düzeni birebir benzemez.
