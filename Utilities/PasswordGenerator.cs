using WarehouseManage.Helpers;

namespace WarehouseManage.Utilities;

/// <summary>
/// Utility class để generate password hash cho test users
/// Sử dụng trong môi trường development để tạo test data
/// </summary>
public static class PasswordGenerator
{
    public static void GenerateTestPasswords()
    {
        var passwords = new Dictionary<string, string>
        {
            { "admin", "admin123" },
            { "manager", "manager123" },
            { "employee", "employee123" }
        };

        Console.WriteLine("=== Generated Password Hashes ===");
        
        foreach (var kvp in passwords)
        {
            var hash = PasswordHelper.HashPassword(kvp.Value);
            Console.WriteLine($"User: {kvp.Key} | Password: {kvp.Value} | Hash: {hash}");
        }
        
        Console.WriteLine("\n=== SQL Insert Statements ===");
        
        var users = new[]
        {
            new { Username = "admin", Password = "admin123", FullName = "Quản trị viên", Email = "admin@warehouse.com", Role = "Admin" },
            new { Username = "manager", Password = "manager123", FullName = "Quản lý kho", Email = "manager@warehouse.com", Role = "Manager" },
            new { Username = "employee", Password = "employee123", FullName = "Nhân viên kho", Email = "employee@warehouse.com", Role = "Employee" }
        };

        foreach (var user in users)
        {
            var hash = PasswordHelper.HashPassword(user.Password);
            Console.WriteLine($"""
                INSERT INTO Users (Username, PasswordHash, FullName, Email, Role, Status, CreatedAt)
                VALUES ('{user.Username}', '{hash}', N'{user.FullName}', '{user.Email}', '{user.Role}', 1, GETDATE());
                """);
        }
    }
}
