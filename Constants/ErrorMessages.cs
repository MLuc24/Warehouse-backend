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
    }
    
    public static class General
    {
        public const string INVALID_INPUT = "Dữ liệu đầu vào không hợp lệ";
        public const string INTERNAL_ERROR = "Có lỗi xảy ra, vui lòng thử lại sau";
        public const string UNAUTHORIZED = "Bạn không có quyền truy cập";
    }
}
