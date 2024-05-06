namespace DotnetAPI.Dtos
{
    public partial class UserForLoginDto
    {
        public String Email { get; set; }
         public String Password { get; set; }

        public UserForLoginDto()
        {
            if (Email == null)
            {
                Email ="";
            }
              if (Password == null)
            {
                Password  ="";
            }
           
        }
        }
        }
