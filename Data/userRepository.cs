namespace DotnetAPI.Data
{
    public class UserRepository
    {
        DataContextEF _entityFramework;
        IMapper _mapper;
        public UserControllerEF(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }
    }