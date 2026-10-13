# Yandex Games Kısıtları ve Ölçek Kararları

> `PARKING_MASTERY_PLAN.md` içindeki Bölüm 0B'den taşındı (içerik birebir). Plan dosyasında bu bölümün kısa özeti ve Y1–Y5 adımları (0B.6) kalır.
> **Y1 adımı** bu dosyadaki 0B.2 tablosunu test edilebilir bir kontrol listesine çevirip genişletecek; tamamlanınca buraya 'Kontrol listesi' bölümü eklenir.


> Kaynak: Yandex Games dokümantasyonu (Game requirements, Quick start, Ad placement — gereksinim sayfası "Last updated: 9 Ekim 2026"). Numaralar resmi gereksinim maddeleridir (ör. 1.3, 4.7). Moderasyon 3–5 iş günü sürer; testler gerçek kullanıcı koşullarını simüle eder, kendi cihazında tekrarlanmayan hata da red nedeni olabilir.

### 0B.1 Kitle → tasarım sonuçları

| Yandex verisi | Bizim için sonuç |
|---|---|
| Rusya + BDT %60, diğer %40 | RU öncelikli dil, ardından EN/TR; Kiril destekli font şart |
| Telefon %50, bilgisayar %40, tablet/TV %10 | Mobil öncelikli: dokunmatik kontrol, büyük butonlar, düşük/orta uç Android |
| Yaş: 35–44 %49, 55+ %20, 0–24 %11 | Casual kitle: sade arayüz, okunaklı büyük yazı, adil/kademeli zorluk |
| Ortalama platform süresi 55 dk | Kısa levellerden oluşan çok sayıda level uygun (v1: 30, v2: 50 level) |
| Rating > 30 olmalı (yeni oyunda ~2 hafta sonra oluşur; 3 hafta ≤ 30 kalırsa yayından kalkar) | İlk dakika kritik: hızlı ilk kazanç, hatasız ve akıcı çalışma |

### 0B.2 Zorunlu gereksinimler → plan adımı

| Gereksinim | Ne yapacağız | Adım |
|---|---|---|
| 1.1, 1.19 SDK doğru başlatılır; `LoadingAPI.ready()` oynanabilir olduğu an; `GameplayAPI.start/stop`; pause/resume olayları | SDK akışını resmi dokümana göre doğrula | B3, J5 |
| 1.2 Girişsiz oynanır; giriş yalnız bilinçli butonla, yalnız Yandex ID | Misafir mod + isteğe bağlı giriş butonu | B4, J3, J4 |
| 1.3, 4.7 Odak kaybında ve reklamda ses/oyun durur | Odak + reklam duraklatma | J2, J6 |
| 1.6.1 Mobil: tam ekran, uzun basışta menü/seçim yok, WebGL uyarısı yok, tamamen dokunmatik | WebGL şablonu + kontrol şeması | Y3, Y4, C6 |
| 1.6.2 Masaüstü: alan sığar, uzun kenar ≤ 2× kısa kenar, klavye/fare, klavye düzeninden bağımsız, OS kısayolu yok, sağ tık menüsü yok | WebGL şablonu + girdi | Y4, C6 |
| 1.9 İlerleme eylemden hemen sonra kaydedilir; yenileme/döndürmede kaybolmaz | Anında kayıt | C4, J4 |
| 1.10 Yeniden boyutlandırma/döndürmede doğru çizim; sayfa kaydırma / pull-to-refresh yok; **tek elle oynanabilir** | UI ölçekleme + tek-el şeması | Y3, Y4, E9 |
| 1.12 Monetizasyon açık (reklam veya satın alma) | Reklam | C5, J2 |
| 1.14, 1.15 Hata/donma/çökme yok, oyun bitmiş görünümde | Tam test turları | K4, K5 |
| 1.20 Tarayıcı/OS uyumu (Android 5+, iOS 9+, taslakta seçilen ortam) | Unity 6 WebGL2 gerektirir → gerçekçi cihaz tabanını belirle, taslakta yalnız doğrulanan platformları seç, düşük uç cihazda aç-oyna testi | Y2, K4 |
| 1.7, 1.18 Yandex S3'e mutlak URL yok; oyun açıldığı URL'ye bağlı kısıtlanmaz | Kod/şablon taraması + farklı adreste açılış testi | K7 |
| 1.21 Toplam dosya ≤ 100 MB (sıkıştırmasız) | Boyut bütçesi (0B.3) | Y2, K1 |
| 1.22 `index.html` arşiv kökünde; dosya/klasör adlarında boşluk ve Rusça karakter yok | Build paketleme kontrolü | K7 |
| 1.23 Çalışma anında etkileşimli YZ yasak (önceden üretilmiş içerik serbest) | Uyumlu | — |
| 2.2 Kontroller oyunda veya taslaktaki How to play alanında anlatılır | Tutorial + taslak metni | E10, K6 |
| 2.8, 2.9 Zorluk kademeli; ana içerik > 10 dk veya tekrar sebebi | Zorluk eğrisi | H2, H14 |
| 2.14 Dil `ysdk.environment.i18n.lang` ile otomatik | Merkezi yerelleştirme | E8, J5 |
| 3.4 Siyaset/savaş/din/büyü/gerçekçi şiddet içeriği yok | İçerik politikası | Y5 |
| 3.5, 3.6 Tüm materyalin hakkı bizde; katalogdaki oyunları kopyalama | Telif + farklılaşma | Y5, 1.3 |
| 4.x Reklam kuralları | Bkz. 0B.5 | C5, J2 |
| 5.x, 8.2.2 Taslak alanları, benzersiz ad (free/top/best yok), gerçek oynanış ekran görüntüleri | Mağaza materyalleri | K6 |

