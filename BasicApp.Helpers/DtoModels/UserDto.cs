using BasicApp.Helpers.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BasicApp.Helpers.DtoModels;

public class UserRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
}

public class UserResponse
{
    public int Id {  get; set; }
    public bool isActive { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpireDate { get; set; }
    public int? PasswordRetryCount { get; set; }
}