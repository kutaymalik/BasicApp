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

namespace BasicApp.API.Controllers;

[Route("BasicApp/api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtHelper _jwtHelper;
    private readonly JwtConfig jwtConfig;

    public UserController(IUnitOfWork unitOfWork, JwtHelper jwtHelper, IOptionsMonitor<JwtConfig> jwtConfig)
    {
        _unitOfWork = unitOfWork;
        _jwtHelper = jwtHelper;
        this.jwtConfig = jwtConfig.CurrentValue;
    }

    [HttpPost("CreateUser")]
    public async Task<Response<UserResponse>> CreateUser([FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = _unitOfWork.UserRepository.FirstOrDefault(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return new Response<UserResponse> { isSuccessful = false, Message = "Email is already registered." };
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

        return new Response<UserResponse> { isSuccessful = true, Data = response };
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<Response<UserResponse>> GetById(int id)
    {
        var userIdFromToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userRoleFromToken = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (userIdFromToken == null)
        {
            return new Response<UserResponse> { isSuccessful = false, Message = "Unauthorized access!" };
        }

        if (userIdFromToken != id.ToString() && userRoleFromToken != "admin")
        {
            return new Response<UserResponse> { isSuccessful = false, Message = "Unauthorized access!" };
        }

        var user = _unitOfWork.UserRepository.FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return new Response<UserResponse> { isSuccessful = false, Message = "User not found!" };
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

        return new Response<UserResponse> { isSuccessful = true, Data = response };
    }


    [HttpGet("GetAllUsers")]
    [Authorize(Roles = "admin")]
    public async Task<Response<List<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
    {
        // Check if user already exists
        var users = _unitOfWork.UserRepository.GetAll().ToList();

        if (users == null || users.Count == 0)
        {
            return new Response<List<UserResponse>> { isSuccessful = false, Message = "No users found!" };
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
                RefreshToken = user.RefreshToken,
                RefreshTokenExpireDate = user.RefreshTokenExpireDate
            };
            responseList.Add(response);
        }

        return new Response<List<UserResponse>> { isSuccessful = true, Data = responseList };
    }

    [HttpPost("Login")]
    public async Task<Response<LoginResponse>> Login([FromBody] Helpers.DtoModels.LoginRequest request, CancellationToken cancellationToken)
    {
        User user = _unitOfWork.UserRepository.FirstOrDefault(x => x.Email == request.Email);

        if (user == null)
        {
            return new Response<LoginResponse>
            {
                Message = "User information is incorrect!",
                isSuccessful = false,
                StatusCode = System.Net.HttpStatusCode.Unauthorized
            };
        }

        if (user.PasswordRetryCount >= 5)
        {
            return new Response<LoginResponse>
            {
                StatusCode = System.Net.HttpStatusCode.Forbidden,
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
                StatusCode = System.Net.HttpStatusCode.Unauthorized
            };
        }

        var md5 = Md5.Create(request.Password.ToLower());

        if (user.Password != md5)
        {
            user.PasswordRetryCount++;
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new Response<LoginResponse>
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                isSuccessful = false,
                Message = "Invalid user informations"
            };
        }

        if (!user.IsActive)
        {
            return new Response<LoginResponse>
            {
                StatusCode = System.Net.HttpStatusCode.Unauthorized,
                isSuccessful = false,
                Message = "Invalid user informations"
            };
        }

        string token = _jwtHelper.Token(user);

        TokenResponse tokenResponse = new()
        {
            Token = token,
            ExpireDate = DateTime.Now.AddMinutes(jwtConfig.AccessTokenExpiration),
            Email = user.Email,
            RefreshToken = _jwtHelper.CreateRefreshToken(),
            UserName = user.UserName,
            Role = user.Role,
            Id = user.Id
        };

        user.RefreshToken = tokenResponse.RefreshToken;
        user.RefreshTokenExpireDate = tokenResponse.ExpireDate;

        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.CompleteAsync(cancellationToken);

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
            StatusCode = System.Net.HttpStatusCode.OK,
            Data = loginResponse
        };
    }
}