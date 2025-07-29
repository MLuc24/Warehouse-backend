namespace WarehouseManage.Constants;

public static class AuthConstants
{
    public const int TOKEN_EXPIRY_HOURS = 24;
    public const int REFRESH_TOKEN_EXPIRY_DAYS = 30;
    
    public static class Roles
    {
        public const string ADMIN = "Admin";
        public const string MANAGER = "Manager";
        public const string EMPLOYEE = "Employee";
    }
    
    public static class ClaimTypes
    {
        public const string USER_ID = "userId";
        public const string USERNAME = "username";
        public const string ROLE = "role";
        public const string FULL_NAME = "fullName";
    }
}
