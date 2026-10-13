# Parking Mastery: Proje Durumu ve Yapılacaklar

*Belge sürümü: 2.0 | Motor: Unity 6 (Built-in pipeline) | Hedef: Yandex Games, WebGL*

## ⚠️ Claude için çalışma kuralları (her oturumda oku)

1. **Neyi okuyacağın:** Önce bu dosyayı oku. Sonra yalnızca yapacağın adımla ilgili bölümü aç: oyun kuralları için `01_oyun_tasarimi.md`, kod yapısı / Yandex kuralları / asset yöntemi için `02_teknik_ve_yandex.md`.
2. **Kodu dosya olarak verme, doğrudan Unity projesine ekle**
3. **Bir iş bittiğinde biten adımı bu dosyada `[x]` yap.
4. **Asset kuralı:** Her üçüncü taraf dosya için `CREDITS.md`'ye satır ekle (kaynak, lisans).

Not: B ve C'de engeller/leveller "gri kutu" (sade materyal) hâlinde yapılır, görsel giydirme D'de olur. Her adım tek oturumluk iştir.

---

## 0. Teknik temizlik
- [ ] 0a. Olay sistemi: eski anahtarlı olayları tipli olaylara çevir (`EventType` → `GameEventType`), eskiler kalkar. Derleme + konsol temiz.
- [ ] 0b. Yerel yedek kaydına sürüm alanı ekle (eski kayıt bozulmasın).
- [ ] 0c. `MaxLevels` tek yerde tutulsun (LevelData sayısından türesin).
- [ ] 0d. Kullanılmayan eski yıldız eşiği alanlarını ve `Brake` düğme adını temizle.

## A. Araç fiziği ve sürüş hissi
- [ ] A1. Rampa/eğim: tutma, kayma eşikleri, tümsekte zıplama sınırı.
- [ ] A2. Park algılama toleransı: konum/açı eşiği, bekleme süresi, "az kaldı" durumu (hizalama geri bildirimi).
- [ ] A3. Araç ölçüleri (uzunluk, genişlik, dönüş çapı) not edilsin: level boşluk hesabı için `LEVEL_DESIGN.md`'ye yazılır.
- [ ] A4. Mobilde "aracı sıfırla" butonu (takılma durumu için).

## B. Engel sistemi (01 §7)
- [ ] B2. Engel kataloğu: ID, kategori, boyut, ceza kuralı, hangi dünyada tanıtılır (`OBSTACLE_CATALOG.md`).
- [ ] B3. Engel temel bileşeni + engel verisi (ScriptableObject). Çarpma tespiti tag yerine bu bileşenle.
- [ ] B4. Çarpışma tepkisi + ceza kuralı bağlantısı, Physics Layers matrisi.
- [ ] B5. Sabit engel prefab'ları (gri kutu): duvar, kolon, bariyer, çit, kaldırım, konteyner (Box collider).
- [ ] B6. Park etmiş araba prefab'ları: 3-5 renk/model, tek mesh, ucuz collider.
- [ ] B7. Fizikli engeller: koni, varil, koli, çöp kutusu. Rigidbody, uyku modu, devrilme algılama.
- [ ] B8. Hareketli engel altyapısı: yol noktaları, hız, bekleme, deterministik zamanlama.
- [ ] B9. Hareketli engel prefab'ları: yola göre giden araç, bariyer kolu, kayan kapı.
- [ ] B10. Zemin engelleri (v1: yalnız hız tümseği + rampa), rampa eşikleri A6 ile uyumlu.
- [ ] B11. Engel geri bildirimi: çarpışma renk/flaşı, uyarı ikonu, yıldız kaybı kuralı (ses E'de eklenir, buraya yer tutucu bırak).
- [ ] B12. Engel yerleştirme aracı (Editor): grid ve çakışma kontrolü.
- [ ] B13. Yerleştirme aracına minimum geçiş genişliği uyarısı (A8 ölçüleriyle).
- [ ] B14. Engel test sahnesi: hepsi yan yana, çarpma ve performans kontrolü.

## C. Level tasarımı (01 §5-6, v1: Level 1-30)
- [ ] C1. Level Editor aracı: LevelData'dan sahne düzeni üretme.
- [ ] C2. Editor'e oynanabilirlik doğrulaması: hedefe ulaşılabilir mi, boşluk ölçüsü (araç ölçüleri prefab'dan okunur).
- [ ] C3. Zorluk/süre/yıldız kuralları (`LEVEL_DESIGN.md`): süre formülü, yıldız eşikleri, hedef süre 30-120 sn, ilk level 1 dakikadan kısa.
- [ ] C4. Engel-level dağıtım matrisi (`OBSTACLE_PROGRESSION.md`): her levelde hangi engeller, yeni engel nerede tanıtılır, engel sayısı bütçesi.
- [ ] C5. Level 1-3 tasarla ve test et (en basit, öğretici).
- [ ] C6. Level 4-6
- [ ] C7. Level 7-9
- [ ] C8. Level 10-12
- [ ] C9. Level 13-15
- [ ] C10. Level 16-18
- [ ] C11. Level 19-21
- [ ] C12. Level 22-24
- [ ] C13. Level 25-27
- [ ] C14. Level 28-30
- [ ] C15. Her yeni engel için tanıtım leveli kontrolü (tek başına, rahat alanda, ilk karşılaşma güvenli); eksik olanlar düzeltilir.
- [ ] C16. Tüm leveller için otomatik doğrulama testi (spawn, hedef, süre, yıldız, Missing Reference yok).
- [ ] C17. Zorluk eğrisi gözden geçirme (özellikle 1-10 ve 25-30 arası sıçramalar).
- [ ] C18. Erken WebGL duman testi (yayınlanmayan): tek level açılıyor mu, boyut/FPS ölçümü, sonuçlar `02` Bölüm 2.1'e yazılır. (Erken risk avı.)

