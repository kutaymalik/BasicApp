using BasicApp.Data.UOW;
using BasicApp.Data.Domain;
using BasicApp.Helpers.SharedModels;
using Microsoft.AspNetCore.Mvc;
using BasicApp.Helpers.DtoModels;
using BasicApp.Helpers.Encryption;
using BasicApp.Helpers.Enum;
using BasicApp.Helpers.JWT;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BasicApp.Helpers.Token;
using Microsoft.Extensions.Options;
using BasicApp.Helpers.Session;

namespace BasicApp.API.Controllers;

[Route("BasicApp/api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtHelper _jwtHelper;
    private readonly JwtConfig _jwtConfig;
    private readonly ISessionService _sessionService;

    public UserController(IUnitOfWork unitOfWork, JwtHelper jwtHelper, IOptionsMonitor<JwtConfig> jwtConfig, ISessionService sessionService)
    {
        _unitOfWork = unitOfWork;
        _jwtHelper = jwtHelper;
        _jwtConfig = jwtConfig.CurrentValue;
        _sessionService = sessionService;
    }

    [HttpPost("CreateUser")]
    public async Task<Response<UserResponse>> CreateUser([FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = _unitOfWork.UserRepository.FirstOrDefault(u => u.Email == request.Email || u.UserName == request.UserName);

        if (existingUser != null)
        {
            return new Response<UserResponse>
            {
                isSuccessful = false,
                Message = "Email is already registered.",
                StatusCode = System.Net.HttpStatusCode.Conflict // 409 Conflict for existing email
            };
        }

        // Hash the password before saving
        var hashedPassword = Md5.Create(request.Password);

        // Create a new user entity
        var user = new User
        {
            UserName = request.UserName,
            Password = hashedPassword,
            Role = UserType.admin.ToString(),
            Email = request.Email,
            InsertDate = DateTime.UtcNow,
            CreatedById = 1,
            IsActive = true
        };

        await _unitOfWork.UserRepository.InsertAsync(user, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken);

        // Prepare response
        var response = new UserResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            PasswordRetryCount = user.PasswordRetryCount,
            Password = hashedPassword,
        };

        return new Response<UserResponse>
        {
            isSuccessful = true,
            Data = response,
            StatusCode = System.Net.HttpStatusCode.Created // 201 Created for new resource
        };
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin, user")]
    public async Task<Response<UserResponse>> GetById(int? id)
    {
        var userIdFromToken = _sessionService.CheckSession().sessionId;
        var userRoleFromToken = _sessionService.CheckSession().sessionRole;

        if (userIdFromToken == null)
        {
            return new Response<UserResponse>
            {
                isSuccessful = false,
                Message = "Unauthorized access!",
                StatusCode = System.Net.HttpStatusCode.Unauthorized // 401 Unauthorized
            };
        }

        if (userIdFromToken != id)
        {
            return new Response<UserResponse>
            {
                isSuccessful = false,
                Message = "Unauthorized access!",
                StatusCode = System.Net.HttpStatusCode.Forbidden // 403 Forbidden for mismatched ID
            };
        }

        User user = new User();

        if (userRoleFromToken == "user")
        {
            var userId = _sessionService.CheckSession().sessionId;
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == userId && u.IsActive == true);
        }

        if (userRoleFromToken == "admin")
        {
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == id && u.IsActive == true);
        }

        if (user == null)
        {
            return new Response<UserResponse>
            {
                isSuccessful = false,
                Message = "User not found!",
                StatusCode = System.Net.HttpStatusCode.NotFound // 404 Not Found for missing user
            };
        }

        var response = new UserResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            PasswordRetryCount = user.PasswordRetryCount,
            Password = user.Password,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpireDate = user.RefreshTokenExpireDate
        };

        return new Response<UserResponse>
        {
            isSuccessful = true,
            Data = response,
            StatusCode = System.Net.HttpStatusCode.OK // 200 OK for successful retrieval
        };
    }

    [HttpGet("GetAllUsers")]
    [Authorize(Roles = "admin")]
    public async Task<Response<List<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
    {
        // Check if users exist
        var users = _unitOfWork.UserRepository.Where(x => x.IsActive == true).ToList();

        if (users == null || users.Count == 0)
        {
            return new Response<List<UserResponse>>
            {
                isSuccessful = false,
                Message = "No users found!",
                StatusCode = System.Net.HttpStatusCode.NotFound // 404 Not Found when no users exist
            };
        }

        var responseList = new List<UserResponse>();
        foreach (var user in users)
        {
            var response = new UserResponse
            {
                
                UserName = user.UserName,
                Email = user.Email,
                PasswordRetryCount = user.PasswordRetryCount,
                Password = user.Password,
                Role = user.Role,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpireDate = user.RefreshTokenExpireDate
            };
            responseList.Add(response);
        }

        return new Response<List<UserResponse>>
        {
            isSuccessful = true,
            Data = responseList,
            StatusCode = System.Net.HttpStatusCode.OK // 200 OK for successful retrieval
        };
    }

    [HttpPost("Login")]
    public async Task<Response<LoginResponse>> Login([FromBody] Helpers.DtoModels.LoginRequest request, CancellationToken cancellationToken)
    {
        User user = _unitOfWork.UserRepository.FirstOrDefault(x => x.Email == request.Email && x.IsActive == true);

        if (user == null)
        {
            return new Response<LoginResponse>
            {
                Message = "User information is incorrect!",
                isSuccessful = false,
                StatusCode = System.Net.HttpStatusCode.Unauthorized // 401 Unauthorized for invalid login
            };
        }

        if (user.PasswordRetryCount >= 5)
        {
            return new Response<LoginResponse>
            {
                StatusCode = System.Net.HttpStatusCode.Forbidden, // 403 Forbidden for locked account
                isSuccessful = false,
                Message = "You entered your password incorrectly 5 times. Please contact the admin to open your account!"
            };
        }

        var authPass = Md5.Create(request.Password).ToLower();

        if (user.Password != authPass)
        {
            user.PasswordRetryCount++;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return new Response<LoginResponse>
            {
                Message = "User information is incorrect!",
                isSuccessful = false,
                StatusCode = System.Net.HttpStatusCode.Unauthorized // 401 Unauthorized for incorrect password
            };
        }

        if (!user.IsActive)
        {
            return new Response<LoginResponse>
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized, // 401 Unauthorized for inactive user
                isSuccessful = false,
                Message = "User account is inactive."
            };
        }

        string token = _jwtHelper.Token(user);

        TokenResponse tokenResponse = new()
        {
            Token = token,
            ExpireDate = DateTime.Now.AddMinutes(_jwtConfig.AccessTokenExpiration),
            Email = user.Email,
            RefreshToken = _jwtHelper.CreateRefreshToken(),
            UserName = user.UserName,
            Role = user.Role,
            Id = user.Id
        };

        user.RefreshToken = tokenResponse.RefreshToken;
        user.RefreshTokenExpireDate = tokenResponse.ExpireDate;
        user.PasswordRetryCount = 0;

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        LoginResponse loginResponse = new LoginResponse
        {
            User = user,
            Token = tokenResponse.Token
        };

        return new Response<LoginResponse>
        {
            isSuccessful = true,
            StatusCode = System.Net.HttpStatusCode.OK, // 200 OK for successful login
            Data = loginResponse
        };
    }

    [HttpDelete("DeleteUser/{id}")]
    [Authorize(Roles = "admin, user")]
    public async Task<Response<string>> DeleteUser(int? id, CancellationToken cancellationToken)
    {
        User user = new User();
        if(_sessionService.CheckSession().sessionRole == "user")
        {
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == _sessionService.CheckSession().sessionId && u.IsActive == true);
        }

        if (_sessionService.CheckSession().sessionRole == "admin")
        {
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == id && u.IsActive == true);
        }

        if (user == null)
        {
            return new Response<string>
            {
                isSuccessful = false,
                Message = "User not found!",
                StatusCode = System.Net.HttpStatusCode.NotFound
            };
        }

        _unitOfWork.UserRepository.Delete(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new Response<string>
        {
            isSuccessful = true,
            Message = "User deleted successfully.",
            StatusCode = System.Net.HttpStatusCode.OK
        };
    }

    [HttpPut("UpdateUser/{id}")]
    [Authorize(Roles = "admin, user")]
    public async Task<Response<UserResponse>> UpdateUser(int? id, [FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        User user = new User();
        if (_sessionService.CheckSession().sessionRole == "user")
        {
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == _sessionService.CheckSession().sessionId && u.IsActive == true);
        }

        if (_sessionService.CheckSession().sessionRole == "admin")
        {
            user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == id && u.IsActive == true);
        }

        if (user == null)
        {
            return new Response<UserResponse>
            {
                isSuccessful = false,
                Message = "User not found!",
                StatusCode = System.Net.HttpStatusCode.NotFound
            };
        }

        // Yalnızca null olmayan alanları güncelle
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user.UserName = request.UserName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.Password = Md5.Create(request.Password);
        }

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

        var response = new UserResponse
        {
            UserName = user.UserName,
            Email = user.Email,
            PasswordRetryCount = user.PasswordRetryCount,
            Password = user.Password,
            Role = user.Role,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpireDate = user.RefreshTokenExpireDate
        };

        return new Response<UserResponse>
        {
            isSuccessful = true,
            Data = response,
            StatusCode = System.Net.HttpStatusCode.OK
        };
    }
}