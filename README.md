# FurnishAR

**Sefa Keskin - 2022555043**

Android cihazlar için Unity ve AR Foundation kullanılarak geliştirilmiş artırılmış gerçeklik (AR) tabanlı mobilya yerleştirme uygulaması. Kamera aracılığıyla odadaki yatay zeminler algılanır ve seçilen koltuk modelleri gerçek boyutlarıyla ortama yerleştirilir.

## Özellikler

- Yatay zemin algılama ve ızgara görünümü
- 3 farklı koltuk modeli arasından seçim yapabilme
- Sahneye birden fazla mobilya ekleyebilme
- Yerleştirilen mobilyayı dokunarak seçme, sürükleyerek taşıma, döndürme ve silme
- ARCore Depth API ile derinlik algılama (Occlusion) desteği

## Kullanılan Teknolojiler

- Unity 6 (6000.0.85f1)
- AR Foundation 6.7 & Google ARCore XR Plugin 6.7
- C#

## Kurulum ve Çalıştırma

1. Projeyi Unity Hub üzerinden **6000.0.85f1** sürümü ile açın.
2. `Assets/Scenes/FurnishAR.unity` sahnesini açın.
3. **File -> Build Profiles** (veya Build Settings) menüsünden Android platformunun seçili olduğundan emin olun.
4. **Build** butonuna tıklayarak APK çıktısını alın (hazır derlenmiş APK dosyası `Builds/FurnishAR.apk` konumunda da bulunmaktadır).

## Klasör Yapısı

- `Assets/Scenes` : Ana uygulama sahnesi (`FurnishAR.unity`)
- `Assets/Scripts` : Mobilya yerleştirme, seçim ve arayüz kontrol scriptleri
- `Assets/Shaders` : Zemin ızgarası ve derinlik (occlusion) shader dosyaları
- `Assets/Materials` : Sahne ve arayüz materyalleri
- `Assets/Prefabs` : AR zemin prefabı
- `Assets/Assets` : 3B koltuk modelleri ve arayüz görselleri