### 0B.2b Test edilebilir kontrol listesi (Y1, güncellendi: 2026-10-13)

> Durum: `[ ]` bekliyor, `[x]` geçti. Test = yayın öncesi elle/araçla doğrulama. Kaynak: yandex.com/dev/games/doc/en/concepts/requirements (güncel hâl okundu).

**Teknik**
- [ ] 1.1/1.19 SDK başlatılır; `LoadingAPI.ready()` oynanabilir ilk anda çağrılır; `GameplayAPI.start/stop` doğru. *Test:* Yandex taslak test linkinde konsol logu + ready çağrı zamanı.
- [ ] 1.2 Misafir tam oynar; giriş yalnız bilinçli butonla ve **yalnız Yandex ID**. *Test:* girişsiz tüm 30 level + butonsuz otomatik giriş penceresi çıkmıyor.
- [ ] Reklam/ödeme yalnız Yandex SDK üzerinden; YAN monetizasyonu açık. *Test:* kodda başka reklam/ödeme SDK'sı yok, konsolda Monetization açık.
- [ ] Başka oyunlara bağlantı yok. *Test:* UI/metin/şablon taraması.
- [ ] 1.7/1.18 Yandex S3'e mutlak URL yok; farklı adreste açılır. *Test:* build içinde `yandex.*s3` / `https://` taraması + başka host'ta aç.
- [ ] 1.20 Tarayıcılar: Yandex Browser, Chrome, Firefox, Opera, Safari, Yandex mobil uygulama. *Test:* her birinde aç-oyna (Y2 cihaz tabanı).
- [ ] 1.21 Toplam ≤ 100 MB sıkıştırmasız. *Test:* build klasör boyutu.
- [ ] 1.22 `index.html` arşiv kökünde; ad/klasörde boşluk ve Rusça karakter yok. *Test:* zip içeriği listesi.
- [ ] 1.23 Çalışma anında etkileşimli YZ yok. *Test:* kod/bağımlılık taraması.
- [ ] Portal para birimi kullanılmıyor (satın alma v1 dışı). *Test:* tarama.

