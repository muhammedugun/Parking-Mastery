# Parking Mastery: Oyun Tasarım Belgesi

*Sürüm: 1.0 | Motor: Unity 6 (Built-in pipeline) | Hedef: Yandex Games, WebGL (telefon + masaüstü)*

> **Bu belgenin amacı:** Oyunun nasıl olacağını netleştirmek ve geliştirme sırasında Claude'a aktarmak. Kararlaştırılmış yerler **[KARAR]**, sonra netleşecek yerler **[AÇIK]** ile işaretlidir. Yapılacaklar ve ilerleme `PROJE_DURUMU.md` içindedir, burada yoktur.

## 1. Oyuna Genel Bakış

**Tür:** Tek oyunculu, fizik tabanlı araba park etme oyunu. Oyuncu arabayı engellerin arasından geçirip park alanına doğru şekilde park eder.

**Tasarım ilkesi [KARAR]:** Oyun sade kalacak. Yeni bir şey eklemeden önce "bu gerçekten gerekli mi?" diye sorulacak.

**Kitle [KARAR]:** Yandex Games kitlesi ağırlıklı casual ve 35 yaş üstü. Bu yüzden arayüz sade ve okunaklı, zorluk adil ve kademeli, ilk dakika hızlı ve hatasız olmalı. Rusya ve BDT oyuncuların %60'ı, telefon kullanımı %50.

**Kapsam [KARAR]:**

- Tek araba modeli (yalnız renk değişir).
- **v1:** 30 level, 3 dünya (Şehir Otoparkı, Liman, Şantiye).
- **v2:** Level 31-50 + 2 dünya (Kış Yolu, Gece Şehri), güncelleme olarak eklenir.
- Para kazanma yalnız reklamdır. Uygulama içi satın alma yok.

**İçerik kuralı [KARAR]:** Gerçek marka/logo, siyaset, din, savaş, askeri araç ve şiddet yok. Araba kurgusal, plaka ve tabela yazıları kurgusal olur.

## 2. Oyun Akışı

1. Oyuncu ana menüden **Oyna**'ya basar, level seçim ekranı açılır.
2. Level seçer (yalnız açılmış olanlar oynanır).
3. Arabayı sürer, engellere çarpmadan park alanına girer.
4. Araba park alanında hizalı ve yeterince uzun süre durursa level kazanılır, süreye göre 1-3 yıldız verilir.
5. Sonraki level açılır. İlerleme her eylemden hemen sonra kaydedilir.

**Level seçim ekranı [KARAR]:** Yalnız var olan levelleri gösterir (sayı `LevelData` listesinden türetilir). "Yakında" kartı konmaz.

## 3. Kontroller [KARAR]

| Platform | Kontrol |
| --- | --- |
| Masaüstü | W/A/S/D ve ok tuşları, Space (el freni), R (aracı sıfırla). Fiziksel tuş konumuna göre çalışır (klavye düzeninden bağımsız). |
| Mobil | **Tek başparmak, dinamik joystick.** Ekranın alt yarısında dokunulan nokta joystick merkezi olur. Yatay = direksiyon, dikey = gaz/geri. Aşağı çekmek: hareket halindeyken fren, durunca geri vites. Bırakınca gaz kesilir. Ayrı fren butonu yok. |

- Pause, yeniden başlat ve kamera butonları ekranın üst köşelerinde durur, joystick ile çakışmaz.
- Mobilde "aracı sıfırla" butonu da olmalı.
- Mobil için eğim (accelerometer) kontrolü v1'de yok. Tek elle oynanabilirlik Yandex şartıdır, joystick bunu sağlar. His kötü çıkarsa eğim v2'ye ertelenir.

## 4. Araba ve Kamera

- Araba `Rigidbody` + `WheelCollider` ile sürülür. Gaz, fren, geri, yumuşatılmış direksiyon, hıza bağlı direksiyon ve el freni vardır.
- Çarpışma tepkisi: kamera sarsıntısı, ses, ceza.
- Kamera: takip, hıza göre zoom, park alanına yaklaşırken yakınlaşma. Mobilde FOV biraz geniş.
- Ayar değerleri koda gömülmez, Inspector'de durur. Sürüş hissi kötü çıkarsa yalnız şu değerler oynanır: `handbrakeRearGrip` (1 = kayma kapalı), `steerSpeedExponent` (1 = doğrusal), `steerReturnResponse`, kamera FOV/yaklaşma değerleri (0 = ilgili özellik kapalı).

## 5. Park ve Puanlama [KARAR]

