using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LaptopStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaptopStore.Services
{
    public class ReceiptService
    {
        public byte[] GenerateReceipt(Order order, List<OrderItem> orderItems, User user)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Time zone
            var kenyaTime = TimeZoneInfo.ConvertTimeFromUtc(order.OrderDate,
                TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"));

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // HEADER
                    page.Header()
                        .AlignCenter()
                        .Text("DKL EmpireLaptopStore \n Order Receipt")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    // CONTENT
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Order Information
                            column.Item().Text($"Order #: {order.OrderNumber}");
                            column.Item().Text($"Order Date: {kenyaTime:MMMM dd, yyyy hh:mm tt}");
                            column.Item().Text($"Status: {order.Status}");

                            // Customer Information
                            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(customerColumn =>
                            {
                                customerColumn.Spacing(5);
                                customerColumn.Item().Text("BILLING INFORMATION").SemiBold();
                                customerColumn.Item().Text($"Customer: {user.FirstName} {user.LastName}");
                                customerColumn.Item().Text($"Email: {user.Email}");
                                customerColumn.Item().Text($"Shipping Address: {order.ShippingAddress}");
                            });

                            // Order Items Table
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
                                    var productName = !string.IsNullOrEmpty(item.ProductName) ? item.ProductName :
                                                      item.Product?.Name ?? "Product";

                                    table.Cell().Text(index.ToString());
                                    table.Cell().Text(productName).WrapAnywhere();
                                    table.Cell().Text(item.Quantity.ToString());
                                    table.Cell().Text($"KES {item.UnitPrice:F2}");
                                    table.Cell().Text($"KES {(item.UnitPrice * item.Quantity):F2}");
                                }
                            });

                            // Summary
                            column.Item().AlignRight().Column(summaryColumn =>
                            {
                                summaryColumn.Spacing(5);
                                var subtotal = orderItems.Sum(i => i.UnitPrice * i.Quantity);
                                var tax = subtotal * 0.08m;
                                var shipping = subtotal > 0 ? 10.00m : 0.00m;
                                var total = subtotal + tax + shipping;

                                summaryColumn.Item().Text($"Subtotal: KES {subtotal:F2}");
                                summaryColumn.Item().Text($"Tax (8%): KES {tax:F2}");
                                summaryColumn.Item().Text($"Shipping: KES {shipping:F2}");
                                summaryColumn.Item().Text($"Total: KES {total:F2}").SemiBold().FontSize(14);
                            });

                            // Payment Method
                            column.Item().Text($"Payment Method: {order.PaymentMethod}");

                            // Thank you message
                            column.Item().AlignCenter().Text("Thank you for your purchase with Us!").Italic().FontSize(12);
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(t =>
                        {
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
