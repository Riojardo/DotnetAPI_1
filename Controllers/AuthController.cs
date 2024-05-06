using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
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
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.Password)
            {
                string sqlCheckUserExist =
                "SELECT Email FROM TutorialAppSchema.Auth WHERE Email ='" + userForRegistration.Email + "'";
                IEnumerable<string> existingUser = _dapper.LoadData<string>(sqlCheckUserExist);
                if (existingUser.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    // string passwordSaltPlusString =_config.GetSection("Appsettings:PasswordKey")
                    // .Value + Convert.ToBase64String(passwordSalt);

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth =
                    @"INSERT INTO TutorialAppSchema.Auth ([Email],
                    [PasswordHash],
                    [PasswordSalt])  
                    VALUES('" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt )";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;
                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        string sqlAddUser =
                        @"INSERT INTO TutorialAppSchema.Users(
                        [FirstName],
                        [LastName],
                        [Email],
                        [Gender],
                        [Active]
                        ) VALUES (" +
                         "'" + userForRegistration.FirstName +
                         "', '" + userForRegistration.LastName +
                        "', '" + userForRegistration.Email +
                        "', '" + userForRegistration.Gender +
                        "', 1)";
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to ass user");

                    }
                    throw new Exception("Failled to register User :'0 ");

                }
                throw new Exception("there are already an user with this Email :/ ");
            }
            throw new Exception("Password no matching :(");

        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt =
            @"SELECT
            [PasswordHash],
            [PasswordSalt]
            FROM TutorialAppSchema.Auth 
            WHERE Email ='" + userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation =
            _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

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