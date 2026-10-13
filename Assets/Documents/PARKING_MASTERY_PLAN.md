# Parking-Mastery — Baştan Profesyonel Yeniden Yapım Planı

> **Nasıl kullanılır (ücretsiz Claude için):**
> 1. Bu dosyayı proje klasörünün köküne koy (ör. `PARKING_MASTERY_PLAN.md`).
> 2. Her yeni sohbette şunu yaz: *"Planı oku, ilk işaretlenmemiş `[ ]` adımı yap, bitince `[x]` yap ve özet geç."*
> 3. **Bir sohbette sadece 1 adım (en fazla 2 küçük adım).** Her adım tek oturumda bitecek boyutta tasarlandı.
> 4. Her adımın sonunda: Console'da hata yok → sahne kaydedildi → (varsa) git commit.
> 5. Bir adım yarım kalırsa, adımın altına `> NOT:` satırıyla nerede kaldığını yaz.
> 6. **Tamamen bitmiş adımlar dosyadan silinir:** Bir adım tamamen bittiyse (açık kalan iş, test edilmemiş kısım veya ertelenen madde yoksa) adımı ve altındaki `> NOT:` satırlarını sil; sonraki adımlar için gereken bilgi varsa ilgili `[ ]` adımına kısa not olarak taşı. Yarım kalan, notunda açık iş/test edilmemiş kısım olan ve işaretlenmemiş `[ ]` adımlar dosyada kalır.

