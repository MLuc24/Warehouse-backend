namespace WarehouseManage.Constants;

public static class GoodsReceiptConstants
{
    public static class Status
    {
        public const string Draft = "Draft";
        public const string AwaitingApproval = "AwaitingApproval"; // Employee tạo, cần Admin/Manager duyệt
        public const string Pending = "Pending"; // Đã được duyệt, gửi email cho supplier
        public const string SupplierConfirmed = "SupplierConfirmed"; // Supplier đã xác nhận
        public const string Completed = "Completed"; // Đã nhập kho
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected"; // Admin/Manager từ chối
    }

    public static class ReceiptNumberPrefix
    {
        public const string Standard = "GR";
        public const string Format = "GR{0:yyyyMMdd}{1:D3}"; // GR20250806001
    }

    public static class WorkflowActions
    {
        public const string Approve = "Approve"; // Admin/Manager approve
        public const string Reject = "Reject"; // Admin/Manager reject
        public const string SupplierConfirm = "SupplierConfirm"; // Supplier confirms
        public const string CompleteReceipt = "CompleteReceipt"; // Nhập kho
        public const string Cancel = "Cancel"; // Hủy phiếu
    }

    public static class EmailTemplates
    {
        public const string SupplierConfirmation = "SupplierGoodsReceiptConfirmation";
        public const string ApprovalNotification = "GoodsReceiptApprovalNotification";
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
        public const string GoodsReceiptNotFound = "Không tìm thấy phiếu nhập kho";
        public const string SupplierNotFound = "Không tìm thấy nhà cung cấp";
        public const string ProductNotFound = "Không tìm thấy sản phẩm";
        public const string InvalidQuantity = "Số lượng không hợp lệ";
        public const string InvalidUnitPrice = "Đơn giá không hợp lệ";
        public const string CannotDeleteCompleted = "Không thể xóa phiếu nhập đã hoàn thành";
        public const string ReceiptNumberExists = "Số phiếu nhập đã tồn tại";
        public const string EmptyDetails = "Chi tiết phiếu nhập không được để trống";
    }
}
