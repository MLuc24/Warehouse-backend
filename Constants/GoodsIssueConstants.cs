namespace WarehouseManage.Constants;

public static class GoodsIssueConstants
{
    public static class Status
    {
        public const string Draft = "Draft";
        public const string AwaitingApproval = "AwaitingApproval";
        public const string Approved = "Approved";
        public const string Preparing = "Preparing";
        public const string Delivered = "Delivered";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";
    }

    public static class IssueNumberPrefix
    {
        public const string Standard = "GI";
        public const string Format = "GI{0:yyyyMMdd}{1:D3}"; // GI20250808001
    }

    public static class WorkflowActions
    {
        public const string Submit = "Submit"; // Draft → AwaitingApproval
        public const string Approve = "Approve"; // AwaitingApproval → Approved
        public const string Reject = "Reject"; // AwaitingApproval → Rejected
        public const string StartPreparing = "StartPreparing"; // Approved → Preparing
        public const string MarkDelivered = "MarkDelivered"; // Preparing → Delivered
        public const string CompleteIssue = "CompleteIssue"; // Delivered → Completed
        public const string Cancel = "Cancel"; // Any status → Cancelled
        public const string Resubmit = "Resubmit"; // Rejected → AwaitingApproval
    }

    public static class EmailTemplates
    {
        public const string DeliveryNotification = "CustomerDeliveryNotification";
        public const string ApprovalNotification = "GoodsIssueApprovalNotification";
        public const string PreparationNotification = "GoodsIssuePreparationNotification";
    }

    public static class Validation
    {
        public const int MaxNotesLength = 1000;
        public const int MinQuantity = 1;
        public const int MaxQuantity = 999999;
        public const decimal MinUnitPrice = 0.01m;
        public const decimal MaxUnitPrice = 999999999.99m;
    }

    public static class ErrorMessages
    {
        public const string GoodsIssueNotFound = "Không tìm thấy phiếu xuất kho";
        public const string CustomerNotFound = "Không tìm thấy khách hàng";
        public const string ProductNotFound = "Không tìm thấy sản phẩm";
        public const string InsufficientStock = "Không đủ tồn kho";
        public const string InvalidStatusTransition = "Không thể chuyển trạng thái";
        public const string AccessDenied = "Không có quyền thực hiện";
        public const string AlreadyDelivered = "Phiếu xuất đã được giao";
        public const string NotPrepared = "Phiếu xuất chưa được chuẩn bị";
        public const string IssueNumberExists = "Số phiếu xuất đã tồn tại";
        public const string EmptyDetails = "Chi tiết phiếu xuất không được để trống";
        public const string InvalidQuantity = "Số lượng không hợp lệ";
        public const string InvalidUnitPrice = "Đơn giá không hợp lệ";
        public const string CannotDeleteCompleted = "Không thể xóa phiếu xuất đã hoàn thành";
    }
}
