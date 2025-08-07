using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using WarehouseManage.DTOs.GoodsReceipt;

namespace WarehouseManage.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateGoodsReceiptPdfAsync(GoodsReceiptDto goodsReceipt);
    }

    public class PdfService : IPdfService
    {
        public Task<byte[]> GenerateGoodsReceiptPdfAsync(GoodsReceiptDto goodsReceipt)
        {
            return Task.Run(() => GenerateGoodsReceiptPdf(goodsReceipt));
        }

        private byte[] GenerateGoodsReceiptPdf(GoodsReceiptDto goodsReceipt)
        {
            try
            {
                using var stream = new MemoryStream();
                using var writer = new PdfWriter(stream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Set font
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Company header
                var companyHeader = new Paragraph("CÔNG TY QUẢN LÝ KHO HÀNG")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(companyHeader);

                var title = new Paragraph("PHIẾU NHẬP KHO")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20);
                document.Add(title);

                // Receipt information
                var infoTable = new Table(2).UseAllAvailableWidth();
                
                infoTable.AddCell(new Cell().Add(new Paragraph($"Số phiếu: {goodsReceipt.ReceiptNumber ?? "Chưa có"}").SetFont(font)));
                infoTable.AddCell(new Cell().Add(new Paragraph($"Ngày tạo: {goodsReceipt.ReceiptDate?.ToString("dd/MM/yyyy") ?? "Chưa có"}").SetFont(font)));
                
                infoTable.AddCell(new Cell().Add(new Paragraph($"Nhà cung cấp: {goodsReceipt.SupplierName ?? ""}").SetFont(font)));
                infoTable.AddCell(new Cell().Add(new Paragraph($"Trạng thái: {GetStatusText(goodsReceipt.Status ?? "")}").SetFont(font)));

                if (!string.IsNullOrEmpty(goodsReceipt.Notes))
                {
                    infoTable.AddCell(new Cell(1, 2).Add(new Paragraph($"Ghi chú: {goodsReceipt.Notes}").SetFont(font)));
                }

                document.Add(infoTable);
                document.Add(new Paragraph().SetMarginBottom(20)); // Spacing

                // Products table
                var productsTable = new Table(new float[] { 1, 4, 2, 2, 2, 2, 2 }).UseAllAvailableWidth();
                
                // Headers
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("STT").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Tên sản phẩm").SetFont(boldFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("SKU").SetFont(boldFont)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Số lượng").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Đơn vị").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Đơn giá").SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT)));
                productsTable.AddHeaderCell(new Cell().Add(new Paragraph("Thành tiền").SetFont(boldFont).SetTextAlignment(TextAlignment.RIGHT)));

                // Data rows
                if (goodsReceipt.Details?.Any() == true)
                {
                    for (int i = 0; i < goodsReceipt.Details.Count; i++)
                    {
                        var detail = goodsReceipt.Details[i];
                        
                        productsTable.AddCell(new Cell().Add(new Paragraph((i + 1).ToString()).SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                        productsTable.AddCell(new Cell().Add(new Paragraph(detail.ProductName ?? "").SetFont(font)));
                        productsTable.AddCell(new Cell().Add(new Paragraph(detail.ProductSku ?? "").SetFont(font)));
                        productsTable.AddCell(new Cell().Add(new Paragraph(detail.Quantity.ToString()).SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                        productsTable.AddCell(new Cell().Add(new Paragraph(detail.Unit ?? "").SetFont(font).SetTextAlignment(TextAlignment.CENTER)));
                        productsTable.AddCell(new Cell().Add(new Paragraph($"{detail.UnitPrice:N0} đ").SetFont(font).SetTextAlignment(TextAlignment.RIGHT)));
                        productsTable.AddCell(new Cell().Add(new Paragraph($"{detail.Subtotal:N0} đ").SetFont(font).SetTextAlignment(TextAlignment.RIGHT)));
                    }
                }

                document.Add(productsTable);

                // Total amount
                var totalParagraph = new Paragraph($"Tổng tiền: {goodsReceipt.TotalAmount:N0} đ")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetMarginTop(10)
                    .SetMarginBottom(30);
                document.Add(totalParagraph);

                // Signature section
                var signatureTable = new Table(3).UseAllAvailableWidth();
                
                signatureTable.AddCell(new Cell().Add(new Paragraph("Người tạo phiếu").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                signatureTable.AddCell(new Cell().Add(new Paragraph("Phụ trách kho").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                signatureTable.AddCell(new Cell().Add(new Paragraph("Giám đốc").SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER)));
                
                signatureTable.AddCell(new Cell().Add(new Paragraph($"({goodsReceipt.CreatedByUserName ?? ""})").SetFont(font).SetTextAlignment(TextAlignment.CENTER).SetMarginTop(40)));
                signatureTable.AddCell(new Cell().Add(new Paragraph("").SetMarginTop(40)));
                signatureTable.AddCell(new Cell().Add(new Paragraph("").SetMarginTop(40)));

                document.Add(signatureTable);

                // Export timestamp
                var timestamp = new Paragraph($"Xuất lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginTop(20);
                document.Add(timestamp);

                document.Close();
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
            }
        }

        private static string GetStatusText(string status)
        {
            return status switch
            {
                "Draft" => "Nháp",
                "Pending" => "Chờ xác nhận NCC",
                "AwaitingApproval" => "Chờ phê duyệt",
                "SupplierConfirmed" => "NCC đã xác nhận",
                "Rejected" => "Bị từ chối",
                "Cancelled" => "Đã hủy",
                "Completed" => "Hoàn thành",
                _ => status
            };
        }
    }
}
