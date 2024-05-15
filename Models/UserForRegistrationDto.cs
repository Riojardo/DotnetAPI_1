namespace DotnetAPI.Dtos
{
    public partial class UserForRegistrationDto
    {
        public String Email { get; set; }
        public String Password { get; set; }
        public String PasswordConfirm { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Gender { get; set; }
        public bool Active {get; set; }
        public string JobTitle {get; set;}
        public string Department {get; set;}
         public decimal Salary {get; set;}

        public UserForRegistrationDto()
        {
            if (Email == null)
            {
                Email = "";
            }
            if (Password == null)
            {
                Password = "";
            }
            if (PasswordConfirm == null)
            {
                PasswordConfirm = "";
            }
            if (FirstName == null)
            {
                FirstName = "";
            }
            if (LastName == null)
            {
                LastName = "";
            }
            if (Gender == null)
            {
                Gender = "";
            }
            if (JobTitle == null)
            {
                JobTitle = "";
            }
            if (Department == null)
            {
                Department = "";
            }
        }
    }
}

