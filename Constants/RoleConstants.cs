namespace WarehouseManage.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";  // Đổi từ User thành Employee
    public const string Manager = "Manager";
    
    public static readonly string[] AllRoles = { Admin, Employee, Manager };
    
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role);
    }
}
