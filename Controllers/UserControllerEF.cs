using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserControllerEF : ControllerBase
{
    DataContextEF _entityFramework;
    IMapper _mapper;
    public UserControllerEF(IConfiguration config)
    {
        _entityFramework = new DataContextEF(config);

        _mapper = new Mapper( new MapperConfiguration
        (cfg => {cfg.CreateMap<CreateUserDTO, User>();
         } ));
    }

    // [HttpGet("TestConnection")]
    // public DateTime TestConnection()
    // {
    //     return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    // }

    [HttpGet("GetUsers")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<User> GetUsers()
    {

        IEnumerable<User> users = _entityFramework.Users.ToList<User>();
        return users;
        // return new string[] {"user1", "user2" };
        // return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        // {
        //     Date = DateTime.Now.AddDays(index),
        //     TemperatureC = Random.Shared.Next(-20, 55),
        //     Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        // })
        // .ToArray();
    }



    [HttpGet("GetSingleUser/{userId}")]

    public User GetSingleUser(int userId)
    {

        User? user = _entityFramework.Users
        .Where(u => u.UserId == userId)
        .FirstOrDefault<User>();

        if (user != null)
        {
            return user;
        }
        else
        {
            throw new Exception("Faied To find User");
        }

    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDb = _entityFramework.Users
           .Where(u => u.UserId == user.UserId)
           .FirstOrDefault<User>();

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            else
            {
                throw new Exception("Faied To update User");
            }
        }
        else
        {
            throw new Exception("Faied To find User");
        }
    }

    [HttpPost("CreateUser")]
    public IActionResult AddUser(CreateUserDTO user)
    {
        // User userDb = new User();
        User userDb = _mapper.Map<User>(user);

        // userDb.Active = user.Active;
        // userDb.FirstName = user.FirstName;
        // userDb.LastName = user.LastName;
        // userDb.Email = user.Email;
        // userDb.Gender = user.Gender;

        _entityFramework.Add(userDb);
        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        else
        {
            throw new Exception("Faied To create User");
        }

    }

[HttpDelete("DeleteUser/{userId}")]
public IActionResult DeleteUser(int userId)
{
     User? userDb = _entityFramework.Users
           .Where(u => u.UserId == userId)
           .FirstOrDefault<User>();

    if (userDb != null)
    {
       _entityFramework.Users.Remove(userDb);
            if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Failed to delete User");

    }
 throw new Exception("Failed to find User");
}

     [HttpGet("GetUserSalary/{userId}")]
    // public IEnumerable<User> GetUsers()
    public IEnumerable<UserSalary> GetUserSalaryEF(int userId)
    {
        return _entityFramework.UserSalary
       .Where(u => u.UserId == userId)
        .ToList();
    }

    [HttpPut("UserSalary")]
    public IActionResult PutUserSalaryEF(UserSalary userForUpdate)
    {
        UserSalary? userToUpdate = _entityFramework.UserSalary
        .Where(u => u.UserId == userForUpdate.UserId)
        .FirstOrDefault();

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Faied to update salary on savechange");
        }
        throw new Exception("Fail to find userSalary to update");
    }
  [HttpDelete("UserSalary/{userId}")]
    public IActionResult DeleteUserSalaryEf(int userId)
    {
        UserSalary? userToDelete = _entityFramework.UserSalary
            .Where(u => u.UserId == userId)
            .FirstOrDefault();

        if (userToDelete != null)
        {
            _entityFramework.UserSalary.Remove(userToDelete);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Deleting UserSalary failed on save");
        }
        throw new Exception("Failed to find UserSalary to delete");
    }


    [HttpGet("UserJobInfo/{userId}")]
    public IEnumerable<UserJobInfo> GetUserJobInfoEF(int userId)
    {
        return _entityFramework.UserJobInfo
            .Where(u => u.UserId == userId)
            .ToList();
    }

    [HttpPost("UserJobInfo")]
    public IActionResult PostUserJobInfoEf(UserJobInfo userForInsert)
    {
        _entityFramework.UserJobInfo.Add(userForInsert);
        if (_entityFramework.SaveChanges() > 0)
        {
            return Ok();
        }
        throw new Exception("Adding UserJobInfo failed on save");
    }


    [HttpPut("UserJobInfo")]
    public IActionResult PutUserJobInfoEf(UserJobInfo userForUpdate)
    {
        UserJobInfo? userToUpdate = _entityFramework.UserJobInfo
            .Where(u => u.UserId == userForUpdate.UserId)
            .FirstOrDefault();

        if (userToUpdate != null)
        {
            _mapper.Map(userForUpdate, userToUpdate);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Updating UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to Update");
    }


    [HttpDelete("UserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfoEf(int userId)
    {
        UserJobInfo? userToDelete = _entityFramework.UserJobInfo
            .Where(u => u.UserId == userId)
            .FirstOrDefault();

        if (userToDelete != null)
        {
            _entityFramework.UserJobInfo.Remove(userToDelete);
            if (_entityFramework.SaveChanges() > 0)
            {
                return Ok();
            }
            throw new Exception("Deleting UserJobInfo failed on save");
        }
        throw new Exception("Failed to find UserJobInfo to delete");
    }


}
