namespace WarehouseManage.Constants;

public static class EmployeeConstants
{
    // Employee Roles
    public const string MANAGER_ROLE = "Manager";
    public const string EMPLOYEE_ROLE = "Employee";
    public const string ADMIN_ROLE = "Admin";

    // Status
    public const bool ACTIVE_STATUS = true;
    public const bool INACTIVE_STATUS = false;

    // Validation Limits
    public const int USERNAME_MIN_LENGTH = 3;
    public const int USERNAME_MAX_LENGTH = 50;
    public const int PASSWORD_MIN_LENGTH = 6;
    public const int PASSWORD_MAX_LENGTH = 100;
    public const int FULLNAME_MIN_LENGTH = 2;
    public const int FULLNAME_MAX_LENGTH = 100;
    public const int EMAIL_MAX_LENGTH = 255;
    public const int PHONE_MAX_LENGTH = 20;
    public const int ADDRESS_MAX_LENGTH = 500;

    // Search & Pagination
    public const int DEFAULT_PAGE_SIZE = 10;
    public const int MAX_PAGE_SIZE = 100;
    public const int MIN_PAGE_SIZE = 5;

    // Sorting Options
    public static readonly string[] VALID_SORT_FIELDS = 
    {
        "Username", "FullName", "Email", "Role", "Status", "CreatedAt"
    };
    
    public static readonly string[] VALID_SORT_DIRECTIONS = 
    {
        "asc", "desc"
    };

    // Employee Management Permissions
    public static class Permissions
    {
        public const string VIEW_EMPLOYEES = "ViewEmployees";
        public const string CREATE_EMPLOYEE = "CreateEmployee";
        public const string UPDATE_EMPLOYEE = "UpdateEmployee";
        public const string DELETE_EMPLOYEE = "DeleteEmployee";
        public const string RESET_PASSWORD = "ResetEmployeePassword";
        public const string VIEW_EMPLOYEE_STATS = "ViewEmployeeStats";
    }

    // Error Messages
    public static class ErrorMessages
    {
        public const string EMPLOYEE_NOT_FOUND = "Không tìm thấy nhân viên";
        public const string USERNAME_ALREADY_EXISTS = "Tên đăng nhập đã tồn tại";
        public const string EMAIL_ALREADY_EXISTS = "Email đã được sử dụng";
        public const string INVALID_EMPLOYEE_ID = "ID nhân viên không hợp lệ";
        public const string CANNOT_DELETE_EMPLOYEE = "Không thể xóa nhân viên này";
        public const string INVALID_ROLE = "Vai trò không hợp lệ";
        public const string INVALID_STATUS = "Trạng thái không hợp lệ";
        public const string PASSWORD_TOO_WEAK = "Mật khẩu quá yếu";
        public const string CANNOT_MODIFY_ADMIN = "Không thể chỉnh sửa tài khoản Admin";
        public const string CANNOT_DELETE_SELF = "Không thể xóa chính tài khoản của mình";
        public const string INSUFFICIENT_PERMISSIONS = "Không có quyền thực hiện thao tác này";
    }

    // Success Messages
    public static class SuccessMessages
    {
        public const string EMPLOYEE_CREATED = "Tạo nhân viên thành công";
        public const string EMPLOYEE_UPDATED = "Cập nhật nhân viên thành công";
        public const string EMPLOYEE_DELETED = "Xóa nhân viên thành công";
        public const string EMPLOYEE_REACTIVATED = "Khôi phục nhân viên thành công";
        public const string PASSWORD_RESET = "Đặt lại mật khẩu thành công";
        public const string STATUS_UPDATED = "Cập nhật trạng thái thành công";
    }

    // Default Values
    public static class Defaults
    {
        public const string DEFAULT_SORT_BY = "CreatedAt";
        public const string DEFAULT_SORT_DIRECTION = "desc";
        public const int DEFAULT_STATS_MONTHS = 12;
        public const string DEFAULT_PASSWORD = "Employee@123";
    }
}