Boyut etiketleri: **[S]** küçük (~1 sohbet'in yarısı), **[M]** orta (1 sohbet).

---

## 0. Mevcut Durum (otomatik tespit edildi)

- Sahneler: `MainMenu`, `LevelSelectionMenu`, `Levels/1 … 30` (toplam 32 sahne, hepsi Build Settings'te).
- Oyun scriptleri (`Assets/Scripts`, 26 adet): `AdManager, LevelSFXManager, MusicManager, SFXManager, CarCameraControl, CarControl, CarSoundControl, WheelControl, EventBus, GameLoopManager, Initialization, LevelSelectionMenu, LevelTimeCounter, ParkingArea, SavesYG, SceneLoadManager, StarCounter, EngineSoundUI, GeneralUIManager, InLevelUI, LevelPopupManager, LevelTimeCounterUI, MainMenuMusicUI, StarCounterUI, SwitchImage, TextTranslator`.
- Eklentiler: **PluginYourGames** (Yandex SDK — dokunulmayacak, sadece sarmalanacak), **SimpleInput**.
- Örnek seviye hiyerarşisi (Level 3): `Directional Light, Audio, Managers (5 script bir arada), Canvas_InLevel (8 çocuk; üstünde InLevelUI + LevelTimeCounterUI + StarCounterUI + LevelPopupManager + AudioSource + SFXManager + AdManager!), Ground (tek mesh), Main Camera, EventSystem, ParkingArea, Car, Obstacles (96 düz çocuk)`.
- Tespit edilen sorunlar:
  - Her level sahnesinde yönetici/UI kopyası var → 30 yerde aynı şeyi düzeltmek gerekiyor.
  - Bir Canvas objesi UI + ses + reklam + popup görevlerini birlikte üstlenmiş (God Object).
  - Obstacles düz liste (grup/prefab yok), Ground tek mesh, **ortam (environment) yok**.
  - Projede `Assets/Editor/LevelTools/LevelBuilder.cs` var (bugün değişmiş) → ne olduğu incelenecek.
- Kısıtlar: **Yandex Games (tarayıcı/WebGL)** → küçük build (sınır 100 MB sıkıştırmasız), düşük draw call, mobil+masaüstü, RU/EN/TR yerelleştirme. Ayrıntılı kısıtlar ve ölçek kararları: **Bölüm 0B**.

---

## 0B. Yandex Games Kısıtları ve Ölçek Kararları

> **Ayrıntılı tablolar (0B.1 Kitle, 0B.2 Zorunlu gereksinimler, 0B.3 Ölçek/bütçe, 0B.4 Kapsam, 0B.5 Reklam kuralları): `YANDEX_CHECKLIST.md`.** Planda geçen "bkz. 0B.x" atıfları o dosyadaki başlıklara karşılık gelir; ilgili adımda sadece o bölümü oku.

**Özet (her adımda geçerli kararlar):**

- **Hedef:** Yandex Games, WebGL; telefon + masaüstü (TV v1 dışı), yön önerisi yatay. Mobilde tek elle oynanabilir (Y3 kararı).
- **Bütçe:** Yandex sınırı toplam ≤ 100 MB (sıkıştırmasız). Kendi hedeflerimiz: build ≤ ~40 MB, Brotli indirme ≤ ~15 MB, ses ≤ ~8 MB; mobil 30 FPS / masaüstü 60 FPS, draw call ≤ ~150, level başı ≤ ~100k üçgen.
- **Reklam:** Yalnız reklam geliri (satın alma v1 dışı). Level içinde reklam yok; interstitial yalnız oyun dışı eylemden sonra (≤ 2 sn gecikme, kendi bekleme aralığımız ≥ 60–90 sn); rewarded ayrı butonla, somut ödül yazılı, devam etmenin şartı değil (Tekrar dene ücretsiz). Reklam sırasında oyun + ses durur.
- **Giriş / dil:** Misafir olarak tam oyun, Yandex girişi isteğe bağlı buton. Diller RU (öncelik) / EN / TR, dil `ysdk.environment.i18n.lang` ile otomatik; Kiril destekli font.
- **Kapsam:** Tek araba modeli (+ renk). 5 tema ortak palet/prefab havuzunu paylaşır. v1 = 30 level, 3 dünya (Şehir Otoparkı, Liman, Şantiye); v2 = Level 31–50 + Kış/Gece. Önce yayınlanmayan WebGL dikey dilim.
- **İçerik:** Gerçek marka/logo, siyaset, din, savaş, askeri araç, şiddet yok; her üçüncü taraf dosya `CREDITS.md`'ye yazılır.

### 0B.6 Yandex uyumluluk adımları

- [x] **Y5 [S]** İçerik/telif politikası: yalnız kendi üretimimiz / CC0 / önceden üretilmiş YZ içeriği; atıf gerektiren lisanslardan kaçın; gerçek marka/logo yok; siyaset/din/savaş/şiddet yok; krediler oyun içi *metin* olarak; katalogdaki benzer park oyunlarından farklılaşma notu (3.6); oyun adı kuralları (benzersiz; free/top/best/game gibi kelimeler yok).
  > NOT: Y5 bitti — politika `YANDEX_CHECKLIST.md` 0B.10'da, `CREDITS.md` kural 6-7 eklendi. Açık kalanlar: oyun adı benzersizlik kontrolü ve katalog karşılaştırması K6'da; oyun içi kredi ekranı E fazında.

---

## 1. Varlık (Asset) Stratejisi — Ücretsiz & Abonelik Yok

| İhtiyaç | Yöntem (öncelik sırasıyla) |
|---|---|
| **Ses efektleri** | (a) Unity Editor içinde **C# ile prosedürel WAV sentezi** (tık, kazanma, çarpma, fren, motor döngüsü, yıldız). (b) Telifsiz CC0 paketler (Kenney, OpenGameArt-CC0) — Unity Editor'den indirmeyi denerim; olmazsa sen indirip klasöre atarsın. |
| **Müzik** | (a) Prosedürel/jeneratif döngüler (basit melodi + ritim, C# ile WAV) — menü + oyun içi hafif döngü. (b) CC0 müzik (OpenGameArt, Kenney) — lisansı `CREDITS.md`'ye yazılır. |
| **3D model** | (a) Unity primitifleri + **ProBuilder** ile low-poly (bina, çit, bariyer, ağaç, koni, lamba, konteyner). (b) CC0 paketler: Kenney, Quaternius (indirme denemesi / senin indirmen). (c) **Blender 5.2** (kurulu ve bağlı, 2026-10-12 doğrulandı) — özgün/organik/karmaşık işlerde Blender Bridge ile (`run_python`, `import_model`, `compare_screenshot`). **Hangisinin kullanılacağı işe göre seçilir (aşağıdaki seçim kuralı); Blender zorunlu değildir.** |
| **Texture / Materyal** | Prosedürel texture (gürültü, asfalt, çimen, yol çizgisi), tek renk paletli atlas (düşük boyut), CC0: ambientCG / Poly Haven (kullanıcı indirir ya da Editor'den indirmeyi dene). |
| **Yapay zekâ ses/model üretimi** | Unity MCP'de `generate_audio / generate_image / generate_model` araçları var ama **sağlayıcı API anahtarı** ister (fal.ai, Tripo, Meshy vb.). Ücretsiz kredi/plan sunuyor olabilirler; anahtarı sen alırsan kullanırım. **Bu planın hiçbir adımı buna bağımlı değil.** |

Kural: Üçüncü taraf her dosya için `CREDITS.md` satırı (kaynak, lisans) eklenir.

**Model yöntemi seçim kuralı (Blender zorunlu değil):** Her model/prop işinde o işe en uygun yöntem seçilir; yöntemler karıştırılabilir, seçim ve gerekçesi ilgili adımın notuna yazılır.

| İş türü | Tercih edilen yöntem |
|---|---|
| Basit/geometrik şekiller (duvar, kolon, bariyer, çit, kaldırım, rampa, tümsek, zemin, işaretler) | Unity primitifleri / **ProBuilder** — doğrudan Unity'de, hızlı, tek palet, basit collider |
| Hazır ve stile uyan CC0 karşılığı olanlar (araba, ağaç, konteyner, çöp kutusu, lamba…) | **CC0 paket** (Kenney, Quaternius) — poligon fazlaysa ya da stile uymuyorsa elenir; `CREDITS.md` satırı şart |
| Organik/karmaşık şekiller, özgün prop'lar, CC0 modeli düzenleme/sadeleştirme, modifier (bevel, array, mirror), rig/animasyon, çok sayıda varyant | **Blender** (`blender_bridge`) — glb ile içe aktarılır, `compare_screenshot` ile kontrol edilir |

Seçim ölçütleri (sırayla): stil uyumu (temiz low-poly, tek palet) → poligon/WebGL bütçesi → toplam süre/emek → lisans. Blender kapalıysa ya da bağlanamıyorsa hiçbir adım bloklanmaz; ProBuilder/CC0 yoluna düşülür.


---

## 2. Faz A — Hazırlık ve Denetim


- [x] **A4 [M]** Script denetimi 1/3: `Car/*` (CarControl, WheelControl, CarSoundControl, CarCameraControl) → `AUDIT.md` içine sorun listesi.
> NOT: `AUDIT.md` hazır (28 bulgu). Çözüldü: CM-1/2/3, CC-3, CS-1, CC-1/2/9/10, CC-4 (CC-7/8 kısmen). Kalan: CC-7 yeniden adlandırma → B5/C1; CC-8 → O3; CS-2..5 → F fazı. Play modunda test edilmedi; D2'de `CarController` yazılırken bu davranışlar korunmalı.
- [x] **A5 [M]** Script denetimi 2/3: `GameLoopManager, ParkingArea, LevelTimeCounter, StarCounter, EventBus, SceneLoadManager, Initialization, SavesYG, AdManager`.
> NOT: `AUDIT.md` → 'A5' bölümü hazır (14 bulgu: 🔴 PA-1 ParkingArea çıkış yakalama yok, GL-1 seviye numarası `int.Parse`). CC-6 de bu sırada kapatıldı (kalan: `IAudioService` → F). Play modunda ödül/yıldız/servis hızlı testi geçti; müzik butonu denenmedi (MusicManager MainMenu'de).

---

## 3. Faz B — Mimari Temeli

Hedef klasör yapısı:

```
Assets/_Project/
  Art/ (Models, Materials, Textures, Shaders, VFX)
  Audio/ (Music, SFX, Mixer)
  Data/ (LevelData SO'ları, Config SO'ları)
  Prefabs/ (Car, Environment, Obstacles, UI, Systems)
  Scenes/ (Boot, MainMenu, LevelSelect, Gameplay, Levels/)
  Scripts/ (Core, Gameplay, Car, UI, Audio, Services, Editor)
```

- [x] **B2 [M]** Assembly Definition + namespace düzeni (`ParkingMastery.Core/Gameplay/UI/Services`).
> NOT: 6 runtime asmdef + 1 Editor asmdef (`Assets/_Project/Scripts/<Klasör>/ParkingMastery.<Klasör>.asmdef`), tüm kod namespace'e alındı. Bağımlılık grafiği (döngüsüz): `Core` (bağımsız) ← `Services` (SimpleInput.Runtime) ← `Car`, `Audio`, `Gameplay` (Core+Services) ← `UI` (Services, Car, Core, Gameplay, UnityEngine.UI); `Editor` → Gameplay. Namespace'ler: `ParkingMastery.Core/Services/Car/Audio/Gameplay/UI/EditorTools/Yandex`. DÖNGÜLER KIRILDI: (1) `GameLoopManager`→`LevelPopupManager` yerine `GameLoopManager.CloseAllPopupsRequested` static event'i (LevelPopupManager OnEnable/OnDisable'da abone); (2) `MainMenuMusicUI` UI→`Audio` klasörüne taşındı (GUID korundu). YG eklentisi asmdef'siz (Assembly-CSharp) olduğundan `SavesYG` + `Yandex*Service` → `Scripts/Platform/Yandex/` (asmdef'siz, `ParkingMastery.Yandex`); `GameServices` artık YG'yi bilmiyor, varsayılanlar `YandexServicesBootstrap` ile `SetDefaultFactories` üzerinden kaydoluyor (Play + Editor). `EventType` UnityEngine.EventType ile çakıştığı için kullanan dosyalarda `using EventType = ParkingMastery.Core.EventType;` alias'ı var → C1/B5'te yeniden adlandırılmalı (örn. `GameEventType`). Kontrol: derleme hatasız; 32 sahnede missing script 0, 1004 UnityEvent çağrısının hepsi geçerli; 35 prefab temiz; editörde `GameServices.Ads`=YandexAdService. Yedek: proje kökünde `Backup_B2_Scripts_111149` (sorun yoksa silinebilir). Play modunda/WebGL'de test edilmedi.
- [x] **B3 [M]** `Boot` sahnesi + kalıcı **Systems prefab'ı** (Audio, Save, Ads, Localization, SceneLoader) — bir kez yüklenir.
> NOT: Boot sahnesi (`Scenes/Boot.unity`, build index 0: Boot Camera + `Systems` prefab örneği + `GameBootstrap`) hazır. `Systems.prefab` (`DontDestroyOnLoad`, tekil `SystemsRoot`): MusicManager (+Menu/Level AudioSource), SceneLoader. `GameBootstrap` servisleri erken başlatır (Platform, Localization, Ads, Saves), kayıt hazır olana kadar en fazla 5 sn bekler, sonra `MainMenu` yükler. Kayıt/Reklam/Yerelleştirme statik `GameServices` üzerinden erişilir (sahne nesnesi değil; B4'te arayüzler tamamlanacak). Editörde Boot dışından Play'e basılırsa `SystemsRoot.EditorEnsureSystems` prefab'ı otomatik oluşturur; menü `Parking-Mastery ▸ Play From Boot` ile Play her zaman Boot'tan başlatılabilir. Kontrol: derleme hatasız, Console'da hata yok, sahnelerde MusicManager/ikinci SystemsRoot kopyası yok. Sahnelerdeki `SceneLoadManager` kopyaları BİLEREK bırakıldı (durumsuz; butonların UnityEvent bağlantıları onlara bağlı) → B7'de tek Gameplay sahnesine geçince kalkacak. Play modunda/WebGL'de Boot→MainMenu akışı test edilmedi.
- [x] **B4 [M]** Servis katmanı: `ISaveService` (Yandex SavesYG sarmalayıcı + yerel yedek), `IAdService`, `ILocalizationService`, `IAudioService`.
  > NOT: B4 TAMAM (2026-10-13). Servisler `GameServices` üzerinden: `Saves`, `Ads`, `Leaderboard`, `Localization`, `Platform`, `Audio` (yeni). (1) `IAudioService` (`Services/`) + `AudioService` (`Audio/`, `AudioServicesBootstrap` ile kayıtlı): müzik/motor sesi ayarını kaydeder, `MusicManager.ApplyMuteState()` ile uygular, `SettingsChanged` olayı var; `MusicManager.Mute/UnMute/CheckMute`, `EngineSoundUI`, `CarSoundControl` buna geçti. (2) `LocalBackupSaveService` (`Services/`, Yandex'e bağlı değil): `ISaveService` sarmalayıcısı, her yazmayı PlayerPrefs JSON yedeğine (`PM_SaveBackup_v1`) yazar; bulut hazır değilken okumalar yedekten gelir, bulut yüklenince birleştirir (açık seviyeler birleşimi, seviye başına max yıldız) ve buluta geri yazar; `YandexServicesBootstrap` `YandexSaveService`'i bununla sarar. `IsReady` bilerek BULUT hazır mı demek (GameBootstrap bulutu bekler). Seviye üst sınırı yedekte 64. (3) `ILocalizationService`: `LanguageChanged` olayı + `SetLanguage` eklendi (`YG2.onSwitchLang/SwitchLanguage`). `Platform/Yandex` dışında `YG2` kullanımı kalmadı. Kontrol: derleme hatasız, Console temiz, yedek mantığı editörde birim denemesiyle doğrulandı (yaz→yeniden oluştur→geri okundu). Test edilmedi: gerçek WebGL/Yandex SDK akışı. KALAN küçük işler: `TextTranslator` hâlâ `Language` okuyor, `LanguageChanged`'e abone olmuyor (Faz E/yerelleştirme adımında); AUDIT.md 'Açık işler' CC-6 satırı kapatılabilir.
- [x] **B5 [M]** `EventBus` yenile (tipli, bellek sızıntısız abone/çıkma) + **GameState makinesi** (Loading/Playing/Paused/Won/Failed).
  > NOT: B5 TAMAM (2026-10-13). (1) `Core/EventBus.cs` yenilendi: `EventType` → **`GameEventType`** (UnityEngine.EventType çakışması ve 12 dosyadaki `using EventType = ...` alias'ları kalktı; `ObstacleHit` enum'dan çıktı). Anahtarlı API (`EventBus.Subscribe/Unsubscribe/Publish(GameEventType, Action)`, `EventBus<T>`) aynı kaldı ama artık UnityEvent değil: aynı dinleyici iki kez eklenmez, yayın sırasında abonelik değişimi güvenli (çıkan dinleyici o turda çağrılmaz), bir dinleyicinin hatası diğerlerini kesmez (loglanır), Play'e girerken (`SubsystemRegistration`) tüm abonelikler sıfırlanır (domain reload kapalıyken bile). `EventBus.ClearAll()` eklendi. (2) **Tipli olaylar**: `IGameEvent` + `EventBus.Subscribe<TEvent>/Unsubscribe<TEvent>/Publish<TEvent>` (readonly struct, GC yok); `Core/GameEvents.cs`: `ObstacleHitEvent(RemainingHits)`, `GameStateChanged(Previous, Current)`. Yeni olaylar tipli yazılmalı; eski anahtarlı olaylar C1'de kademeli taşınır. `CarControl` artık `ObstacleHitEvent` yayınlıyor; `LevelSFXManager` abone (isteğe bağlı `_obstacleHitClip` alanı — F fazında ses atanınca çalar; boşsa çalmaz). (3) **`Core/GameStateMachine.cs`**: `GameState {Loading, Playing, Paused, Won, Failed}`, `Current`, `TryChange`, `CanTransition`; geçersiz geçiş yok sayılır (editör/dev'de uyarı), her değişimde `GameStateChanged`. Geçişler: Loading→Playing; Playing→Paused/Won/Failed/Loading; Paused→Playing/Loading; Won→Loading; Failed→Playing (reklamla devam)/Loading. Ara köprü (B7'ye kadar): sahne yüklenince Loading, sayısal adlı seviye sahnesiyse Playing; ParkingSuccessful→Won, LevelFailed→Failed, ResumeGame→Playing; `GameLoopManager.PauseGame/ResumeGame` Paused/Playing yapar. Menü sahnelerinde durum Loading kalır → B7'de `LevelLoader` geçişleri kendisi yönetecek. Kontrol: derleme hatasız, Console temiz, editörde birim denemesi (çift abone, yayın sırasında çıkış, tipli/yüklü olay, ClearAll, 9 durum geçişi) beklenen sonuçları verdi. Test edilmedi: Play modunda tam oyun akışı (kazan/kaybet/duraklat/reklamla devam) ve WebGL. KALAN küçük işler: `ObstacleHitEvent` için UI aboneliği (kalan hak göstergesi, Faz E) ve ses klibi ataması (Faz F); kalan anahtarlı olayların tipliye taşınması (C1).
- [x] **B6 [M]** `LevelData` ScriptableObject (id, dünya/tema, süre, yıldız eşikleri, araba spawn, hedef, engel listesi, müzik, hava/ışık preset).
  > NOT: B6 TAMAM (2026-10-13). `Core/Data/LevelData.cs` (ScriptableObject, menü `Parking-Mastery ▸ Level Data`): kimlik (`LevelId`, `WorldTheme` 5 dünya), süre/yıldız (`TargetTimeSeconds`, `ThreeStarTime`, `TwoStarTime`, `TimeLimitSeconds` 0=sınırsız), kural (`MaxObstacleHits`), yerleşim (`CarSpawn{position,yaw}`, `ParkingTarget{position,yaw,size,fitTolerance}`, `GroundPrefab`, `Obstacles` listesi `{prefab,position,yaw,scale}`), atmosfer (`Music`, `Environment`). Yalnız veri; alanlar `[SerializeField]` + salt-okunur özellikler. `CalculateStars(sn)` eski `GameLoopManager` kuralıyla aynı (>2★ süresi → 1, >3★ süresi → 2, aksi 3). `Validate(List<string>)`: id, 3★<2★ sırası, süre sınırı>2★, hak≥1, park alanı boyutu>0, boş engel prefab'ı. `Core/Data/EnvironmentPreset.cs` (ışık rengi/şiddeti/yönü, ortam rengi, skybox, sis, hava efekti prefab'ı) yalnız veri; **uygulayıcısı Faz G'de**. Editor aracı: `Parking-Mastery ▸ Levels ▸ Extract LevelData From Open Scene` (`Editor/LevelTools/LevelDataExtractor.cs`) açık level sahnesinden `Assets/_Project/Data/Levels/Level_XX.asset` üretir (StarCounter eşikleri, Car konum/yön/hak, ParkingArea konum/boyut, prefab bağlantılı engeller; bağlantısızlar atlanıp raporlanır). Editor asmdef'ine `Core` ve `Car` referansı eklendi. Şema Level 1 ile doğrulandı: `Level_01.asset` üretildi (3★≤35sn, 2★≤55sn, hak 3, 61 engelin 61'i prefab bağlantılı aktarıldı, Validate=true; bozuk veride Validate=false). Not: `Level_01.asset` ÖRNEK/şema denemesidir; 30 level'in toplu dönüşümü B7'de (veya H fazında yeniden tasarımla) yapılacak. `targetTimeSeconds` çıkarımda 3★ süresine eşitlendi (H2'de gerçek hedef süre belirlenecek). Henüz kullanılmayan alanlar: `Music`, `Environment`, `GroundPrefab` (F/G/B7'de bağlanacak); `TimeLimitSeconds` oyunda uygulanmıyor (zamanlı level mekaniği Faz H). Engel referansı şimdilik ham `GameObject` prefab; O3'te `ObstacleData` SO'suna geçilebilir. Kontrol: derleme hatasız, Console temiz. Play modunda denenmedi (henüz hiçbir şey LevelData okumuyor; B7'de LevelLoader okuyacak).
- [x] **B7 [M]** **Tek `Gameplay` sahnesi** + `LevelLoader`: seviye = LevelData + prefab düzeni (30→50 ayrı sahne yerine veri güdümlü). Eski sahneler referans için `Legacy/`'ye alınır.
  > NOT: B7 TAMAM (2026-10-13; önceki oturum yarım kalmıştı: kod yazılmış, sahne/veri yoktu). (1) `Scenes/Gameplay.unity` (build index 3; Boot 0, MainMenu 1, LevelSelectionMenu 2): eski `1.unity`'den türetildi, `Obstacles` boşaltıldı, `Managers` üzerine `LevelLoader` eklendi (katalog, araç, ParkingArea, StarCounter, Obstacles kökü bağlı). (2) 30 `LevelData` (`Data/Levels/Level_01..30.asset`, Extractor ile) + `Data/LevelCatalog.asset` (Validate=true). 8/15/21'de `Obstacles` dışındaki kök seviye engeller de dahil (3/2/1 adet; eski JSON'dan farkın nedeni bu). (3) `SceneLoadManager`: sayısal ad ('7') → `LevelSession.CurrentLevelId` + Gameplay yükler; `RestartScene` aynı seviyeyi yeniden kurar; `LoadNextLevel` LevelSession üzerinden. `GameStateMachine` köprüsü sadece Loading yapar, Playing'i `LevelLoader.Start` verir. (4) Eski 30 sahne `Scenes/Legacy/`'ye taşındı (GUID korundu), build'den çıkarıldı; `LevelBuilder` yolu güncellendi. Zemin ve Obstacles kökü 30 levelda aynı olduğundan Gameplay'de sabit. Kontrol: derleme hatasız; 30 levelin tümünde `LevelLoader.Apply` sonucu eski sahneyle birebir aynı (engel sayısı+konum toplamı, araç, park alanı, yıldız eşikleri, hak); Play'de Gameplay'den Level_01 (61 engel, Playing) ve çalışırken Level_07'ye geçiş (81 engel) doğrulandı; Console'da hata yok. Test edilmedi: tam oyun akışı (kazan/kaybet/sonraki seviye/reklamla devam), Boot→Menü→Level seç akışı, WebGL; editör arka plandayken RestartScene sonrası Playing geçişi gözlenemedi. KALAN: sahnelerdeki `SceneLoadManager` kopyaları duruyor (butonların UnityEvent'leri onlara bağlı); `GroundPrefab`/`Music`/`Environment` alanları hâlâ bağlı değil (F/G).

---

## 4. Faz C — Kodun Yeniden Yazımı

- [x] **C1 [M]** `GameLoopManager` → yeni `LevelController` (durum makinesi, başla/bitir/yeniden dene).
  > NOT: C1 TAMAM (2026-10-13). `GameLoopManager` → `Gameplay/LevelController.cs` (dosya taşınarak yeniden adlandırıldı, GUID korundu; sahnede `Managers` üzerindeki bileşen bozulmadı). Sorumluluklar: **başlat** (`Start` → seviye yüklüyse `GameState.Playing`; bu iş `LevelLoader.Start`'tan taşındı, `LevelLoader.Start` kaldırıldı), **bitir** (`ParkingSuccessful` → `CompleteLevel`: sonraki leveli aç, yıldızı iyileştiyse kaydet, skor tablosu), **yeniden dene/duraklat/devam/sonraki** (statik `RestartLevel/PauseGame/ResumeGame/LoadNextLevel`, `CloseAllPopupsRequested` olayı aynı). Değişenler: `FindFirstObjectByType` yok, `[SerializeField] _loader` + `_timeCounter` Gameplay sahnesinde bağlı; yıldız artık `LevelData.CalculateStars(float)` ile hesaplanıyor (eski kuralla aynı: >2★ süresi → 1, >3★ süresi → 2; kesirli süre karşılaştırılır, ör. Level_01 @35.4sn=2★, @55.4sn=1★); kayıt çağrıları (`UnlockLevel(id+1)`, `Get/SetLevelStars(id)`) eski değerlerle aynı. `InLevelUI` ve `LevelPopupManager` `LevelController`'a geçirildi. Kontrol: derleme hatasız, Gameplay'de missing script 0, Play'de Level_01 yüklenip `Playing`'e geçti (61 engel), Console'da oyun hatası yok. Test edilmedi: kazan/kaybet akışı ve kayıt yazımı (gerçek kaydı bozmamak için tetiklenmedi), reklamla devam, WebGL. KALAN: `LevelController` yeni kazan/kaybet için tipli olay (`GameStateChanged`) yayını henüz kullanmıyor (anahtarlı `ParkingSuccessful/LevelFailed` hâlâ kullanılıyor).
- [x] **C2 [M]** `ParkingArea` → `ParkingZone` (araç hizası/açı/tam içinde olma kontrolü, bekleme süresi, görsel geri bildirim).
  > NOT: C2 TAMAM (2026-10-13). `ParkingArea` → `Gameplay/ParkingZone.cs` (sınıf yeniden adlandırıldı; sahnede GameObject adı `ParkingArea` kaldı, çünkü `LevelBuilder`/`LevelDataExtractor` ada göre arıyor). Kurallar: araç kutusu bölge içinde (`_fitTolerance`, LevelData.fitTolerance ile) + yatay yön açısı `_maxAngle` (varsayılan 90° = eski kural) + kesintisiz `_requiredStayTime` (2 sn); koşul bozulursa süre sıfırlanır. Görsel geri bildirim: doğru=mavi, yanlış yön=sarı, başarı=yeşil, dışarıda=varsayılan renk; `StayProgress`/`IsCarInside` InLevelUI halkasını sürer. `LevelLoader._parkingArea` → `_parkingZone` (`FormerlySerializedAs` ile; sahne yeniden kaydedildi, referans korundu). Kontrol: derleme hatasız, Console'da hata yok, Gameplay'de missing script 0. Play'de test edildi (Level_01): doğru yön → içeride + süre sayıyor + mavi; 180° ters → sıfırlandı + sarı; dışarı → varsayılan renk. Test edilmedi: bekleme süresi dolunca başarı/yeşil renk (gerçek kaydı bozmamak için tetiklenmedi), diğer levellerde `_maxAngle`/`_fitTolerance` uygunluğu, WebGL. KALAN: `ParkingSuccessful/CarInsideParkingArea/CarOutsideParkingArea` olay adları hâlâ eski (C1/B5 tipli olay geçişiyle birlikte yeniden adlandırılabilir).
- [x] **C3 [S]** `LevelTimeCounter` + `StarCounter` → `LevelScoring` (LevelData eşiklerinden yıldız).
  > NOT: C3 TAMAM (2026-10-13). `LevelTimeCounter` + `StarCounter` → tek `Gameplay/LevelScoring.cs` (`StarCounter.cs` dosyası taşınarak yeniden adlandırıldı, GUID korundu; `LevelTimeCounter` silindi, Gameplay'de `Managers` üzerinden bileşeni kaldırıldı). Sorumluluk: geçen saniyeyi sayar (`Update` + biriken süre; `InvokeRepeating` yok), `LevelTimeUpdated` yayınlar, yıldız kaybedildikçe `StarCountChanged` yayınlar (**yalnız geçişte, bir kez**; eskiden her saniye tekrar yayınlanıyordu), `CalculateStars()` ile ödül yıldızını `LevelData.CalculateStars` kuralıyla verir. `LevelLoader` artık `_scoring.Configure(data)` çağırır (eşikler LevelData'dan); `LevelController._scoring` kullanır (`LevelTimeCounter` bağımlılığı yok). Ödüllü reklamda sayımın çifte başlama hatası kapandı (devam = bayrak). Editör araçları (`LevelBuilder`, `LevelDataExtractor`) `LevelScoring`'e geçirildi. Kontrol: derleme hatasız, Console'da hata yok, Gameplay'de missing script 0, referanslar bağlı. Play'de test edildi (Level_01, eşik 35/55): süre sayıyor; 3★ eşiği aşılınca 2★ + HUD'da 3. yıldız soluk; 2★ eşiği aşılınca 1★ + 2. yıldız soluk. Test edilmedi: park başarısı/kaybedince sayacın durması ve ödüllü reklamla devam (olay yayınlamak kaydı bozacağı için doğrudan tetiklenmedi), süreyi yeniden dene sonrası sıfırlanması, WebGL. KALAN: `LevelTimeCounterUI`/`StarCounterUI` hâlâ ayrı UI sınıfları (F/E fazı UI yeniden tasarımında); eşik alanları (`_threeStarTime/_twoStarTime`) yalnız LevelData'sız eski sahneler/editör araçları için yedek olarak duruyor (C7 temizliğinde kaldırılabilir).
- [x] **C4 [M]** Kayıt sistemi: ilerleme, yıldızlar, ayarlar, en iyi süre; veri sürümleme ve eski kaydı taşıma (mevcut oyuncuların kaydı kaybolmasın).
  > NOT: C4 TAMAM (2026-10-13). Kayıt şeması sürümlendi: `SavesYG.saveVersion` + `CurrentSaveVersion = 1` + `MigrateIfNeeded()` (v0 → v1: alanlar eksikse tamamlanır, yıldız 0–3'e sıkıştırılır, yıldızı olan seviye ve sonraki seviye açık sayılır, negatif süre temizlenir; **hiçbir ilerleme silinmez**, mevcut JSON alan adları değişmedi, yani eski oyuncu kaydı olduğu gibi okunur). Yeni: `levelsBestTime` + `ISaveService.GetBestTime/TrySetBestTime/SaveVersion`; `LevelController.CompleteLevel` en iyi süreyi yazar. `YandexSaveService` bulut verisi gelince taşır ve değiştiyse geri yazar (`OnSdkData`); `LocalBackupSaveService` (PlayerPrefs `PM_SaveBackup_v1`, JSON) buluta hazır değilken yedek tutar, bulut gelince **birleştirir (en yüksek yıldız, açık seviyeler, en kısa süre; asla düşürmez)**; `YandexServicesBootstrap` ikisini bağlar. Kontrol: derleme hatasız, Console'da hata yok, Gameplay'de missing script 0. Bellekte test edildi (gerçek kayda dokunmadan): sahte v0 kayıt taşındı (L1=3★, bozuk 7★→3★, yıldızlı seviye + sonrası açık, `levelsBestTime` null→liste, toplam yıldız doğru), ikinci `MigrateIfNeeded` çağrısı değişiklik yapmadı, `TrySetBestTime` yalnız daha kısa süreyi kabul etti, yeni kayıt L1 açık / L2 kapalı. Test edilmedi: gerçek Yandex bulut kaydıyla yükleme/geri yazma (WebGL + SDK gerekir), `LocalBackupSaveService.Merge` canlı çakışma senaryosu (bulut ile yerel farklıyken), en iyi sürenin UI'da gösterimi (UI fazı E'de).
  > KALAN: yerel yedek `Snapshot`'ında sürüm alanı yok (şema değişirse eski yedek okunurken alan eksikliği `Ensure` ile tolere ediliyor ama açık bir sürüm eklenmeli); `MaxLevels = 64` sabiti seviye sayısı (50) için yeterli ama tek yerde tanımlı değil (C7/C8 temizliğinde `SavesYG` ile birleştirilebilir).
- [ ] **C5 [M]** Reklam akışı (Yandex 4.x kurallarına göre, bkz. 0B.5): interstitial **yalnız oyun dışı eylemden sonra** (Sonraki / Tekrar / Menü) ve ≤ 2 sn gecikmeyle, level içinde reklam yok; kendi bekleme aralığımız; rewarded **ayrı butonla ve ödülü yazan metinle** (ek süre / ipucu / level atlama) — ödül devam etmenin şartı olamaz, yani Tekrar dene ücretsiz kalır; reklam sırasında oyun + ses durur; test modu (draft/prod ortamı).
- [ ] **C6 [S]** Girdi: klavye (fiziksel tuşlar, klavye düzeninden bağımsız, OS kısayollarıyla çakışmayan) + fare + dokunmatik (SimpleInput) tek `ICarInput` arayüzü arkasında; **mobil: tek başparmak dinamik joystick (Y3, bkz. 0B.8): yatay=direksiyon, dikey=gaz/geri, ayrı fren butonu yok**.
  > NOT: C6 KISMEN (AUDIT CC-5, 2026-10-13): `ICarInput` + `SimpleInputCarInput` yazıldı (`Assets/Scripts/Services/`), `CarControl` arayüzü kullanıyor (`SetInput`). KALAN: mobil dinamik joystick uygulaması (Y3), WebGL'de klavye düzeni/Space testi.
- [ ] **C7 [S]** Eski kullanılmayan script/prefab/asset temizliği (referans taraması ile).
- [ ] **C8 [S]** Kod stil geçişi: null-check, `FindObjectOfType` / `Update` içi tahsisleri kaldır, `[SerializeField]` kullanımı, XML özet yorumları.

---

## 5. Faz D — Araba Fiziği, Kontrol, Kamera

- [ ] **D1 [M]** Rigidbody + WheelCollider ayarlarını yeniden kur (ağırlık merkezi, süspansiyon, sürtünme eğrileri) — park manevrasına uygun "kontrollü arcade" his.
- [ ] **D2 [M]** `CarController` yeniden yazımı: gaz/fren/geri vites, direksiyon yumuşatma, hıza bağlı direksiyon açısı, el freni.
- [ ] **D3 [S]** Çarpışma: hasar yok ama **çarpışma tepkisi** (ses, kamera sarsıntısı, ceza/uyarı), engellere göre fizik materyalleri.
- [ ] **D4 [M]** Kamera: takip + hıza göre zoom + park anında sinematik yaklaşma; çarpışmada hafif shake; mobilde uygun FOV.
- [ ] **D5 [S]** Araba görseli: materyal, far/stop/fren lambası, tekerlek dönüşü, iz (tire marks) ve toz parçacığı.
- [ ] **D6 [S]** ~~Birden fazla araba~~ — **v1 kapsam dışı** (bkz. 0B.4): tek araba modeli (+ renk seçeneği); çoklu araba sonraki güncellemeye.

---

## 6. Faz E — UI / UX Yeniden Tasarımı

Kararlar: uGUI + TextMeshPro (WebGL için en güvenli), tek **UI tema ScriptableObject'i** (renk, font, buton stili), her ekran ayrı prefab, `Canvas Scaler` referans 1920×1080 + safe area.

- [ ] **E1 [S]** Görsel kimlik: renk paleti, font seçimi (Türkçe/Kiril karakter desteği!), buton/panel sprite'ları (9-slice, prosedürel), ikon seti.
- [ ] **E2 [M]** UI altyapısı: `UIManager`, `ScreenBase`, ekran geçiş animasyonları (CanvasGroup fade/scale), buton ses/animasyon bileşeni.
- [ ] **E3 [M]** **Ana Menü** (Oyna, Ayarlar, Dil, Ses, Liderlik tablosu [isteğe bağlı], Krediler) + hafif arka plan (statik görsel veya basit dönen araba; ağır 3D sahne yok). Çıkış butonu yok (web oyununda işlevsiz).
- [ ] **E4 [M]** **Level Seçim** ekranı: level sayısı `LevelData` listesinden türetilir (v1: 30, v2: 50; 'yakında' kartı yok — 1.15), dünya/sayfa sekmeleri, yıldız gösterimi, kilitli/açık durumu, kaydırma.
- [ ] **E5 [M]** **Oyun içi HUD**: süre, yıldız hedefi, pause, yeniden başlat, sürüş kontrolleri (mobil: alt yarıda dinamik joystick, pause/yeniden başlat/kamera üst köşelerde ve joystick ile çakışmaz — Y3/0B.8; masaüstü ipucu); HUD tüm en-boy oranlarında kesilmez / üst üste binmez.
- [ ] **E6 [M]** **Pause / Kazanma / Kaybetme** popup'ları: yıldız animasyonu, sayaç artışı, sonraki level / tekrar / menü, ödüllü reklam butonu.
- [ ] **E7 [S]** **Ayarlar** paneli (müzik/SFX ayrı slider, dil, titreşim, grafik kalitesi).
- [ ] **E8 [S]** Yerelleştirme: **RU (öncelik) / EN / TR**; dil `ysdk.environment.i18n.lang` ile otomatik seçilir (2.14), desteklenmeyen dil → EN; manuel dil değiştirme bayrak/dil adıyla (bilinmeyen dilde de bulunabilsin); merkezi metin tablosu (`TextTranslator` yerine); Kiril + Türkçe karakterli font.
- [ ] **E9 [S]** Çözünürlük / en-boy oranı testleri (16:9, 4:3, 20:9 dikey/yatay) + safe area.
- [ ] **E10 [S]** Eğitim (tutorial): ilk 3 levelde kısa, atlanabilir ipuçları.

---

## 7. Faz F — Ses ve Müzik

- [ ] **F1 [S]** `AudioMixer` kur: Master / Music / SFX / UI / Engine grupları; ayar kaydı ile bağla.
- [ ] **F2 [M]** `AudioService` + `SoundBank` (SO) — havuzlu AudioSource, 3D/2D ayrımı, ses tekrarını engelleme. Eski `SFXManager/LevelSFXManager/MusicManager` kaldırılır.
- [ ] **F3 [M]** Prosedürel SFX üretici (Editor aracı) → WAV: UI tık/hover, kazanma, kaybetme, yıldız, çarpma (hafif/sert), fren, korna, geri vites bip.
- [ ] **F4 [M]** Motor sesi: hıza/RPM'e bağlı pitch + volume döngü sistemi (`EngineAudio`).
- [ ] **F5 [M]** Müzik: menü döngüsü + 3 oyun içi döngü (üretilmiş veya CC0), geçişte crossfade.
- [ ] **F6 [S]** Ortam sesleri (şehir uğultusu, rüzgâr, liman, kuş) tema başına bir döngü.
- [ ] **F7 [S]** Ses boyutu optimizasyonu — **toplam ses hedefi ≤ ~8 MB** (şu an `Assets/Audio` ≈ 42.7 MB): Vorbis, mono SFX, kısa SFX Decompress On Load, müzik Compressed In Memory/Streaming, müzik sayısı/süresini kıs.

---

## 8. Faz G — Görsel Stil, Işık, Ortam (Environment)

Stil: **temiz low-poly / stylized** (düşük poligon, tek palet, WebGL'de hızlı). **Ölçek kuralı:** 5 tema ortak materyal paletini ve ortak prefab havuzunu paylaşır; tema farkı renk/ışık/zemin/ses/birkaç özgün prop ile verilir (bkz. 0B.4).

- [ ] **G1 [S]** Render ayarları: **Gamma renk uzayı kararı** (şu an Linear), ışık yönü, mobilde gölgesiz/ucuz gölge, ambient, fog, kalite seviyeleri (Low/Med/High — şu an yalnız Medium var); Built-in pipeline korunur.
- [ ] **G2 [M]** Materyal paleti + prosedürel texture seti (asfalt, çizgili otopark, çimen, beton, metal, su, kar).
- [ ] **G3 [M]** Ortam kiti 1 — **Şehir Otoparkı**: kolon, duvar, bariyer, koni, çöp kutusu, lamba, park çizgileri, diğer park etmiş arabalar (prefab). *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **G4 [M]** Ortam kiti 2 — **Liman / Konteyner**: konteyner (mevcut model), vinç silueti, palet, deniz, martı sesi. *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **G5 [M]** Ortam kiti 3 — **Şantiye / Sanayi**: iş makinesi silueti, kum yığını, çit, uyarı bantları. *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **G6 [M]** *(v2)* Ortam kiti 4 — **Kış / Dağ yolu**: kar, çam ağacı, buz zemin (düşük sürtünme), sis. *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **G7 [M]** *(v2)* Ortam kiti 5 — **Gece Şehri / Neon**: gece ışıklandırma, sokak lambası, ışık küreleri, hafif bloom. *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **G8 [S]** Gökyüzü/skybox ve zaman-ışık presetleri (gündüz, gün batımı, gece).
- [ ] **G9 [S]** Dekoratif objeler için **statik batching / GPU instancing / LOD yok-az** kuralı, draw call bütçesi.
- [ ] **G10 [S]** Post-process (hafif, kapatılabilir): bloom, vinyet, renk düzeltme — **mobilde varsayılan kapalı**; WebGL'de maliyeti ölçülerek karar verilir.

---

## 8B. Faz G2 — Engel Sistemi (Baştan Tasarım)

Mevcut engeller (Level'lardaki `Obstacles` altındaki düz listeler) tek tek incelenecek; gereksiz, anlamsız veya oyuna katkısı olmayanlar **atılacak**, kalanlar yeniden yapılacak. Hedef: 50 levele yetecek, her biri bir **amaca hizmet eden** engel kataloğu.

Tasarım ilkesi: Her engel en az birine hizmet etmeli → *yolu daraltır / dikkat ister / fizik ve kontrol becerisini sınar / ortam hissini güçlendirir.* Hiçbiri sırf dolgu olmayacak.

Önerilen katalog (denetimden sonra kesinleşir):

| Kategori | Engeller | Davranış |
|---|---|---|
| Sabit | Duvar, kolon, bariyer, çit, konteyner, park etmiş araba, kaldırım | Çarpınca ceza/uyarı, hareket etmez |
| Fizikli (devrilebilir) | Trafik konisi, varil, koli yığını, çöp kutusu | Çarpınca devrilir; devirme = ceza veya yıldız kaybı |
| Hareketli | Yavaş geçen araç, açılıp kapanan bariyer kolu, kayan kapı | Zamanlama gerektirir (Dünya 3'ten itibaren) |
| Zemin | Hız tümseği, rampa, buz/kar, çamur, su birikintisi | Sürtünme/hız değiştirir (Dünya 2–4) |
| Kural/Bölge | Yasak bölge, kontrol noktası, zorunlu geçiş kapısı | Sahaya kural koyar (Dünya 4–5) |

- [ ] **O1 [M]** Mevcut engel denetimi: tüm 30 levelden kullanılan engel tiplerini çıkar (model/boyut/collider/adet), kullanım sıklığını ve işe yararlığını raporla → `OBSTACLE_AUDIT.md` (tut / yeniden yap / at kararı).
- [ ] **O2 [S]** Engel kataloğunu kesinleştir: her engel için ID, kategori, boyut, ceza kuralı, hangi dünyada tanıtılır → `OBSTACLE_CATALOG.md`.
- [ ] **O3 [M]** Altyapı: `ObstacleBase` + `ObstacleData` (SO) — çarpışma tepkisi, ceza, ses/efekt, etiket/katman; çarpışma matrisi (Physics Layers) düzeni.
- [ ] **O4 [M]** Sabit engel prefab'ları (duvar, kolon, bariyer, çit, kaldırım, konteyner) — düşük poligon, tek materyal paleti, basit collider (BoxCollider ağırlıklı). *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **O5 [M]** Park etmiş araba prefab'ları (3–5 renk/model varyantı, **tek mesh, ucuz collider**). *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **O6 [M]** Fizikli engeller (koni, varil, koli, çöp kutusu): Rigidbody ayarları, uyku modu, devrilme algılama, performans için sınırlı sayı. *Model yöntemi: Bölüm 1'deki seçim kuralına göre (ProBuilder / CC0 / Blender — işe en uygun olan; seçim ve gerekçe adım notuna yazılır).*
- [ ] **O7 [M]** Hareketli engeller: yola göre hareket eden araç, bariyer kolu, kayan kapı — `MovingObstacle` (yol noktaları, hız, bekleme), deterministik zamanlama.
- [ ] **O8 [M]** Zemin engelleri (**v1: hız tümseği + rampa; buz/çamur/su v2**): hız tümseği, rampa, buz, çamur, su — fizik materyali + WheelCollider sürtünme etkisi + ses/efekt.
- [ ] **O9 [S]** *(v2)* Kural bölgeleri: yasak bölge, kontrol noktası, zorunlu kapı (trigger tabanlı).
- [ ] **O10 [S]** Engel geri bildirimi: çarpışmada vurgu rengi/flaş, ses, kısa uyarı ikonu; yıldız kaybı kuralıyla `LevelScoring`'e bağlama.
- [ ] **O11 [S]** Engel yerleştirme araçları (Editor): grid'e oturtma, çakışma kontrolü, minimum geçiş genişliği uyarısı (arabanın boyuna göre).
- [ ] **O12 [S]** Engel prefab'ları test sahnesi: hepsi yan yana, her biri için araba çarpma testi ve performans kontrolü.
- [ ] **O13 [S]** **Engel–Level dağıtım matrisi** (`OBSTACLE_PROGRESSION.md`): 50 levelin her biri için hangi engellerin kullanılacağı, yeni engelin hangi levelde tanıtılacağı, engel sayısı bütçesi (aşağıdaki kurallara göre).
- [ ] **O14 [S]** Her yeni engel tipi için **tanıtım leveli** tasarımı: engel tek başına, rahat alanda, ilk karşılaşma güvenli (ceza yok veya hafif) → sonraki levellerde diğer engellerle birleşir.

### 8B.1 Engel Tasarım Kuralları (50 level için)

1. **Gereksiz engel yok:** Her engel bir amaca hizmet eder (yolu daraltır / dikkat ister / beceri sınar / ortam hissi verir). Sırf boşluk doldurmak için konan engel silinir. (Referans: mevcut Level 3'te 96 düz engel var; çoğunun bu ölçüte uymadığı O1'de ortaya çıkarılacak.)
2. **Tek seferde tek yenilik:** Bir levelde en fazla **1 yeni engel tipi** tanıtılır; ilk görüldüğü level kolay olur.
3. **Kademeli darlık:** Minimum geçiş genişliği arabanın genişliğinin ~2.0 katından (Dünya 1) ~1.25 katına (Dünya 5) kademeli iner. H1 aracı bunu otomatik uyarır.
4. **Adil olma:** Her engel görsel olarak ne olduğunu belli eder (renk/şekil dili: sabit = gri/beton, devrilebilir = turuncu/parlak, hareketli = sarı-siyah şerit, kural bölgesi = zemin işareti). Hiçbir engel görünmez veya haksız ceza vermez.
5. **Performans bütçesi (WebGL/mobil):** level başına fizikli (Rigidbody) engel ≤ ~20, hareketli engel ≤ ~6, collider'lar basit (Box/Sphere), tüm engel prefab'ları ortak materyal paletini kullanır; mobilde 30 FPS hedefi bozulmamalı.
6. **Tekrar yok:** Aynı engel düzeni kopyalanmaz; her level engelleri farklı bir *soruyu* sorar (dar geçit, dönüş, zamanlama, kayma…).
7. **Geniş kitle:** Yandex kitlesi ağırlıklı casual (35+ yaş çoğunlukta) — engeller sezgisel ve adil olmalı; ceza kuralı ikonla/level başında anlatılır, haksız sürpriz ceza yok.

### 8B.2 Engellerin Dünyalara Dağılımı (taslak — O1/O2 sonrası kesinleşir)

| Dünya | Level | Yeni tanıtılan engeller | Kullanılabilen engeller |
|---|---|---|---|
| 1 Şehir Otoparkı | 1–10 | Duvar, kolon, bariyer, kaldırım (1–2) → trafik konisi (3) → park etmiş araba (5) → çöp kutusu (8) | Sabit + hafif fizikli |
| 2 Liman | 11–20 | Konteyner (11), varil (13), koli/palet yığını (15), dar koridor ve paralel park düzenleri | Dünya 1 + konteyner/varil grupları |
| 3 Şantiye | 21–30 | Hız tümseği (21), rampa (23), yavaş geçen araç (25), bariyer kolu (27), kayan kapı (29), kum yığını/çit | + hareketli + zemin |
| 4 Kış Yolu | 31–40 | Buz (31), kar yığını (33), çamur/su birikintisi (35), kontrol noktası (37) | + kaygan zemin; hareketli engellerle birleşim |
| 5 Gece Şehri | 41–50 | Yasak bölge (41), zorunlu geçiş kapısı (44) | Tüm katalog; karışık, dar alan + zaman + az hata payı |


---

## 9. Faz H — 50 Level Tasarımı

Zorluk eğrisi (5 dünya × 10 level):

| Dünya | Level | Tema | Yeni mekanik |
|---|---|---|---|
| 1 Şehir Otoparkı | 1–10 | Açık alan → dar geçit | Temel park, koni, duvar |
| 2 Liman | 11–20 | Konteyner labirenti | Paralel park, dar dönüş |
| 3 Şantiye | 21–30 | Sert zemin, engel yoğun | Zamanlı, hareketli engel (yavaş) |
| 4 Kış Yolu | 31–40 | Kaygan zemin | Düşük sürtünme, uzun fren |
| 5 Gece Şehri | 41–50 | Karışık, usta seviyesi | Dar alan + zaman + az hata payı |

- [ ] **H1 [M]** **Level Editor aracı** (Editor penceresi): LevelData'dan sahne düzeni üretme, engel yerleştirme, "oynanabilirlik" doğrulama (başlangıçtan hedefe ulaşılabilir mi, boşluk ölçüsü).
- [ ] **H2 [S]** Zorluk/süre/yıldız kuralları (`LEVEL_DESIGN.md`): süre formülü, yıldız eşikleri; **level hedef süresi 30–120 sn (5 dk altı)**, ilk level < 1 dk'da kazanılabilir, zorluk kademeli (Yandex 2.8), casual kitleye uygun adil eşikler; toplam içerik > 10 dk (2.9).
> **Bağımlılık:** H3–H12 başlamadan O2, O3, O13 tamamlanmış olmalı. Level tasarlarken engel seçimi `OBSTACLE_PROGRESSION.md`'ye uyar.

> **Sürüm dağılımı:** H3–H8 (Level 1–30) **v1**'de, H9–H12 (Level 31–50) **v2**'de yapılır. H13/H14 v1'de yalnız Level 1–30 için, v2'de 50 level için çalıştırılır.

- [ ] **H3 [M]** Level 1–5 tasarla + test.
- [ ] **H4 [M]** Level 6–10 tasarla + test.
- [ ] **H5 [M]** Level 11–15
- [ ] **H6 [M]** Level 16–20
- [ ] **H7 [M]** Level 21–25
- [ ] **H8 [M]** Level 26–30
- [ ] **H9 [M]** Level 31–35
- [ ] **H10 [M]** Level 36–40
- [ ] **H11 [M]** Level 41–45
- [ ] **H12 [M]** Level 46–50
- [ ] **H13 [S]** Tüm leveller için otomatik doğrulama testi (editor script): spawn, hedef, süre, yıldız, Missing Reference yok.
- [ ] **H14 [S]** Zorluk eğrisi gözden geçirme (özellikle 1–10 ve 25–30 arası sıçramalar).

---

## 10. Faz I — Oyun Hissi (Juice)

- [ ] **I1 [S]** Park başarı efekti: ışık halkası, konfeti, kamera yavaşlama, yıldız sesi.
- [ ] **I2 [S]** Çarpışma geri bildirimi: kıvılcım/toz, kamera shake, kısa titreşim (destekliyorsa).
- [ ] **I3 [S]** Egzoz dumanı, tekerlek tozu/kar spreyi, fren izi.
- [ ] **I4 [S]** UI mikro animasyonlar (buton basma, sayaç, yıldız doldurma).
- [ ] **I5 [S]** Sahne geçişleri: fade + yükleme çubuğu.

---

## 11. Faz J — Yandex Games Entegrasyonu

- [ ] **J1 [S]** Yandex'in **güncel** gereksinim sayfalarını yeniden kontrol et (Game requirements, Ad placement, SDK, Changelog; 0B 2026-10-12 tarihli okumaya dayanır) ve `YANDEX_CHECKLIST.md`'yi güncelle.
- [ ] **J2 [M]** Reklam entegrasyonu (yalnız SDK üzerinden; kendi tam ekran reklam taklidi yok): C5 kurallarının sahaya uygulanması, reklam öncesi/sonrası oyun+ses duraklatma, reklam sonrası ilerleme korunur, gerçek reklamlı test (draft modu / prod ortamı); ek reklam birimi gerekirse yalnız sticky banner.
- [ ] **J3 [S]** Liderlik tablosu (toplam yıldız veya en iyi süre) — **isteğe bağlı**; giriş yalnız oyuncu bilinçli butona basarsa istenir (Yandex ID), giriş yapmayan oyuncu hiçbir şeyden engellenmez.
- [ ] **J4 [S]** Bulut kaydı + yerel yedek: ilerleme her eylemden hemen sonra kaydedilir, sayfa yenileme/ekran döndürmede kaybolmaz (1.9); misafir oyuncunun ilerlemesi de saklanır (1.2.2); draftta bulut kayıt seçeneği açılır; offline davranış.
- [ ] **J5 [S]** SDK yaşam döngüsü: `ysdk.environment.i18n.lang` ile otomatik dil (2.14), oynanabilir olduğu an `LoadingAPI.ready()` (1.19.2), `GameplayAPI.start/stop` (level başı/sonu/pause), `game_api_pause/resume` olayları; PluginYourGames'in bunları doğru yaptığı doğrulanır.
- [ ] **J6 [S]** Sekme değişimi / odak kaybında sesi kıs + oyunu duraklat (1.3 — sık red nedeni); reklam sırasında da aynı davranış (4.7).

---

## 12. Faz K — Optimizasyon ve Yayın

- [ ] **K1 [M]** WebGL build ayarları: Brotli, kod kırpma (Managed Stripping High), Texture/Audio import optimizasyonu, boş shader/paket temizliği. **Hedef:** toplam dosya ≤ ~40 MB sıkıştırmasız (Yandex sınırı 100 MB), Brotli sonrası indirme ≤ ~15 MB (bizim hedefimiz).
- [ ] **K2 [S]** Bellek ve GC: Update içi tahsis taraması, nesne havuzu (efekt/ses).
- [ ] **K3 [S]** Kalite profilleri: Low/Med/High, otomatik cihaz seçimi (mobil → Low/Med).
- [ ] **K4 [M]** Tam oynanış testi: 50 level; masaüstü + mobil tarayıcı; reklam akışı; kayıt/yükleme.
- [ ] **K5 [S]** Hata düzeltme turu (bilinen sorunlar listesi → `KNOWN_ISSUES.md`).
- [ ] **K6 [S]** Mağaza materyalleri: ikon, kapak, ekran görüntüleri (oyundan otomatik alınır), açıklama TR/EN/RU.
- [ ] **K7 [S]** Son build + Yandex taslağına yükleme kontrol listesi (her yayın sürümü — v1 ve v2 — için tekrarlanır): `index.html` arşiv kökünde, dosya/klasör adlarında boşluk ve Rusça karakter yok (1.22); toplam ≤ 100 MB sıkıştırmasız (1.21); **kodda ve şablonda Yandex S3'e mutlak URL yok (1.7) — arama ile tara**; **oyun hangi URL'de açılırsa açılsın çalışır, URL'ye bağlı kısıtlama yok (1.18) — build'i farklı bir adreste/alt yolda açarak test et**; taslakta bulut kayıt seçeneği (1.11), yönelim, desteklenen platformlar (Y2 sonucuna göre), How to play metni (2.2), yaş derecelendirmesi; `YANDEX_CHECKLIST.md` baştan sona işaretli.

---

## 13. Önerilen Çalışma Sırası

Üç aşamada ilerlenir (bkz. 0B.4 madde 10):

**Aşama 0 — Hazırlık:** `1.1–1.3 → A1–A9 → Y1, Y5`

**Aşama 1 — Dikey dilim (yayınlanmaz, erken risk avı):** `Y2 → Y3 → Y4 → B1–B7 → C1–C2, C6 → D1–D2, D4 → E2, E5 (asgari HUD) → C5 (asgari: 1 interstitial + 1 rewarded) → J5 → J6 → K1`
Çıktı: WebGL'de açılan, Yandex SDK'sı başlatılmış, `LoadingAPI.ready()` çağıran, veri güdümlü tek level oynanabilen sürüm. Boyut, FPS, mobil tek-el kontrol, eski cihaz (1.20) ve reklam akışı sonuçları `YANDEX_CHECKLIST.md`'ye yazılır. Büyük içerik işine bu sonuçlar bilinmeden girilmez.

**Aşama 2 — v1 (Level 1–30, 3 dünya) → Yandex'e gönderim:** `C3–C4, C5 (tam), C7–C8 → D3, D5 → F1–F2 → E1, E3–E4, E6–E10 → F3–F7 → G1–G5, G8–G10 → O1–O3 → O4–O7, O8 (yalnız hız tümseği + rampa), O10–O12 → O13–O14 (Level 1–30 kısmı) → H1–H2 → H3–H8 → H13–H14 (Level 1–30) → I → J1–J4, J2 (tam) → K2–K7`
Level seçim ekranı yalnız mevcut leveli gösterir (`LevelData` sayısından türetilir); 'yakında' kartı konmaz (bitmemiş görünüm riski, 1.15).

**Aşama 3 — v2 (Level 31–50) → güncelleme:** `G6–G7 → O8 (buz, çamur, su) → O9 → O13–O14 (Level 31–50 kısmı) → H9–H12 → H13–H14 (50 level) → E4 güncelle → K4–K7 (50 level tam test, mağaza metinleri/ekran görüntüleri güncelle)`

**Not:** Engel sistemi (O1–O14) level tasarımından (H3–H12) **önce** bitmeli; O1 denetimi için A9 (legacy_levels.json) çıktısı gerekir. Engel prefab'ları G fazındaki ortam kitleriyle birlikte düşünülür.

Neden bu sıra: önce altyapı (veri güdümlü level + tek gameplay sahnesi) oturursa, 50 level ve ortam çalışmaları tekrar edilmez; UI ve ses de tek yerden yönetilir.
