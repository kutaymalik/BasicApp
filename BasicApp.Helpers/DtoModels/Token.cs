namespace BasicApp.Helpers.DtoModels;

public class TokenRequest
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public class TokenResponse
{
    public DateTime ExpireDate { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public int Id { get; set; }
}