- Kazanmak için araç **park alanının içinde, hizalı ve doğru yönde** olmalı, bekleme süresi dolmalı. Bekleme süresi tek yerde tanımlanır.
- Yön kontrolü sıkı olmalı (yaklaşık ≤ 30° sapma), 90° yan duran araç geçerli sayılmaz.
- Araçta birden fazla collider olsa da bekleme süresi tek sayılır (araç gövdesi üzerinden).
- Yıldız eşikleri `LevelData` içinde tutulur ve tek yerden hesaplanır. Süre başarı anında dondurulur.
- **Level hedef süresi 30-120 sn.** İlk level 1 dakikadan kısa sürede kazanılabilir. Toplam içerik 10 dakikadan uzun olmalı.

## 6. Dünyalar ve Zorluk

| Dünya | Level | Tema | Yeni mekanik | Sürüm |
| --- | --- | --- | --- | --- |
| 1 Şehir Otoparkı | 1-10 | Açık alandan dar geçide | Temel park, koni, duvar | v1 |
| 2 Liman | 11-20 | Konteyner labirenti | Paralel park, dar dönüş | v1 |
| 3 Şantiye | 21-30 | Sert zemin, engel yoğun | Hız tümseği, rampa, yavaş hareketli engel | v1 |
| 4 Kış Yolu | 31-40 | Kaygan zemin | Düşük sürtünme, uzun fren | v2 |
| 5 Gece Şehri | 41-50 | Karışık, usta seviyesi | Dar alan + zaman + az hata payı | v2 |

- Zorluk kademeli artar, ani sıçrama olmaz (özellikle 1-10 ve 25-30 arası kontrol edilir).
- 5 dünya ortak materyal paletini ve ortak prefab havuzunu paylaşır. Dünyaları ayıran şeyler: renk, ışık, zemin, ses, birkaç özgün prop.

## 7. Engel Sistemi [KARAR]

Her engel en az bir amaca hizmet eder: yolu daraltır, dikkat ister, beceri sınar veya ortam hissi verir. Sırf dolgu olan engel yoktur.

### 7.1 Katalog

| Kategori | Engeller | Davranış | Sürüm |
| --- | --- | --- | --- |
| Sabit | Duvar, kolon, bariyer, çit, konteyner, park etmiş araba, kaldırım | Çarpınca ceza, hareket etmez | v1 |
| Devrilebilir | Trafik konisi, varil, koli/palet yığını, çöp kutusu | Çarpınca devrilir, devirme ceza veya yıldız kaybı | v1 |
| Hareketli | Yavaş geçen araç, bariyer kolu, kayan kapı | Zamanlama gerektirir | v1 |
| Zemin | Hız tümseği, rampa | Hızı/sürtünmeyi değiştirir | v1 |
| Zemin (kaygan) | Buz, kar yığını, çamur, su birikintisi | Sürtünmeyi düşürür | v2 |
| Kural/Bölge | Yasak bölge, kontrol noktası, zorunlu geçiş kapısı | Sahaya kural koyar | v2 |

### 7.2 Tasarım kuralları

1. **Tek seferde tek yenilik:** Bir levelde en fazla 1 yeni engel tipi tanıtılır. Tanıtım leveli kolaydır, engel rahat alanda tek başına durur, ilk karşılaşmada ceza yok veya hafiftir.
2. **Kademeli darlık:** En dar geçit, arabanın genişliğinin yaklaşık 2 katından (Dünya 1) 1,25 katına (Dünya 5) kademeli iner.
3. **Adil olma:** Engelin ne olduğu görünüşünden anlaşılır. Sabit = gri/beton, devrilebilir = turuncu/parlak, hareketli = sarı-siyah şerit, kural bölgesi = zemin işareti. Görünmez veya haksız ceza veren engel olmaz. Ceza kuralı ikonla veya level başında anlatılır.
4. **Tekrar yok:** Aynı engel düzeni kopyalanmaz. Her level farklı bir soru sorar (dar geçit, dönüş, zamanlama, kayma).
5. **Performans bütçesi:** Level başına en fazla ~20 fizikli (Rigidbody) engel ve ~6 hareketli engel. Collider'lar basit olur (Box/Sphere/Capsule), mesh collider kullanılmaz. Tüm engel prefab'ları ortak paleti kullanır.
6. **Çarpma tespiti:** Engel tanıma tag yerine `ObstacleBase` bileşeniyle yapılır. Duvara sürtünmenin ceza sayılıp sayılmayacağı **[AÇIK]**, engel altyapısı kurulurken karar verilir.

### 7.3 Engellerin dünyalara dağılımı (taslak, engel denetiminden sonra kesinleşir)

| Dünya | Tanıtım sırası |
| --- | --- |
| 1 | Duvar, kolon, bariyer, kaldırım (L1-2), trafik konisi (L3), park etmiş araba (L5), çöp kutusu (L8) |
| 2 | Konteyner (L11), varil (L13), koli/palet yığını (L15) |
| 3 | Hız tümseği (L21), rampa (L23), yavaş geçen araç (L25), bariyer kolu (L27), kayan kapı (L29) |
| 4 (v2) | Buz (L31), kar yığını (L33), çamur/su (L35), kontrol noktası (L37) |
| 5 (v2) | Yasak bölge (L41), zorunlu geçiş kapısı (L44) |

