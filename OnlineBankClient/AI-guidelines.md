# AI Destekli Geliştirme Rehberi

Bu dosya, proje üzerinde yapılacak sonraki gelişmeler için AI destekli çalışma prensiplerini ve kodlama standartlarını içerir.

## 1. Amaç
- Mevcut `home` bileşeni üzerinde arka plan görseli düzenlemeleri yapıldı.
- Gelecekteki talepleri izlemek ve sürüm notlarını tutmak için stabil bir kaynak.

## 2. Proje yapısı
- `src/app/components` içinde bileşenler var.
- Stil değişiklikleri `*.component.css` içinde yönetiliyor.
- Bileşen mantığı `*.component.ts` ve HTML `*.component.html` içinde.

## 3. Stil/UX kuralları
- Arka plan görselleri `hero-section` içinde pseudo-elementlerle yapılıyor.
- `z-index` ile içerik her zaman önde tutulmalı.
- Geçişli renkler ve baloncuk efektleri `radial-gradient` ile verilmiş.

## 4. Kod yazım kuralları
- Temiz ve kısa bir CSS için mümkün olduğunca tekrar etmeden çoklu gradient kullan.
- Bootstrap sınıfları (`d-flex`, `text-center`) var; bunlar korunmalı.

## 5. Gelecek adımlar
- Baloncuk animasyonu: `@keyframes` ve `animation` kullanıp `hero-section::after` içine eklenebilir.
- Tema özelleştirme için sunucu tarafı renk değişkeni (kapsül) eklenebilir.

## 6. AI'dan istenebilecekler
- Yeni bileşen arka plan tasarımları (ilgili metin bozulmadan)
- Performans iyileştirmesi: gereksiz gradient sayısını azaltma
- Eksik responsive davranışlar için mobil optimizasyon
