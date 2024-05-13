using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dapper;
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
                    // byte[] passwordSalt = new byte[128 / 8];
                    // using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    // {
                    //     rng.GetNonZeroBytes(passwordSalt);
                    // }

                    // // string passwordSaltPlusString =_config.GetSection("Appsettings:PasswordKey")
                    // // .Value + Convert.ToBase64String(passwordSalt);

                    // byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    // string sqlAddAuth =
                    // @"EXEC TutorialAppSchema.spRegistration_Upsert 
                    // @Email = @EmailParam,
                    // @PasswordHash = @PasswordHashParam, 
                    // @PasswordSalt = @PasswordHashSaltParam ";


                    // List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
                    // emailParameter.Value = userForRegistration.Email;
                    // sqlParameters.Add(emailParameter);

                    // SqlParameter passwordSaltParameter = new SqlParameter("@PasswordHashSaltParam", SqlDbType.VarBinary);
                    // passwordSaltParameter.Value = passwordSalt;
                    // sqlParameters.Add(passwordSaltParameter);

                    // SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary);
                    // passwordHashParameter.Value = passwordHash;
                    // sqlParameters.Add(passwordHashParameter);

                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert
                            @FirstName = '" + userForRegistration.FirstName +
                      "', @LastName = '" + userForRegistration.LastName +
                      "', @Email = '" + userForRegistration.Email +
                      "', @Gender = '" + userForRegistration.Gender +
                      "', @Active = 1" +
                      ", @JobTitle = '" + userForRegistration.JobTitle +
                      "', @Department = '" + userForRegistration.Department +
                      "', @Salary = '" + userForRegistration.Salary + "'";

                        // ) VALUES (" +
                        //  "'" + userForRegistration.FirstName +
                        //  "', '" + userForRegistration.LastName +
                        // "', '" + userForRegistration.Email +
                        // "', '" + userForRegistration.Gender +
                        // "', 1)";
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user");

                    }
                    throw new Exception("Failled to register User :'0 ");

                }
                throw new Exception("there are already an user with this Email :/ ");
            }
            throw new Exception("Password no matching :(");

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