## 8. Arayüz (UI)

- uGUI + TextMeshPro. Tek **UI tema ScriptableObject'i** (renk, font, buton stili). Her ekran ayrı prefab. Canvas referansı 1920×1080 + safe area.
- Ekranlar: Ana Menü, Level Seçim, Oyun İçi HUD, Pause, Kazanma, Kaybetme, Ayarlar, Dil, Krediler, Liderlik (isteğe bağlı).
- **HUD:** süre, yıldız hedefi, pause, yeniden başlat, kamera. Hiçbir en-boy oranında kesilmez veya üst üste binmez (16:9, 4:3, 20:9, dikey ve yatay).
- **Kazanma/Kaybetme:** yıldız animasyonu, sonraki level / tekrar / menü butonları.
- **Ayarlar:** müzik ve SFX ayrı kaydırıcı, dil, titreşim, grafik kalitesi.
- **Eğitim:** ilk 3 levelde kısa, atlanabilir ipuçları.
- **Diller [KARAR]:** RU (öncelik), EN, TR. Dil `ysdk.environment.i18n.lang` ile otomatik seçilir, desteklenmeyen dil EN'ye düşer. Oyuncu bayrak/dil adıyla elle değiştirebilir. Metinler merkezi bir tabloda tutulur, Kiril ve Türkçe karakterli font kullanılır.
- Metinler görsele gömülmez, yazı olarak kalır (yerelleştirme için).

## 9. Reklam [KARAR]

- **Level içinde reklam yok.**
- **Interstitial (tam ekran):** yalnız oyun dışı eylemden sonra (Sonraki, Tekrar, Menü, Level seç). Eylemle reklam arası ≤ 2 sn. Kendi bekleme aralığımız en az 60-90 sn.
- **Rewarded (ödüllü):** oyuncu ayrı butona kendi isteğiyle basar, buton reklam izleneceğini ve somut ödülü yazar (ek süre, ipucu, level atlama gibi). Devam etmenin şartı olamaz, **Tekrar dene ve sonraki level her zaman ücretsiz**. Şu an tek ödül "devam et" (`resume`).
- Reklam sırasında oyun ve ses durur, reklam sonrası ilerleme kaybolmaz.
- Reklam yalnız Yandex SDK üzerinden gösterilir, taklit reklam veya sahte buton yok. Gerekirse ek birim olarak yalnız sticky banner.

## 10. Görsel Stil

- **Temiz low-poly / stylized:** düşük poligon, tek palet, WebGL'de hızlı.
- Her dünya aynı materyal paletini paylaşır, farkı ışık, zemin rengi ve birkaç prop verir.
- Render: Built-in pipeline korunur (URP'ye geçilmez). Mobilde gölgesiz veya ucuz gölge, post-process mobilde varsayılan kapalı. Kalite seviyeleri Low/Medium/High, mobilde otomatik Low/Medium.
- Dekoratif objelerde statik batching / GPU instancing kullanılır, LOD yok veya az.
- Gökyüzü ve ışık presetleri: gündüz, gün batımı, gece.

## 11. Ses ve Müzik

- `AudioMixer` grupları: Master, Music, SFX, UI, Engine. Ayarlar kaydedilir.
- Tek `AudioService` + `SoundBank` (havuzlu AudioSource, aynı sesin üst üste binmesini engeller). Eski ses yöneticileri kaldırılır.
- **SFX:** UI tık, kazanma, kaybetme, yıldız, çarpma (hafif/sert), fren, korna, geri vites bipi.
- **Motor sesi:** hıza/RPM'e ve gaza bağlı pitch + volume döngüsü.
- **Müzik:** menü döngüsü + 3 oyun içi döngü, geçişte crossfade.
- **Ortam sesi:** dünya başına bir döngü (şehir uğultusu, liman/martı, şantiye vb.).
- **Boyut hedefi:** toplam ses ≤ ~8 MB.
- Sekme gizlenince ve reklam sırasında tüm ses kesilir.

## 12. Juice (Oyun Hissi)

- Park başarısı: ışık halkası, konfeti, kamera yavaşlaması, yıldız sesi.
- Çarpışma: kıvılcım/toz, kamera sarsıntısı, kısa titreşim (destekliyorsa), engel vurgu rengi/flaşı.
- Egzoz dumanı, tekerlek tozu/kar spreyi, fren izi.
- UI mikro animasyonları (buton basma, sayaç, yıldız doldurma).
- Sahne geçişlerinde fade + yükleme çubuğu.
- Mobilde sarsıntı ve parlama dozu abartılmaz.
