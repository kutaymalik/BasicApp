using BasicApp.Data.Basemodel;

namespace BasicApp.Data.Domain;

public class User : BaseModel
{
    public string UserName { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpireDate { get; set; }
    public DateTime? DateOfBirth { get; set; } //ddmmyy
    public int? PasswordRetryCount { get; set; }
}