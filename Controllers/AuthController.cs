using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Index = System.Index;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql  _reusableSql;
        private readonly IMapper _mapper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" +
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto() {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                        userComplete.Active = true;

                        if (_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }
        [HttpPut("Reset_Password")]

        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update your Password");
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt =
            @"EXEC TutorialAppSchema.spLoginConfirmation_Get
            @Email ='" + userForLogin.Email + "'";

           DynamicParameters sqlParameters = new DynamicParameters();
            
            // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            // emailParameter.Value = userForLogin.Email;
            // sqlParameters.Add(emailParameter);
            
            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);
           
            UserForLoginConfirmationDto userForConfirmation =
            _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);
            
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password,userForConfirmation.PasswordSalt);

            // if(passwordHash == userForConfirmation.PasswordHash) ----> can't work

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                    return StatusCode(401, "Password was incorect !!!!");
            }

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
               userForLogin.Email + "'";


            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("Refresh_Token")]

        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("UserId")?.Value + "";

            string userIdSql = "SELECT userId FROM TutorialAppSchema.Users WHERE UserId = "
            + userId;

            int userIdFromDB = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userIdFromDB )}
            });
        }

        // private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        // {
        //     string passwordSaltPlusString =
        //                 _config.GetSection("Appsettings:PasswordKey")
        //                 .Value + Convert.ToBase64String(passwordSalt);

        //     return KeyDerivation.Pbkdf2(
        //         password: password,
        //         salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
        //         prf: KeyDerivationPrf.HMACSHA256,
        //         iterationCount: 100000,
        //         numBytesRequested: 256 / 8

        //     );
        // }

        // private string CreateToken(int userId)
        // {
        //     Claim[] claims = new Claim[]
        //     {
        //     new Claim("userId", userId.ToString())
        //     };

        //     string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

        //     SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
        //             Encoding.UTF8.GetBytes(
        //                 tokenKeyString != null ? tokenKeyString : ""
        //             )
        //         );


        //     SigningCredentials credentials =
        //     new SigningCredentials(
        //         tokenKey, SecurityAlgorithms.HmacSha512Signature
        //         );

        //     SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
        //     {
        //         Subject = new ClaimsIdentity(claims),
        //         SigningCredentials = credentials,
        //         Expires = DateTime.Now.AddDays(1)
        //     };

        //     JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        //     SecurityToken token = tokenHandler.CreateToken(descriptor);

        //     return tokenHandler.WriteToken(token);
        // }

    }

}