## D. Ortam tasarımı (01 §10, v1: 3 dünya)
Yöntem (ProBuilder / CC0 / Blender) `02` Bölüm 3'e göre seçilir, seçim adımın notuna yazılır.
- [ ] D1. Render kararları: Gamma/Linear kararı (şu an Linear), ışık yönü, mobilde ucuz gölge, fog.
- [ ] D2. Kalite seviyeleri Low/Medium/High tanımı.
- [ ] D3. Materyal paleti (renk tablosu, materyaller).
- [ ] D4. Prosedürel texture seti: asfalt, çizgili otopark, çimen.
- [ ] D5. Prosedürel texture seti: beton, metal, su.
- [ ] D6. Dünya 1 (Şehir Otoparkı) yapı elemanları: kolon, duvar, bariyer, park çizgileri.
- [ ] D7. Dünya 1 süs/prop: koni, çöp kutusu, lamba, park etmiş arabalar görselleri.
- [ ] D8. Dünya 1 sahne giydirmesi (level 1-10 sahnelerine uygula, görsel kontrol).
- [ ] D9. Dünya 2 (Liman) yapı elemanları: konteyner, palet, deniz.
- [ ] D10. Dünya 2 siluetler: vinç + sahne giydirmesi (level 11-20).
- [ ] D11. Dünya 3 (Şantiye) elemanları: kum yığını, çit, uyarı bantları.
- [ ] D12. Dünya 3 silueti: iş makinesi + sahne giydirmesi (level 21-30).
- [ ] D13. Gökyüzü ve ışık presetleri (gündüz, gün batımı, gece), dünyalara atama.
- [ ] D14. Batching / instancing kuralı ve draw call bütçesi.
- [ ] D15. Eski Road/Prototype modelleri ve eski sahneler: silinebilir mi kararı, temizlik.
- [ ] D16. Post-process (hafif, kapatılabilir, mobilde varsayılan kapalı): maliyeti ölçerek karar.

