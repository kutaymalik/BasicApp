using BasicApp.Data.Domain;

namespace BasicApp.Helpers.DtoModels;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public User User { get; set; }
    public string Token { get; set; }
}
