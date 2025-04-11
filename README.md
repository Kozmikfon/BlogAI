# 🤖 AI Blog Sistemi -Future

Yapay zeka destekli modern blog üretim platformu. İçerikler GPT-3.5 tarafından günüb belirli saatlerinde otomatik olarak üretilir ve veritabanına eklenir. Kullanıcılar içerikleri okuyabilir ve yorum yazabilir. Admin paneli üzerinden blog ve yorum yönetimi yapılabilir.

---

## 🚀 Özellikler

### ✅ Otomatik İçerik Üretimi
- Günde 2 defa otomatik olarak içerik üretir (00:00 ve 02:00)
- Kategoriler: Teknoloji, Bilim, Sağlık, Yapay Zeka, Girişimcilik
- OpenAI API (GPT-3.5) ile içerik oluşturulur

### 🌐 Kullanıcı Arayüzü (React)
- Blog listeleme ve detay görüntüleme
- Okuma süresi tahmini ve ilerleme çubuğu
- Yorum ekleme ve listeleme
- Blog paylaşma (WhatsApp, Twitter, Kopyala)
- Tema desteği (Dark/Light)

### 🛠️ Admin Panel
- Admin JWT ile giriş yapar
- Blog düzenleme, silme
- Yorum silme
- İstatistikler: toplam blog, son içerik zamanı

### 📈 Ekstra Özellikler
- Hangfire ile zamanlanmış görevler
- PostgreSQL veritabanı kullanımı

---

## 📦 Kullanılan Teknolojiler

### Backend:
- ASP.NET Core 8.0 Web API
- Entity Framework Core + PostgreSQL
- Hangfire (Zamanlanmış görevler)
- OpenAI GPT-3.5 (İçerik üretimi)
- JWT Authentication

### Frontend:
- React.js (Vite + Bootstrap)
- React Router
- Axios
- React Icons

---

## 🛠️ Kurulum ve Başlatma

### 1. Backend (ASP.NET Core API)

```bash
cd BlogProject
dotnet restore
dotnet ef database update
dotnet run
