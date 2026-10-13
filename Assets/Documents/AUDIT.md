# Script Denetimi

> Kapsam: bu dosya denetimi bölüm bölüm biriktirir. Yalnız **açık** bulgular tutulur; bitirilen iş satırıyla birlikte silinir, kaydı tutulmaz.
> Yöntem: **statik kod okuma** (çalıştırılarak doğrulanmadı; 'Doğrula' etiketli maddeler testle teyit edilmeli).
> Önem: 🔴 Yüksek (hata / çökme / kalıcı bozulma riski) · 🟠 Orta (tasarım / bakım / performans) · 🟡 Düşük (stil / küçük iyileştirme)

---

## 1/3 — Car/*

İncelenen dosyalar: `Assets/Scripts/Car/CarControl.cs` (291 satır), `WheelControl.cs` (33), `CarSoundControl.cs` (91), `CarCameraControl.cs` (75).

### Özet

| Önem | Adet |
|---|---|
| 🔴 Yüksek | 0 |
| 🟠 Orta | 7 |
| 🟡 Düşük | 4 |

> Açık bulgu sayıları.

**Genel tablo:** Sürüş mantığı okunaklı ve iyi yorumlanmış (gaz bırakma freni, durma tutma torku, çarpma bekleme süresi, OnEnable/OnDisable abonelik çiftleri). Kalan işler yeniden yazımda (B/C/D/F fazları) ele alınacak.

### CarControl.cs

| ID | Önem | Bulgu | Öneri | Adım |
|---|---|---|---|---|
| CC-5 | 🟠 | **Girdi doğrulaması ve mobil uygulama eksik:** Space tarayıcıda sayfayı kaydırır (şablonda engellenmeli); klavye düzeninden bağımsızlık (Yandex 1.6.2) doğrulanmadı; mobil dinamik joystick için `ICarInput` uygulaması yok (ayrı fren butonu kalkacak). | WebGL build'de AZERTY/Kiril düzende W/A/S/D ve Space testi; joystick uygulamasını yaz. | C6, K4 |
| CC-6 | ✅ | **Çözüldü:** GameLoopManager, LevelSelectionMenu, LevelTimeCounter, MusicManager, EngineSoundUI, InLevelUI, LevelPopupManager, SwitchImage, TextTranslator artık `YG2`/`using YG;` kullanmıyor; hepsi `GameServices` üzerinden (`Ads`, `Saves`, `Leaderboard`, `Localization`, `Platform`). Yandex'e bağlı yalnız `Services/Yandex*.cs` + `SavesYG.cs`. | Kalan: `IAudioService` (müzik/motor sesi şu an `ISaveService` ayarı + doğrudan AudioSource) → F fazı. Play modunda test edilmedi. | F |
| CC-7 | 🟠 | **Olaylar tipsiz ve yüksüz** (`EventType` enum + `UnityEvent`). `ObstacleHit(remaining)` olayına abone UI/ses yok. | Tipli olay sistemi; `ObstacleHit` abonelerini (UI/ses) yaz. | B5, E5, O10 |
| CC-8 | 🟠 | **Çarpma tespiti dar:** yalnız `OnCollisionEnter`; duvara sürtünerek gitmek sayılmaz (tasarım kararı olabilir). Etiket kontrolü `other.transform` üzerinde; collider alt objedeyse ve etiket kökte değilse çarpma kaçar. | Karar ver: sürtünme ceza mı? `ObstacleBase` bileşeniyle tanı (tag yerine). | O3 |
| CC-12 | 🟡 | Mobilde 'araç sıfırla' butonu yok (masaüstünde R tuşu ve `CarControl.ResetCar()` hazır). Yalnız devrilme otomatik algılanıyor; yan/çarpmada takılma algılanmıyor, yalnız elle sıfırlanır. | Butonu `ResetCar()`'a bağla; takılma algısı gerekirse ekle. | E5 |
| CC-13 | 🟡 | `stopHoldTorque` eğimde aracı 'asılı' tutar; rampa/tümsek (O8) gelince yokuşta kalkış/kayma hissi beklenmedik olabilir. | Rampa testinde eşikleri yeniden ayarla. | O8, D1 |

### WheelControl.cs

