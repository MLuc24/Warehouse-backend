namespace WarehouseManage.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Manager = "Manager";
    
    public static readonly string[] AllRoles = { Admin, User, Manager };
    
    public static bool IsValidRole(string role)
    {
        return AllRoles.Contains(role);
    }
}