## E. Ses ve müzik (01 §11)
- [ ] E1. AudioMixer: Master / Music / SFX / UI / Engine grupları.
- [ ] E2. Tek ses servisi + SoundBank (havuzlu, tekrar engelli). Eski ses yöneticileri kalkar.
- [ ] E3. Ses ayarlarının kayda bağlanması (müzik/SFX seviyeleri mixer'a).
- [ ] E4. Prosedürel SFX üretici Editor aracı (iskelet).
- [ ] E5. UI SFX: tık, kazanma, kaybetme, yıldız.
- [ ] E6. Oyun SFX: çarpma, fren, korna, geri vites bipi.
- [ ] E7. Motor sesi: hıza/gaza bağlı pitch + volume.
- [ ] E8. Çarpma sesi engel türüne göre ayrımı (B11 yer tutucusu doldurulur).
- [ ] E9. Müzik: menü müziği + crossfade altyapısı.
- [ ] E10. Oyun içi 3 müzik döngüsü.
- [ ] E11. Ortam sesleri: dünya başına bir döngü.
- [ ] E12. Ses boyutu: toplam ≤ ~8 MB (şu an iki büyük müzik WAV'ı 42,7 MB), Vorbis, kısa SFX mono.

## F. UI tasarımı (01 §8)
- [ ] F1. Level Seçim: level sayısı LevelData'dan, açık/kilitli/yıldız gösterimi.
- [ ] F2. Level Seçim: dünya/sayfa sekmeleri + kaydırma, Ana menüdeki **Oyna** buna bağlanır.
- [ ] F3. Oyun içi HUD: temel yerleşim (süre, kalan çarpma hakkı), tüm en-boy oranlarında kesilmez.
- [ ] F4. HUD: mobil butonlar joystick ile çakışmaz, masaüstü ipucu, en iyi süre.
- [ ] F5. Pause penceresi (devam / tekrar / menü / ayarlar).
- [ ] F6. Kazanma penceresi: yıldız animasyonu, sayaç, sonraki level / tekrar / menü.
- [ ] F7. Kaybetme penceresi + ödüllü reklam butonu yerleşimi (somut ödül yazılır).
- [ ] F8. Eski ekran yöneticileri ve sahnelerdeki eski sahne yükleyici kopyalarını sil, Android geri tuşu `UIManager.Back()`'e bağla.
- [ ] F9. Ayarlar paneli: müzik/SFX ayrı kaydırıcı (E3 ile bağlı), titreşim.
- [ ] F10. Ayarlar: dil seçimi ve grafik kalitesi (D2 ile bağlı).
- [ ] F11. Yerelleştirme: merkezi metin tablosu, eski çeviri bileşeni kalkar, dil değişince ekran güncellenir.
- [ ] F12. Font: Kiril + Türkçe karakterler, TMP WebGL denemesi, dil değiştirme bayrak/dil adıyla.
- [ ] F13. Çözünürlük testleri: 16:9, 4:3, 20:9 (dikey ve yatay) + safe area.

## G. Yandex entegrasyonu
- [ ] G1. SDK yaşam döngüsü: dil otomatik seçimi, `LoadingAPI.ready()` oynanabilir ilk anda (eklentinin otomatik ready'si kapatılır).
- [ ] G2. `GameplayAPI.start/stop`: level başı/sonu/pause'da, pause/resume olayları.
- [ ] G3. Sekme gizlenince / odak kaybında ses ve oyunu durdur (sık red nedeni).
- [ ] G4. Interstitial: yalnız oyun dışı eylem sonrası + bekleme süresi.
- [ ] G5. Rewarded: ayrı buton, ödül kimliği sabit, işleyicilerde kontrol.
- [ ] G6. Reklam sırasında oyun+ses duraklar, çift tıklamada art arda reklam engellenir. Draft ortamında gerçek reklamla test.
- [ ] G7. Bulut kaydı: gerçek Yandex kaydıyla yükleme/geri yazma.
- [ ] G8. Kayıt çakışması (bulut ≠ yerel), yenile/döndürmede kayıp yok, misafir ilerlemesi, offline davranış.
- [ ] G9. Liderlik tablosu (isteğe bağlı): giriş yalnız oyuncu bilinçli butona basarsa istenir, girmeyen hiçbir şeyden engellenmez.
- [ ] G10. Yandex'in güncel gereksinim sayfalarını yeniden kontrol et (gereksinimler, reklam yerleşimi, SDK, değişiklik günlüğü), `02` Bölüm 2'yi güncelle.

## H. Cilalama (polish)
- [ ] H1. Park başarı efekti (konfeti/ışık).
- [ ] H2. Çarpışma efekti.
- [ ] H3. Egzoz / toz / fren izi.
- [ ] H4. UI mikro animasyonları (buton, pencere açılış/kapanış).
- [ ] H5. Sahne geçişi + yükleme çubuğu.
- [ ] H6. Eğitim: ilk 3 levelde kısa, atlanabilir ipuçları.
- [ ] H7. Bellek ve GC taraması.
- [ ] H8. Nesne havuzu (efekt/ses).
- [ ] H9. Kalite profilleri (mobil → Low/Medium varsayılan).

## I. Testler
- [ ] I1. Masaüstü tam oynanış testi: Level 1-15.
- [ ] I2. Masaüstü tam oynanış testi: Level 16-30.
- [ ] I3. Mobil joystick doğrulaması: gerçek telefonda tek elle (yatay + dikey, "aşağı çekince fren / durunca geri" hissi). His kötüyse eğim seçeneği v2'ye ertelenir.
- [ ] I4. Mobil tarayıcıda Level 1-30 gezintisi (performans, dokunma, ekran dönmesi).
- [ ] I5. Reklam akışı testi (interstitial + rewarded, ses/oyun duraklıyor mu).
- [ ] I6. Kayıt/yükleme testi (yenile, tarayıcı kapat/aç, misafir).
- [ ] I7. `02` Bölüm 2.2 listesi baştan sona + WebGL şablon testleri.
- [ ] I8. Hata düzeltme turu 1 (`KNOWN_ISSUES.md` oluştur ve doldur).
- [ ] I9. Hata düzeltme turu 2 (yeniden test, liste boşalana kadar).

## J. Build
- [ ] J1. WebGL ayarları: Brotli, kod kırpma (High), texture/audio import ayarı. Not: PluginYourGames build sırasında sıkıştırmayı (Brotli) ve kod kırpmayı (Minimal) kendi ayarına çeker (koddan okundu, build'le doğrulanmadı). Değiştirmek için eklentinin ayar anahtarları kullanılır.
- [ ] J2. Boş paket/shader temizliği.
- [ ] J3. Build al, boyut / FPS / açılış süresi ölç (hedef: ~40 MB sıkıştırmasız, ~15 MB Brotli).
- [ ] J4. Eski cihaz (WebGL2) ve reklam akışı denemesi.

## K. Publish (v1 yayını)
- [ ] K1. Oyun adı benzersizlik araması, benzer oyun incelemesi.
- [ ] K2. İkon ve kapak görseli.
- [ ] K3. Oyundan ekran görüntüleri.
- [ ] K4. RU/EN/TR açıklama + "How to play" metni.
- [ ] K5. Son build kontrolü: `index.html` arşiv kökünde, adlarda boşluk/Rusça karakter yok, ≤ 100 MB, kodda mutlak URL yok (arama ile tara).
- [ ] K6. Farklı adreste açılış testi.
- [ ] K7. Yandex taslağına yükle: bulut kayıt, yönelim, platform, yaş derecelendirmesi. **v1 yayını.**

## L. v2: Level 31-50 (güncelleme)
v1 yayınlandıktan sonra, oyuncu verisine bakarak başlanır. Ana konsept korunur.
- [ ] L1. Kış Yolu kiti (kar, çam, buz zemin, sis).
- [ ] L2. Gece Şehri kiti (gece ışığı, lamba, hafif bloom).
- [ ] L3. Yeni zemin engelleri: buz, kar, çamur, su.
- [ ] L4. Kural bölgeleri: yasak bölge, kontrol noktası, zorunlu kapı.
- [ ] L5. Engel dağıtımı ve tanıtım levelleri (31-50).
- [ ] L6. Level 31-35
- [ ] L7. Level 36-40
- [ ] L8. Level 41-45
- [ ] L9. Level 46-50
- [ ] L10. Doğrulama testi ve zorluk eğrisi (50 level), level seçim ekranı güncellemesi.
- [ ] L11. 50 level tam test, mağaza metinleri ve ekran görüntülerini güncelle, yayın kontrol listesini tekrarla.