| ID | Önem | Bulgu | Öneri | Adım |
|---|---|---|---|---|
| WC-3 | 🟡 | İz/toz için `WheelHit` (kayma) bilgisi kullanılmıyor. | D5'te `GetGroundHit`. | D5 |

### CarSoundControl.cs

| ID | Önem | Bulgu | Öneri | Adım |
|---|---|---|---|---|
| CS-1 | 🟡 | Motor sesi ayarı `GameServices.Saves` üzerinden okunuyor; kayıt yüklenmeden varsayılan 'açık' sayılıyor. Ayar değişimi olayla iletilmiyor. | Motor sesi ayarını olayla ilet. | B4, F2 |
| CS-3 | 🟠 | Hız kaynağı `linearVelocity.magnitude` (yanal/dikey dahil); gaz girişi, vites/RPM, lastik kayması yok. | RPM + gaz tabanlı `EngineAudio`. | F4 |
| CS-4 | 🟠 | `AudioMixer` yok; ses doğrudan `AudioSource`'ta. Reklam/odak kaybında susma yalnız `MuteCarSound` olayına bağlı. | Mixer grubu `Engine`; durdurma `AudioService`'ten. | F1, F2, J6 |

### CarCameraControl.cs

| ID | Önem | Bulgu | Öneri | Adım |
|---|---|---|---|---|
| CM-6 | 🟠 | Kamera çarpışması yok (engel içinden geçebilir); çarpışma/park sinematiği ve mobil FOV ayarı eksik. | Kamera çarpışması, sinematik, mobil FOV. | D4 |

### Bu denetimde doğrulanamayanlar
- Car prefab'ındaki Rigidbody (kütle, interpolation, collision detection) ve WheelCollider ayarları: Level 2 sahne dosyasında bulunmadı (prefab örneği). **A7 / D1'de** okunacak.
- Bulgular statik okumaya dayanır; çalışma zamanı testiyle teyit edilmeli.

### Yeniden yazımda öncelik sırası (öneri)
1. **B4 / B5 / C1 ile:** CC-6, CC-7 (servis ve olay altyapısı).
2. **C6 / E5 / K4 ile:** CC-5 (mobil joystick girdisi, WebGL klavye testi), CC-12 (araç sıfırla butonu).
3. **O3 ile:** CC-8 (engel tanıma).
4. **D fazı:** CC-13, WC-3, CM-6.
5. **F fazı:** CS-1, CS-3, CS-4.



## A5 — Script denetimi 2/3: Oyun akışı ve çekirdek (2026-10-13)

Kapsam: `GameLoopManager, ParkingArea, LevelTimeCounter, StarCounter, EventBus, SceneLoadManager, Initialization, SavesYG, AdManager`. Sadece okuma; kod değiştirilmedi. Önem: 🔴 yüksek, 🟠 orta, 🟡 düşük.