**Kullanım / UX**
- [ ] 1.3/4.7 Odak kaybı + reklamda oyun ve ses durur, dönünce devam. *Test:* sekme değiştir, reklam oynat.
- [ ] 1.6.1 Mobil: tam ekran, uzun basış menüsü/metin seçimi yok, WebGL uyarısı yok, tam dokunmatik. *Test:* gerçek telefon.
- [ ] 1.6.2 Masaüstü: sığar, uzun kenar ≤ 2× kısa kenar, sağ tık menüsü yok, klavye düzeninden bağımsız (scancode/WASD+oklar), OS kısayolu yok. *Test:* pencere boyutları + AZERTY/RU düzeni.
- [ ] 1.9 İlerleme eylemden hemen sonra kaydedilir; yenile/döndür kaybettirmez. *Test:* level bitir → F5.
- [ ] 1.10 Yeniden boyutlandır/döndür doğru çizilir; kaydırma/pull-to-refresh yok; tek elle oynanır. *Test:* telefon dönüş + tek el.
- [ ] 1.14/1.15 Hata, donma, çökme yok; oyun bitmiş görünür. *Test:* tam tur, Console/F12 temiz.
- [ ] 2.2 Kontroller oyunda veya taslak How to play alanında anlatılır. *Test:* ilk açılış.
- [ ] 2.8/2.9 Zorluk kademeli; ana içerik > 10 dk. *Test:* oynanış süresi ölçümü.
- [ ] 2.14 Dil `ysdk.environment.i18n.lang` ile otomatik (RU/EN/TR). *Test:* tarayıcı/hesap dilini değiştir.

**İçerik / mağaza**
- [ ] 3.4 Siyaset/savaş/din/büyü/gerçekçi şiddet yok. *Test:* içerik gözden geçirme.
- [ ] 3.5/3.6 Tüm materyalin hakkı bizde; katalogdan kopya yok; `CREDITS.md` güncel.
- [ ] 5.x/8.2.2 Taslak alanları, benzersiz ad (free/top/best/game yok), gerçek oynanış görüntüleri.

**Bilinen sapmalar:** Resmi sayfanın güncel hâli giriş için *yalnız Yandex ID* ve S3 mutlak URL yasağını açıkça yazıyor; her ikisi yukarıya eklendi.

### 0B.3 Ölçek ve bütçe kararları

Ölçülen mevcut durum (2026-10-12) → hedef:

