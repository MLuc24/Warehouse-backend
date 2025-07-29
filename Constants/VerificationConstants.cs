namespace WarehouseManage.Constants;

public static class VerificationConstants
{
    public const int CODE_LENGTH = 6;
    public const int CODE_EXPIRY_MINUTES = 5;
    public const int MAX_ATTEMPTS = 3;
    public const int RESEND_COOLDOWN_SECONDS = 60;
    
    public static class Types
    {
        public const string EMAIL = "Email";
        public const string PHONE = "Phone";
    }
    
    public static class Purposes
    {
        public const string REGISTRATION = "Registration";
        public const string FORGOT_PASSWORD = "ForgotPassword";
        public const string CHANGE_EMAIL = "ChangeEmail";
        public const string CHANGE_PHONE = "ChangePhone";
    }
}
