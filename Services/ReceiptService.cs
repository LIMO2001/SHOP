using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LaptopStore.Models;

namespace LaptopStore.Services
{
    public class ReceiptService
    {
        public byte[] GenerateReceipt(Order order, List<OrderItem> orderItems, User user)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("LaptopStore - Order Receipt")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Order Information
                            column.Item().Text($"Order #: {order.OrderNumber}");
                            column.Item().Text($"Order Date: {order.OrderDate:MMMM dd, yyyy hh:mm tt}");
                            column.Item().Text($"Status: {order.Status}");

                            // Customer Information - UPDATED for your User model
                            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(customerColumn =>
                            {
                                customerColumn.Spacing(5);
                                customerColumn.Item().Text("BILLING INFORMATION").SemiBold();
                                customerColumn.Item().Text($"Customer: {user.FirstName} {user.LastName}"); // Using FirstName + LastName
                                customerColumn.Item().Text($"Email: {user.Email}");
                                customerColumn.Item().Text($"Shipping Address: {order.ShippingAddress}");
                            });

                            // Order Items
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(50); // #
                                    columns.RelativeColumn(3);  // Product
                                    columns.ConstantColumn(80); // Qty
                                    columns.ConstantColumn(90); // Price
                                    columns.ConstantColumn(90); // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Text("#").SemiBold();
                                    header.Cell().Text("Product").SemiBold();
                                    header.Cell().Text("Qty").SemiBold();
                                    header.Cell().Text("Price").SemiBold();
                                    header.Cell().Text("Total").SemiBold();
                                });

                                // Items
                                for (int i = 0; i < orderItems.Count; i++)
                                {
                                    var item = orderItems[i];
                                    var index = i + 1;

                                    // Use ProductName from OrderItem (already saved during checkout)
                                    var productName = !string.IsNullOrEmpty(item.ProductName) ? item.ProductName : 
                                                     item.Product?.Name ?? "Product";

                                    table.Cell().Text(index.ToString());
                                    table.Cell().Text(productName);
                                    table.Cell().Text(item.Quantity.ToString());
                                    table.Cell().Text($"${item.UnitPrice:F2}");
                                    table.Cell().Text($"${(item.UnitPrice * item.Quantity):F2}");
                                }
                            });

                            // Summary
                            column.Item().AlignRight().Column(summaryColumn =>
                            {
                                summaryColumn.Spacing(5);
                                summaryColumn.Item().Text($"Subtotal: ${order.TotalAmount:F2}");
                                summaryColumn.Item().Text($"Tax: $0.00");
                                summaryColumn.Item().Text($"Shipping: $0.00");
                                summaryColumn.Item().Text($"Total: ${order.TotalAmount:F2}").SemiBold().FontSize(14);
                            });

                            // Payment Method
                            column.Item().Text($"Payment Method: {order.PaymentMethod}");

                            // Thank you message
                            column.Item().AlignCenter().Text("Thank you for your purchase!").Italic().FontSize(12);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}