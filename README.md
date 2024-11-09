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
