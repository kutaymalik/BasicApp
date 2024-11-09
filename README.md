# BasicApp Backend

Bu proje, BasicApp uygulaması için .NET Core kullanılarak geliştirilmiş bir API'dir. Kullanıcıların kayıt olması, giriş yapması, profil bilgilerini güncellemesi ve hesaplarını silmesi gibi işlemler için gereken API uç noktalarını sağlar.

## Başlangıç

### Gereksinimler

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- SQL Server veya başka bir veritabanı sunucusu
- Visual Studio veya Visual Studio Code (önerilir)

### Kurulum

1. Proje dosyasını bilgisayarınıza klonlayın:
    ```bash
    git clone https://github.com/username/BasicApp.git
    ```

2. Veritabanını oluşturun ve `appsettings.json` dosyasında bağlantı dizgisini yapılandırın:
    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=BasicAppDb;Trusted_Connection=True;"
    }
    ```

3. JWT ayarlarını `appsettings.json` dosyasında yapılandırın:
    ```json
    "JwtConfig": {
        "Secret": "YourJWTSecretKey",
        "AccessTokenExpiration": 30
    }
    ```

4. Gerekli bağımlılıkları yükleyin ve veritabanı migrasyonlarını çalıştırın:
    ```bash
    dotnet restore
    dotnet ef database update
    ```

### Çalıştırma

API'yi aşağıdaki komutla başlatabilirsiniz:
```bash
dotnet run

## Kullanılabilir API Uç Noktaları

| HTTP Yöntemi | Uç Nokta               | Açıklama                      |
|--------------|------------------------|--------------------------------|
| `POST`       | `/User/CreateUser`     | Yeni bir kullanıcı oluşturur   |
| `POST`       | `/User/Login`          | Kullanıcı girişi yapar         |
| `GET`        | `/User/{id}`           | Kullanıcı bilgilerini getirir  |
| `PUT`        | `/User/UpdateUser/{id}`| Kullanıcı bilgilerini günceller |
| `DELETE`     | `/User/DeleteUser/{id}`| Kullanıcı hesabını siler       |

## Kullanım Detayları

### 1. Kullanıcı Oluşturma
- **Endpoint**: `/User/CreateUser`
- **Method**: `POST`
- **Açıklama**: Yeni bir kullanıcı oluşturur.

### 2. Kullanıcı Girişi
- **Endpoint**: `/User/Login`
- **Method**: `POST`
- **Açıklama**: Kullanıcı giriş işlemi yapar ve bir token döner.

### 3. Kullanıcı Bilgilerini Getirme
- **Endpoint**: `/User/{id}`
- **Method**: `GET`
- **Açıklama**: Belirtilen `id`'ye sahip kullanıcının bilgilerini getirir.

### 4. Kullanıcı Bilgilerini Güncelleme
- **Endpoint**: `/User/UpdateUser/{id}`
- **Method**: `PUT`
- **Açıklama**: Belirtilen `id`'ye sahip kullanıcının bilgilerini günceller.

### 5. Kullanıcı Hesabını Silme
- **Endpoint**: `/User/DeleteUser/{id}`
- **Method**: `DELETE`
- **Açıklama**: Belirtilen `id`'ye sahip kullanıcı hesabını siler.

---

Bu API uç noktaları, BasicApp'in kullanıcı yönetim işlemleri için kullanılmaktadır. Her bir uç nokta için daha detaylı bilgiye ulaşmak için gerekli dokümantasyona veya proje detaylarına bakabilirsiniz.