| ID | Önem | Bulgu | Öneri | Faz |
|---|---|---|---|---|
| PA-1 | ✅ | **Çözüldü (2026-10-13):** `ParkingArea`'ya `OnTriggerExit` ve ortak `ResetParkingState()` eklendi; araç alandan çıkınca süre/renk/ok sıfırlanıyor, `CarOutsideParkingArea` yayınlanıyor (başarıdan sonra sıfırlama yok). Play modunda simüle edildi. | Kalan: çok collider'lı araçta çıkış/giriş titremesi → PA-2 ile birlikte doğrula. | C1 |
| PA-2 | 🟠 | Araçta birden fazla `Player` etiketli collider varsa `_carStayTime += Time.deltaTime` her biri için ayrı artar (2 sn daha erken dolar). Doğrula; tek "araç gövdesi" collider'ı veya `attachedRigidbody` ile tekille. | Araç kökünden tek kontrol. | C1/D |
| PA-3 | 🟡 | Başarı süresi `2f` sabit (`ParkingArea` ve `InLevelUI` ayrı ayrı yazmış); `DirectionControl` `Dot >= 0` olduğundan 90° yan duran araç da geçerli sayılır; `_meshRenderer`/`_arrowDown` null kontrolü yok; `_carStayTime`, `_carInsideParkingPoint` `public` alan + alt çizgili ad. | Sabit/serialize alan, açı eşiği (ör. ≤ 30°), salt okunur özellikler. | C1 |
| GL-1 | 🔴 | **Seviye numarası sahne adından `int.Parse` ile okunuyor** (`LoadNextLevel`, `UnlockLevel`). Sahne adı sayı değilse istisna; son seviyede (30) olmayan "31" sahnesi yükleniyor ve 31. seviye açılıyor. 50 level için toplam seviye sayısı bilinmiyor. | `LevelCatalog` (seviye sayısı, sahne adı, süre eşikleri); son seviyede menüye dön. | B3/H |
| GL-2 | 🟠 | **Ödül kimliği filtrelenmiyor:** `GameLoopManager.ResumeLevel`, `LevelTimeCounter.ResumeCounting`, `LevelPopupManager.ShowPauseAndCameraButtons` her ödüllü reklamda çalışır. `AdManager` yalnız `"resume"` gönderiyor ama sabit değil, başka ödül eklenirse oyun yanlışlıkla devam eder. | `AdRewardIds.Resume` sabiti, işleyicilerde `id` kontrolü. | C1/J |
| GL-3 | 🟠 | **Yıldız eşikleri iki yerde kopya:** `StarCounter.ControlStars` ve `GameLoopManager.UnlockLevel` aynı karşılaştırmayı ayrı yazıyor. Süre `int` saniye ve `ParkingSuccessful` dinleyici sırası `StopCounting` ile belirsiz → süre en çok 1 sn kayabilir. | `StarCounter.GetStars(time)` tek kaynak; süreyi başarı anında dondur. | C1 |
| GL-4 | 🟡 | `FindFirstObjectByType` sonuçlarında null kontrolü yok (`UnlockLevel` NRE verebilir); `PauseGame/ResumeGame` statik `Time.timeScale` (reklam/sekme kesintisinde durum izlenmiyor). | Kayıtlı referans + `PauseService` (reklam/sekme için sayaç). | B/J |
| LT-1 | 🟠 | `ResumeCounting` `CancelInvoke` yapmadan `InvokeRepeating` çağırıyor; sayaç çalışırken ödül gelirse **iki sayaç** çalışır (süre 2x akar). Başarıdan sonra gelen ödül de sayacı yeniden başlatır. | Durum bayrağı / coroutine; `CancelInvoke` önce. | C1 |
| EB-1 | 🟠 | `EventBus` ve `EventBus<T>` statik sözlükler: Domain Reload kapalıysa dinleyiciler Play'ler arası kalır (`GameServices` sıfırlanıyor, EventBus sıfırlanmıyor). `EventBus<T>` içinde `Clear` yok. `EventType` adı `UnityEngine.EventType` ile çakışabilir. | `[RuntimeInitializeOnLoadMethod]` ile temizle, `GameEvent` adı (CC-7 ile birlikte), `Clear` ekle. | B5/C1 |
| EB-2 | 🟡 | `UnityEvent` her yayında yük bindiriyor (WebGL'de GC); tek parametreli sürüm dışında yük yok. Olay başına tür bağı yok (ör. `ObstacleHit` yükünün `int` olduğu yorumda). | Hafif `Action` tabanlı olay + olay başına tipli yük. | C2 |
| SL-1 | 🟠 | `SceneLoadManager`: hep sahne adı/`buildIndex` ile senkron yükleme; `RestartScene` `timeScale` sıfırlamıyor (çağıran `ResumeGame` yapmak zorunda); aynı işin hem örnek hem statik sürümü var; `using UnityEngine.UI` gereksiz. WebGL'de yükleme ekranı/ilerleme yok. Yandex kuralı için seviye geçişinde reklam kancası yok. | Tek `SceneLoader` (async + `timeScale=1`), seviye geçişi arayüzü. | B5/J |
| IN-1 | 🟠 | `Initialization`: `Application.targetFrameRate = 60` WebGL'de tarayıcı `requestAnimationFrame` yerine zamanlayıcıya düşer; Unity WebGL için varsayılan bırakmayı önerir. Ayrıca sahneye yerleştirilen `MonoBehaviour` ile yapılıyor. | WebGL'de ayarı kaldır/koşullu yap; `RuntimeInitializeOnLoadMethod`'a taşı. A8'de ölç. | A8/K |
| SV-1 | 🟡 | `SavesYG`: 32 slot sabit (50 level için `EnsureSize` büyütüyor, sorun yok); `SetLevelStars` 0-3 aralığına kısıtlanmıyor; kayıt şema sürümü yok; indeks 0 boş slot. `namespace YG` partial zorunlu (plugin). | `Mathf.Clamp(stars,0,3)`, `saveVersion` alanı. | B4/J |
| AD-1 | 🟠 | `AdManager`: `"resume"` sabit string; çift tıklamada art arda `ShowRewarded`; `IAdService`'te reklam açıldı/kapandı olayı yok → Yandex gereği reklam sırasında ses kapatma/oyun duraklatma yok. | `IAdService.AdStarted/AdClosed`, `AdRewardIds`, "reklam sürüyor" koruması. | J |

**Özet:** 14 bulgu (🔴 2, 🟠 8, 🟡 4). A5 başta `PA-1` ve `GL-1` ile devam etmeli; ikisi de oyuncuyu doğrudan etkiliyor.


## A6 — Script denetimi 3/3: UI, Ses, Seviye menüsü, LevelBuilder (2026-10-13)

Kapsam: `UI/*` (9 dosya), `Audio/*` (3), `LevelSelectionMenu`, `Editor/LevelTools/LevelBuilder.cs`. Sadece okuma; kod değiştirilmedi, sahneler açılmadı (sahne bağlantıları A7'de). "Doğrula" geçen maddeler sahne/Inspector kontrolü ister.

| ID | Önem | Bulgu | Öneri | Faz |
|---|---|---|---|---|
| MM-1 | 🟠 | **Reklam/sekme gizlenince ses kesilmiyor:** `Time.timeScale` sesi durdurmaz; müzik yalnız `AudioSource.mute` ile yönetiliyor, `AudioListener.pause`/odak kaybı kullanılmıyor. Yandex moderasyonu reklam sırasında ve sekme gizliyken sesin kapanmasını ister. YG2 eklentisinin otomatik ses duraklatma seçeneğini doğrula. | `IAudioService` + `AdStarted/AdClosed` ve görünürlük olayında `AudioListener.pause`. | F/J |
| MM-2 | 🟠 | `MusicManager` kırılgan: müzik `Pause/UnPause` + `Invoke(1f)` ile yönetiliyor (kaynak hiç çalmadıysa `UnPause` bir şey yapmaz, `playOnAwake`'e bağımlı); ilk sahnede `Start` ve `OnSceneLoaded` ikisi de `MusicControl` çağırıyor; `FindAnyObjectByType<MainMenuMusicUI>()` sonucu null kontrolsüz; statik `instance` sıfırlanmıyor; sahne adları string. | `Play/Stop` + çapraz geçiş (fade), sahne türü için tek arayüz. | F |
| MM-3 | 🟡 | `MainMenuMusicUI.musicManager` alanı `MusicManager` tarafından `Find` ile dolduruluyor (ters bağımlılık). | `MusicManager.Instance` kullan. | F |
| SFX-1 | 🟠 | `LevelSFXManager` klipleri sihirli indekslerle çalıyor (`_sfxs[0..3]`), uzunluk/null kontrolü yok; klip eksikse `IndexOutOfRange`. PA-1 sonrası `CarOutsideParkingArea` artık gerçekten tetiklendiği için ses ilk kez duyulabilir; çok collider'lı araçta (PA-2) tekrar çalabilir. | Adlandırılmış alanlar/`enum` ile eşleme, doğrulama. | F |
| SFX-2 | 🟡 | `SFXManager` `AudioSource`'u `Start`'ta alıyor (`Awake`'ten önce çağrılırsa NRE); iki ayrı SFX yöneticisi var; SFX için ayar/ses seviyesi yok (yalnız müzik ve motor sesi). | Tek `IAudioService`, kategori başına ses (müzik/SFX/motor). | F |
| UI-1 | 🟠 | **İkon durumu ayardan türetilmiyor:** `GeneralUIManager.SwitchImage` sprite'ı kayıttan bağımsız çeviriyor; `SwitchImage` yalnız `Start`'ta okuyor. Müzik butonu ve `MusicManager.Switch()` ayrı çağrılırsa ikon ile gerçek durum ayrışabilir. Sahnedeki UnityEvent bağlantılarını doğrula. | Müzik için de `EngineSoundUI` gibi `Loaded` olayıyla ayardan türeyen tek bileşen. | E/F |
| UI-2 | 🟡 | `SwitchImage.saveVariableName` serbest string (yazım hatası sessiz uyarı); `GeneralUIManager` açma/kapama mantığını `LevelPopupManager` ile kopyalıyor; çok sayıda kullanılmayan `using` ve karışık satır sonu (`LevelPopupManager.InvokePause`, `EngineSoundUI`, `ParkingArea`). | `enum` alan, tek popup yardımcısı, `.editorconfig` (CRLF/LF). | C1/E |
| LP-1 | 🟠 | `LevelPopupManager`: `CloseAllPopupLevel` içinden `ClosePausePopup` (ResumeGame + UnMute yayını) çağrılıp sonra bir de `GameLoopManager.ResumeLevel` aynı işi yapıyor (çift devam); popup'lar `SetActive` ile bağımsız, geri/Esc tuşu ve açık popup yığını yok; `OpenPausePopup` iki kez çağrılabilir (0.05 sn gecikme). | Popup yığını + tek `PauseService`; Esc/geri desteği. | E |
| IU-1 | 🟠 | `InLevelUI.Update` her karede `ParkingArea` alanlarını yokluyor; `FindFirstObjectByType` null ise her karede NRE; bekleme süresi `2f` sabit (PA-3), renk yeşile döndükten sonra sıfırlanmıyor. Zaten `CarInside/CarOutside` olayları var. | Olay tabanlı güncelleme, süre `ParkingArea`'dan. | E/C1 |
| IU-2 | 🟡 | `HomeButton` `"MainMenu"` string; mobil kontrol yalnız `Start`'ta bir kez karar veriyor. | Sahne adı sabiti, `IPlatformService` ile tek merkez. | E |
| SC-1 | 🟡 | `StarCounterUI` yük (`starCount`) ile dizi indeksini aynı anlamda kullanıyor (1 yıldız → `[1]`, 2 yıldız → `[2]`), sınır kontrolü yok; `StarCounter` eşik aşılınca her saniye yeniden yayın yapıyor. `LevelTimeCounterUI` her saniye `ToString` ile string üretiyor, `mm:ss` biçimi yok. | "Kaybedilen yıldız" olayı, bir kez yayın, biçimlendirme. | E/C2 |
| TT-1 | 🟠 | **Yerelleştirme sahneye gömülü:** `TextTranslator` her `Text`'te kendi çeviri listesini taşıyor (32 sahne, her metin ayrı); dil kodu bölge eki (`tr-TR`) için eşleştirme yok, dil değişiminde güncelleme yok, `Text` yoksa NRE. Legacy `UI.Text` özel karakterler (Türkçe/Kiril) için yazı tipi riski taşıyor. | Merkezi tablo (ScriptableObject/JSON) + anahtar tabanlı `LocalizedText`, TMP. | E |
| LS-1 | 🟠 | `LevelSelectionMenu` kayıtları yalnız `Start`'ta okuyor; WebGL'de SDK verisi geç gelirse kilitler/yıldızlar yanlış kalır (`Saves.Loaded` dinlenmiyor). | `Saves.Loaded` ile yeniden çiz. | E |
| LS-2 | 🟡 | Seviye numarası düğmedeki `Text`'ten `int.Parse` ile okunuyor; kilit ikonu için `GetChild(0)` varsayımı; 30 düğme elle yerleştirilmiş, yıldız grubu 3 çocuk varsayıyor. 50 level için ölçeklenmez. | `LevelCatalog` + sayfalı/otomatik düğme üretimi. | E/H |
| LB-1 | 🟠 | **`LevelBuilder` yalnız 3 seviye tanımlı** (`Get` her bilinmeyen seviye için `L3` döndürüyor). Araç ölçüleri (`HullHalf*`, `TrigHalf*`, `RearToOrigin`, park alanı boyutu, `FitTol`) koda gömülü; D fazında araç prefab'ı/fizik değişirse doğrulama sessizce yanlış sonuç verir. | `LevelSpec` veri dosyası (A9 `legacy_levels.json`), araç ölçülerini prefab'tan oku. | G2/H |
| LB-2 | 🟠 | `OpenScene(Single)` açık, kaydedilmemiş sahneyi uyarmadan atar; `GameObject.Find("Obstacles"/"Car"/"ParkingArea"/"Managers")` ve `Camera.main` null ise açıklamasız NRE. Menü komutu seviye 1-3'ü onaysız kaydeder. | `SaveCurrentModifiedScenesIfUserWantsTo`, null kontrolü + açıklayıcı hata, onay penceresi. | H |
| LB-3 | 🟠 | `Obstacles` altındaki tüm çocuklar `DestroyImmediate` ile siliniyor (elle koyulanlar dahil, Undo yok); prefab yolları/adları sabit (`TrafficCones`, `RoadBarrier-2`, `RoadBollard-2`), `Load` null dönerse `InstantiatePrefab` istisnası. G2'de engeller yeniden tasarlanınca kırılır. | Üretilen engellere ayrı kök/etiket, `Undo`, prefab eşleme tablosu. | G2 |
| LB-4 | 🟡 | 397 satırda 6 tür tek dosyada ve global ad alanında (`LevelSpec`, `LevelBuilder`... ileride runtime adlarıyla çakışabilir); `ParkWindow` içinde atıl satır (`FwdOf(areaYaw);`). Editor klasöründe olduğu için WebGL derlemesine girmiyor (doğru). | Ad alanı `ParkingMastery.Editor`, dosyalara böl. | H |

**Özet:** 19 bulgu (🔴 0, 🟠 11, 🟡 8). Öncelik: `MM-1` (Yandex ses kuralı, yayın engeli olabilir), `LS-1`, `LB-2` (veri kaybı riski). A5 + A6 birlikte 33 bulgu; yeniden yazım sırasında B/C fazlarında kapatılacak.

## A7 — Sahne denetimi (2026-10-13)

Kapsam: `MainMenu`, `LevelSelectionMenu`, `Levels/1`, `15`, `30`. Sahneler eklemeli açılıp okundu, değiştirilmedi, kapatıldı. Eksik script yok, her sahnede tek `EventSystem` var, hiçbir sahnede lightmap/reflection probe yok.

| Sahne | Nesne | Renderer | Vertex | Collider (Mesh) | Dosya | Prefab örneği | Işık |
|---|---|---|---|---|---|---|---|
| MainMenu | 60 | 17 | 11 b | 2 (2) | 146 KB | 0 | 1 Directional, gerçek zamanlı, soft gölge |
| LevelSelectionMenu | 74 | 0 | 0 | 0 | 239 KB | 0 | 1 Directional (gereksiz) |
| Level 1 | 323 | 176 | 39,6 b | 123 (9) | 187 KB | 70 | aynı |
| Level 15 | 357 | 209 | 57 b | 64 (50) | - | - | aynı |
| Level 30 | 879 | 564 | 105,9 b | 354 (74) | 623 KB | 231 | aynı |

Seviye sahnelerinin çekirdeği prefab (`Canvas_InLevel`, `Car`, `Managers`, `Audio`, `Ground`, `ParkingArea`, `Main Camera`); `MainMenu` ve `LevelSelectionMenu` hiç prefab içermiyor. Shader'ların hepsi `Standard` (L15/L30'da ek `Unlit/Color`).

| ID | Önem | Bulgu | Öneri | Faz |
|---|---|---|---|---|
| SN-1 | 🟠 | **Bir seviye = bir sahne:** engeller sahnede tek tek prefab örneği (L1: 54 koni + 7 bariyer, L30: 222 engel). 30 sahne bugün 8.4 MB; 50 level, her engel değişikliği için sahne düzenleme ve sahne başına yükleme maliyeti demek. `LevelBuilder` zaten yol/engel verisinden üretiyor. | Karar gerekli (B3/H): seviye verisi (ScriptableObject/JSON) + tek `Level` sahnesi ya da seviye prefab'ları. | B3/H |
| SN-2 | 🟠 | **Çok `MeshCollider`:** L15'te 64 collider'ın 50'si, L30'da 354'ün 74'ü `MeshCollider` (L1'de 9). Araç-engel temasını (ObstacleHit, bariyer hakkı) kararsızlaştırır ve WebGL'de fizik maliyeti yükler. Hangileri convex/engel prefab'ına ait, doğrula. | Engel prefab'larında Box/Capsule collider, tek tek temizle. | G2 |
| SN-3 | 🟠 | **Işık/render ayarı varsayılan:** her sahnede 1 gerçek zamanlı Directional, `Soft` gölge, `Default-Skybox` ortam ışığı, sis yok, lightmap yok; kamerada `HDR` ve `MSAA` açık, far 1000. L30'da 564 renderer ve 105 bin vertex gölge dökerken WebGL'de pahalı. Sahneler arası görsel çeşitlilik yok. | Gölge mesafesi/hard shadow, kamerada HDR kapalı + far ~200, bake veya basit aydınlatma; pipeline kararı G'de. | G/K |
| SN-4 | 🟠 | **Canvas ölçekleme 800x600 (4:3), match 0.5:** masaüstü 16:9 ve dikey mobilde UI orantısız ölçeklenir; güvenli alan (safe area) yok. İki yazı tipi karışık (`Roboto-Regular` ve yerleşik `LegacyRuntime`), hepsi legacy `Text`. | Referans 1920x1080 + match tuning, `SafeArea`, tek yazı tipi (TMP). | E |
| SN-5 | 🟠 | **Reklam tetikleri sahnede UnityEvent'le bağlı:** her seviyede 5 düğme doğrudan `AdManager.OpenFullscreenAd` çağırıyor (Ana menü/Yeniden/Sonraki vb.), kodda sıklık sınırı yok; ödüllü reklam düğmesi `OpenRewardAd`. Yandex doğal duraklarda ve makul aralıkla ister. YG2 interstitial zamanlayıcı ayarını doğrula. | Tek `AdFlowController`, bekleme süresi, "reklam sürüyor" durumu. | J |
| SN-6 | 🟡 | **UI tıklama sesi kopyala-yapıştır:** `SFXManager.PlaySFX` persistent çağrısı 12 (seviye) + 31 (seçim menüsü) + 5 (ana menü) düğmede; `LoadScene` 31 düğmede sahne adı string parametresiyle. | Tek `UiClickSound` bileşeni / ortak düğme davranışı. | E |
| SN-7 | 🟡 | `Initialization` ve `MusicManager` yalnız `MainMenu` sahnesinde: bir seviye sahnesi doğrudan açılırsa (editör testi, yeniden yükleme) FPS ayarı ve müzik gelmiyor (Play testinde `MusicManager` bulunamadı). Seviye sahnesinde `Car_Engine` `playOnAwake` ile hemen çalıyor. | Açılış (bootstrap) sahnesi / `RuntimeInitializeOnLoadMethod`. | B |
| SN-8 | 🟡 | **UI-1 doğrulandı:** `MainMenu`'de müzik düğmesi hem `GeneralUIManager.SwitchImage` (×2) hem `MainMenuMusicUI.Switch` (×2) çağırıyor; ikon ayardan değil tıklamadan çevriliyor. Kayıt yüklenmeden tıklanırsa ikon ve ayar ayrışır. `EngineSoundUI` ayardan türetiyor (doğru örnek). | Müzik ikonunu da ayardan türet. | E |
| SN-9 | 🟡 | `MainMenu`: kamerada `Animator`, dekor kökü `Scene` 17 renderer + 2 `MeshCollider` (gereksiz), arka plan Skybox; `LevelSelectionMenu` `SolidColor` ve içinde işe yaramayan Directional Light. Menüler tutarsız. | Ortak menü arka planı/prefab. | E/G |
| SN-10 | 🟡 | Metinler: `MainMenu`'de 3 `Text` ve `TextTranslator` yok (çevrilmiyor); seviye içinde 2 `Text`, 1 `TextTranslator`, 48 `Image` — metinlerin çoğu görsel olarak sprite'a gömülü olabilir (doğrula), yerelleştirmeyi zorlaştırır. | Metinleri yazı olarak tut, `LocalizedText` (TT-1). | E |

**Özet:** 10 bulgu (🟠 5, 🟡 5). Öncelik: SN-1 (mimari karar, 50 level planını etkiler), SN-2/SN-3 (WebGL performansı, A8'de ölçülecek). A5+A6+A7 toplam 43 bulgu.