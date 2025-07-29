namespace WarehouseManage.Constants;

public static class ErrorMessages
{
    public static class Auth
    {
        public const string INVALID_CREDENTIALS = "Tên đăng nhập hoặc mật khẩu không đúng";
        public const string USER_NOT_FOUND = "Không tìm thấy người dùng";
        public const string ACCOUNT_DISABLED = "Tài khoản đã bị vô hiệu hóa";
        public const string INVALID_TOKEN = "Token không hợp lệ";
        public const string TOKEN_EXPIRED = "Token đã hết hạn";
        
        // Registration errors
        public const string EMAIL_ALREADY_EXISTS = "Email đã được sử dụng";
        public const string PHONE_ALREADY_EXISTS = "Số điện thoại đã được sử dụng";
        public const string USERNAME_ALREADY_EXISTS = "Tên đăng nhập đã được sử dụng";
        public const string INVALID_EMAIL_FORMAT = "Định dạng email không hợp lệ";
        public const string INVALID_PHONE_FORMAT = "Định dạng số điện thoại không hợp lệ";
        
        // Verification errors
        public const string VERIFICATION_CODE_SENT = "Mã xác thực đã được gửi";
        public const string VERIFICATION_CODE_INVALID = "Mã xác thực không đúng";
        public const string VERIFICATION_CODE_EXPIRED = "Mã xác thực đã hết hạn";
        public const string VERIFICATION_CODE_USED = "Mã xác thực đã được sử dụng";
        public const string MAX_VERIFICATION_ATTEMPTS = "Bạn đã nhập sai quá nhiều lần";
        public const string VERIFICATION_REQUIRED = "Vui lòng xác thực email hoặc số điện thoại trước";
        public const string SEND_VERIFICATION_FAILED = "Không thể gửi mã xác thực, vui lòng thử lại";
    }
    
    public static class General
    {
        public const string INVALID_INPUT = "Dữ liệu đầu vào không hợp lệ";
        public const string INTERNAL_ERROR = "Có lỗi xảy ra, vui lòng thử lại sau";
        public const string UNAUTHORIZED = "Bạn không có quyền truy cập";
    }
}
