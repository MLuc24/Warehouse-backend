using System;
using System.Collections.Generic;

namespace WarehouseManage.Model;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Image { get; set; }

    public bool IsEmailVerified { get; set; } = false;

    public bool IsPhoneVerified { get; set; } = false;

    public string Role { get; set; } = null!;

    public bool? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();

    public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
