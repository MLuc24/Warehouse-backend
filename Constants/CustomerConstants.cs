namespace WarehouseManage.Constants;

public static class CustomerConstants
{
    // Customer Types
    public const string CUSTOMER_TYPE_REGULAR = "Regular";
    public const string CUSTOMER_TYPE_VIP = "VIP";
    public const string CUSTOMER_TYPE_WHOLESALE = "Wholesale";
    
    // Customer Status
    public const string STATUS_ACTIVE = "Active";
    public const string STATUS_INACTIVE = "Inactive";
    
    // Error Messages
    public const string ERROR_CUSTOMER_NOT_FOUND = "Không tìm thấy khách hàng";
    public const string ERROR_CUSTOMER_NAME_EXISTS = "Tên khách hàng đã tồn tại";
    public const string ERROR_CUSTOMER_EMAIL_EXISTS = "Email đã được sử dụng";
    public const string ERROR_CANNOT_DELETE_CUSTOMER_WITH_ORDERS = "Không thể xóa khách hàng có đơn hàng";
    public const string ERROR_INVALID_CUSTOMER_TYPE = "Loại khách hàng không hợp lệ";
    
    // Success Messages
    public const string SUCCESS_CUSTOMER_CREATED = "Tạo khách hàng thành công";
    public const string SUCCESS_CUSTOMER_UPDATED = "Cập nhật khách hàng thành công";
    public const string SUCCESS_CUSTOMER_DELETED = "Xóa khách hàng thành công";
    public const string SUCCESS_CUSTOMER_REACTIVATED = "Kích hoạt lại khách hàng thành công";
    
    // Validation Rules
    public const int MIN_CUSTOMER_NAME_LENGTH = 2;
    public const int MAX_CUSTOMER_NAME_LENGTH = 200;
    public const int MAX_ADDRESS_LENGTH = 500;
    public const int MAX_PHONE_LENGTH = 20;
    public const int MAX_EMAIL_LENGTH = 100;
    
    // Default Values
    public const int DEFAULT_PAGE_SIZE = 10;
    public const int MAX_PAGE_SIZE = 100;
    public const int MAX_TOP_CUSTOMERS = 20;
    public const int DEFAULT_TOP_CUSTOMERS = 5;
    
    // Customer Types List for Validation
    public static readonly string[] VALID_CUSTOMER_TYPES = 
    {
        CUSTOMER_TYPE_REGULAR,
        CUSTOMER_TYPE_VIP,
        CUSTOMER_TYPE_WHOLESALE
    };
    
    // Status Types List for Validation
    public static readonly string[] VALID_STATUS_TYPES = 
    {
        STATUS_ACTIVE,
        STATUS_INACTIVE
    };
}
