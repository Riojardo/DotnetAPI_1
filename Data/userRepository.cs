
using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    // The namespace DotnetAPI.Data contains the class UserRepository
    public class UserRepository : IUserRepository
    {
        // Private fields to hold dependencies
        DataContextEF _entityFramework;

        // Constructor to initialize the UserRepository with required dependencies
        // Note: This constructor expects an IConfiguration instance
        public UserRepository(IConfiguration config)
        {
            // Initializing the DataContextEF instance with the provided configuration
            _entityFramework = new DataContextEF(config);
        }

        // Method to save changes made to the data context
        // Returns true if changes were successfully saved, otherwise false
        public bool SaveChanges()
        {
            // Calling SaveChanges() method of DataContextEF and returning whether any changes were saved
            return _entityFramework.SaveChanges() > 0;
            
        
        }
        public void AddEntity<T>(T entityToAdd)
            {
                if (entityToAdd != null)
                _entityFramework.Add(entityToAdd);
            }

           public void RemoveEntity<T>(T entityToRemove)
            {
                if (entityToRemove != null)
                _entityFramework.Add(entityToRemove);
              
            }

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
      public UserSalary GetSingleUserSalary(int userId)
    {

        UserSalary? userSalary = _entityFramework.UserSalary
        .Where(u => u.UserId == userId)
        .FirstOrDefault<UserSalary>();

        if (userSalary != null)
        {
            return userSalary;
        }
        else
        {
            throw new Exception("Faied To find User");
        }

    }
      public UserJobInfo GetSingleUserJobInfo(int userId)
    {

        UserJobInfo? userJobInfo = _entityFramework.UserJobInfo
        .Where(u => u.UserId == userId)
        .FirstOrDefault<UserJobInfo>();

        if (userJobInfo != null)
        {
            return userJobInfo;
        }
        else
        {
            throw new Exception("Faied To find User");
        }

    }
    

    }

}