| Kalem | Şu an | Hedef |
|---|---|---|
| Aktif platform | StandaloneWindows64 | WebGL |
| Renk uzayı | Linear | Gamma (G1'de ölçüp teyit) |
| Kalite seviyeleri | 1 (Medium) | Low / Medium (+ High masaüstü) |
| Sahne sayısı | 32 (8.4 MB) | 3–4 sahne (Boot, Menu, Gameplay) + LevelData |
| `Assets/Audio` | ≈ 42.7 MB | ≤ ~8 MB |
| `Assets/Plugins` | ≈ 12.3 MB | İncele, gereksizi at (Y2) |
| Assets toplamı | ≈ 75.6 MB | Build ≤ ~40 MB sıkıştırmasız (sınır: 100 MB); Brotli sonrası indirme ≤ ~15 MB |
| Texture | — | Çoğu 512, en çok 1024; tek palet atlası |
| Performans | — | Mobil 30 FPS (orta seviye Android), masaüstü 60 FPS; draw call ≤ ~150; level başı ≤ ~100k üçgen; ucuz/gölgesiz mobil ışık |
| Render | Built-in, Unity 6000.3.25f1 | Built-in korunur (URP'ye geçiş WebGL'de gereksiz risk/boyut) |

> Not: Yandex'in sabit sınırı yalnızca 100 MB'dır; diğer rakamlar mobil tarayıcıda hızlı yükleme için **kendi hedeflerimizdir**.

### 0B.4 Kapsam kararları (v1)

1. **Platform:** Telefon + masaüstü. TV v1 dışı (uzaktan kumanda okları + Back/OK zorunlulukları ek iş). Yönelim taslakta seçilir; öneri: **yatay**. Döndürme/yeniden boyutlandırma testleri zorunlu.
2. **Para kazanma:** Yalnızca reklam (interstitial + rewarded). Uygulama içi satın alma v1 dışı (sunucu kaydı, tüketim yöntemi, konsol senkronu gibi ek koşullar). Sonraki güncellemede değerlendirilir.
3. **Giriş:** Misafir olarak tam oyun; Yandex girişi yalnız isteğe bağlı butonla. Liderlik tablosu ikincil.
4. **Diller:** RU (öncelik), EN, TR; diğer diller EN'ye düşer.
5. **Araç çeşitliliği:** v1 dışı; tek araba modeli (+ renk).
6. **Ortam:** 5 tema korunur ama ortak palet/prefab havuzu paylaşılır; tema = renk, ışık, zemin, ses, birkaç özgün prop. G3–G7 buna göre küçültülür.
7. **Level süresi:** Level < 5 dk (hedef 30–120 sn) → **level içinde reklam yok**; reklam yalnız oyun dışı eylemlerden sonra.
8. **Mobil kontrol:** Tek el şartı için Y3'te karar verilir.
9. **İçerik:** Gerçek marka/logo yok; siyaset, din, savaş, askeri araç, şiddet yok.
10. **Yayın sürümleri:** Önce yayınlanmayan bir WebGL **dikey dilim** (menü + 1 level + SDK + reklam akışı; draft modunda test), sonra **v1 = 30 level, 3 dünya (Şehir Otoparkı, Liman, Şantiye)** yayınlanır; **v2 = Level 31–50 + Kış/Gece ortam kitleri** güncelleme olarak eklenir (güncelleme ana konsepti korur — 1.24). Gerekçe: rating yayından ~2 hafta sonra oluşur ve 3 hafta ≤ 30 kalırsa oyun yayından kalkar; moderasyon 3–5 iş günü sürer; erken yayın gerçek oyuncu verisi verir. Mevcut 30 legacy level (A9) v1'in referansıdır. Engel dağılımı: hız tümseği/rampa/hareketli engeller v1'de; buz/çamur/su ve kural bölgeleri v2'de.

### 0B.5 Reklam kuralları özeti

- **Interstitial:** Yalnız oyun dışı eylemden sonra (Sonraki, Tekrar, Menü, Level seç); eylem ile reklam arası gecikme ≤ 2 sn. Sıklığı platform sınırlar (çok sık çağrı konsol uyarısı verir); ek olarak kendi bekleme aralığımızı koyarız (öneri: ≥ 60–90 sn).
- **Rewarded:** Oyuncu isteyerek ayrı butona basar; buton/metin reklam izleneceğini ve **somut ödülü** açıkça yazar. Ödül ek bonus olmalı (ek süre, ipucu, level atlama) ve devam edebilmenin şartı olmamalı → **Tekrar dene / sonraki level ücretsiz kalır**.
- Reklam sırasında oyun ve ses duraklar; reklam sonrası ilerleme kaybolmaz; reklam yönüyle oyun yönü aynıdır.
- Yandex reklam birimlerinin içeriği/görünümü değiştirilmez; taklit tam ekran reklam veya sahte rewarded butonu yok. Ek birim gerekirse yalnız sticky banner.

### 0B.7 WebGL baseline sonucu (Y2, 2026-10-13)

- Unity 6000.3.25f1, WebGL, OpenGLES3 (WebGL2), Brotli, strip Low, build süresi ~7 dk, **0 hata / 10 uyarı**. Çıktı: `Builds/WebGL_Baseline` (`index.html` kökte ✔).
- **Boyut:** sıkıştırmasız ≈ **52 MB** (limit 100 ✔, hedef ~40 ✘ -12 MB fazla); Brotli ≈ **13.0 MB** (hedef ~15 ✔).
  - `wasm` 38.7 MB (br 6.8) → asıl şişkinlik; kod/strip/paket taraması gerek (sonraki optimizasyon adımı).
  - `data` 13.1 MB (br 6.1); `framework.js` 0.4 MB.
- **Ses kaynağı:** 42.7 MB = `Music_Level.wav` 20.4 + `Music_Menu.wav` 21.0 MB (WebGL'de AAC override var; kaynak WAV'lar repo'yu şişirir → ileride kısa/loop OGG/AAC kaynak). Build'e girmeden toplam ses hedefi (~8 MB) data içinde sağlanıyor görünür; ses payı ayrıca ölçülmeli.
- **12.3 MB "Plugins":** `Assets/Plugins/Roslyn` (MCP aracı DLL'leri) — yalnız Editor, build'e **girmez**; sadece kaynak ağacında yer kaplar.
- **SDK / ready:** `SettingsYG2.autoGRA = True` → `GameReadyAPI` oyun yüklenince otomatik; oyun kodunda elle `GameReadyAPI`/`GameplayAPI.start/stop` çağrısı **yok** (J5/B3'te eklenecek; ready'nin ilk oynanabilir anda olması için AutoGRA kapatılıp elle çağrılması önerilir).
- **Cihaz uyumu (1.20):** Unity 6 WebGL yalnız **WebGL2** → WebGL2 olmayan eski cihazlar (eski iOS/Android) desteklenmez; taslakta platformlar yalnız gerçek cihazda doğrulananlarla seçilecek. **Bekleyen:** orta/düşük uç telefonda ve Safari/Firefox'ta aç-oyna testi (elle, K4).

### 0B.8 Mobil tek-el kontrol kararı (Y3, 2026-10-13)

**Gereksinim 1.10.4:** oyun tek elle kontrol edilebilir; ana ekran kaydırma/sürükleme gerektirmez, gerekli tüm öğeler hemen görünür ve erişilebilir. İhlal = yayın reddi. (1.6.1.4: ivmeölçer kullanımı *izinli*, zorunlu değil.)

**Karar: (c-analog) Tek başparmak, dinamik joystick.** Ekranın alt yarısında (başparmak bölgesi) herhangi bir yere dokunulan nokta joystick merkezi olur; **yatay = direksiyon, dikey = gaz/geri**. Yukarı: ileri gaz · aşağı: hareket hâlindeyken fren, durunca geri vites · bırak: gaz bırakma (mevcut motor freni + durma tutma). Ayrı fren butonu mobilde yok (aşağı çekmek `reverseBrakeTorque` ile fren yapar, CC-1/CC-2 mantığı zaten bunu karşılıyor). Masaüstü: W/S/A/D + ok tuşları + Space (değişmez).

| Aday | Sonuç | Neden |
|---|---|---|
| (a) Yatay kaydırıcı + otomatik gaz + fren/geri butonu | ✘ | Kaydırıcı ve buton aynı anda gerekir → tek başparmakla olmaz; otomatik gaz park hassasiyetini bozar. |
| (b) Eğim (accelerometer) + gaz/fren butonu | ✘ (v1) | Unity WebGL mobil tarayıcıda `Input.acceleration` yok → özel JS eklentisi + iOS'ta kullanıcı jestiyle izin gerekir; yatay/dikey dönüşte ve düşük uç cihazda kararsız; park hassasiyeti zayıf. v2 isteğe bağlı seçenek olabilir. |
| (c) Dokun-sürükle hedef yönlendirme | ✘ | Fizik tabanlı WheelCollider aracına hedef yönlendirme için yeni sürücü katmanı gerekir; mevcut gaz/direksiyon mantığını bozar. |
| **(c-analog) Dinamik joystick** | ✔ | Tek temas, tek başparmak; mevcut `Vertical`/`Horizontal` eksenleri aynen kullanılır → `CarControl` girdi mantığı değişmez. |

**Uygulama notları:** SimpleInput `Joystick` bileşeni zaten `xAxis=Horizontal`, `yAxis=Vertical`, `isDynamicJoystick` seçeneğini ve `movementAreaRadius`'u sunuyor (doğrulandı) → yeni script gerekmez. Joystick dokunma alanı yalnız alt yarıda; pause/yeniden başlat/kamera butonları üst köşelerde, joystick ile **çakışmayacak** (1.10.3). Ölü bölge ≈ %10, yatay hassasiyet yumuşatması `steerResponse` ile (CC-10). Dil/en-boy: safe area içinde (E9).

**Kapsam:** C6 (`ICarInput` + dokunmatik şema), E5 (HUD yerleşimi), D2 (yön/fren girdisi) bu karara göre. **Prototip/test:** Play modunda henüz denenmedi; C6/E5'te gerçek telefonda (dikey+yatay, tek elle) doğrulanacak; his kötüyse alternatif olarak (b) v2'ye ertelenir.

### 0B.9 WebGL şablonu (Y4, 2026-10-13)

Dosyalar: `Assets/WebGLTemplates/YandexGames/index.html` + `style.css` (şablon zaten `PROJECT:YandexGames`). Mevcut: viewport `user-scalable=no`, `overflow:hidden`, `overscroll-behavior:none`, `touch-action:none`, `user-select:none`, sağ tık kapalı.

**Eklenenler:**
- Viewport: `maximum-scale=1`, `viewport-fit=cover`; `html/body` `position:fixed` → sayfa hiçbir koşulda kaymaz (1.10.2).
- Pull-to-refresh / pinch / çift dokunma yakınlaştırması: `touchmove` + `gesture*` + `dblclick` `preventDefault` (1.10.2, 1.6.1).
- Metin seçimi, sürükle-bırak, uzun basış menüsü: `selectstart`/`dragstart` engeli, `-webkit-touch-callout:none`, `user-drag:none` (1.6.1).
- Masaüstü en-boy sınırı: `ApplyAspectCap()` uzun kenarı ≤ 2× kısa kenar yapar (siyah bant, ortalı); mobilde tam ekran. `resize`/`orientationchange`/`fullscreenchange`'de yeniden hesaplanır (1.6.2, 1.10).
- Boşluk/ok/PageUp-Down tuşları sayfayı kaydırmaz (CC-5 notu). Tarayıcı yakınlaştırması (Ctrl+tekerlek) bilinçli olarak **engellenmedi** (moderasyon %80–125 yakınlaştırmayı dener).
- WebGL2 yoksa / başlatma hatasında boş ekran yerine RU/EN/TR kısa mesaj.
- Sistem video oynatıcısı: oyunda video yok; Yandex SDK `<video>` kullanmıyor (ek iş yok).

**Test (K4'te, yeni build ile):** telefonda dikey+yatay döndür, aşağı çek (yenileme yok), uzun bas (menü yok), iki parmak (zoom yok); masaüstünde 3440×1440 ve 800×1600 pencere (2:1 sınırı), %80/%125 yakınlaştırma (kaydırma çubuğu yok), Space/oklar (sayfa kaymıyor), sağ tık (menü yok).

### 0B.10 İçerik / telif politikası (Y5, 2026-10-13)

**Kabul edilen kaynaklar:** yalnız kendi üretimimiz (ProBuilder/Blender/prosedürel), CC0, ya da önceden üretilmiş YZ içeriği. CC-BY gibi atıf gerektiren, 'ticari kullanım yok' veya 'paylaşım-benzeri' lisanslar kabul edilmez. Her üçüncü taraf dosya `CREDITS.md` tablosuna satır olmadan projeye girmez.

**Yasaklı içerik (3.4):** gerçek marka/logo, siyaset, din, savaş/askeri araç, büyü, gerçekçi şiddet. Ek kurallar:
- Araba modeli gerçek bir marka/model kopyası olmaz; kurgusal ad, kurgusal logo, plaka yalnız rastgele/kurgusal metin.
- Billboard, tabela, kamyon/konteyner yazıları ve reklam panoları kurgusal markalarla yapılır; gerçek firma adı yazılmaz.
- Müzik/ses: kayıtlı şarkı, film/oyun sesi örneği yok; yalnız CC0/prosedürel/YZ-önceden-üretilmiş.
- YZ içeriği `CREDITS.md`'de 'YZ-önceden-üretilmiş' lisansıyla araç adı ve tarihle kaydedilir; gerçek kişi/marka benzerliği taşıyan çıktı atılır.

**Oyun içi krediler:** Ayarlar > Hakkında ekranında düz *metin* olarak (RU/EN/TR başlık, kaynak adları dilden bağımsız). Metin `CREDITS.md`'den üretilir; CC0 için atıf zorunlu değildir ama nezaket olarak yazılır. UI uygulaması E fazında (Ayarlar ekranı adımı) yapılır.

**Oyun adı (5.x/8.2.2):** 'Parking Mastery' — yasaklı kelimeler (free/top/best/game) yok, uygun. **Benzersizlik henüz doğrulanmadı:** 'Parking Master' benzeri adlar park türünde yaygın; K6'da Yandex Games kataloğunda ad araması yapılır. Çakışma varsa yedek: ayırt edici alt başlık veya farklı ad (taslak alanında RU/EN/TR ad aynı kök olmalı).

**Farklılaşma notu (3.6) — kendi tasarım kararlarımız:**
- Mobilde tek başparmak dinamik joystick (0B.8); ayrı fren butonu yok.
- 3 tematik dünya (Şehir Otoparkı, Liman, Şantiye) + baştan tasarlanmış engel sistemi (Faz G2); level başına hedeflenmiş öğretici öğe.
- Level içinde reklam yok; 'Tekrar dene' ücretsiz; rewarded yalnız isteğe bağlı ayrı butonla.
- Tam RU/EN/TR yerelleştirme, Kiril destekli font.
- **K6 görevi:** katalogdan ≥3 benzer park oyunu incele; görsel/isim/ikon/level düzeni birebir benzemesin; bulgu bu bölüme eklenir. Katalog karşılaştırması henüz yapılmadı.
