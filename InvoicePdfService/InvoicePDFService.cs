using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.html;
using System.Drawing;
using System.Reflection.Metadata;
using static iTextSharp.text.pdf.AcroFields;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.InvoicePdfService
{
    public class InvoicePDFService
    {
        private readonly string _invoiceFolderPath;
        private readonly BeautyShopDbContext _db;
        public InvoicePDFService(IWebHostEnvironment env, BeautyShopDbContext db)
        {
            _invoiceFolderPath = Path.Combine(env.ContentRootPath, "BeautyShop_Invoices");
            if (!Directory.Exists(_invoiceFolderPath)) Directory.CreateDirectory(_invoiceFolderPath);
            _db = db;
        }

        public Paragraph CenteredParagraph(string text, Font font)
        {
            var p = new Paragraph(text, font)
            {
                Alignment = Element.ALIGN_CENTER,
            };
            return p;
        }

        public Paragraph LeftAlignParagraph(string text, Font font)
        {
            var p = new Paragraph(text, font)
            {
                Alignment = Element.ALIGN_LEFT,
            };
            return p;
        }

        public Paragraph RightAlignParagraph(string text, Font font)
        {
            var p = new Paragraph(text, font)
            {
                Alignment = Element.ALIGN_RIGHT,
            };
            return p;
        }
        public Paragraph BottoAlignParagraph(string text, Font font)
        {
            var p = new Paragraph(text, font)
            {
                Alignment = Element.ALIGN_RIGHT,
            };
            return p;
        }

        public async Task<string> GenerateInvoicePdf(string invoiceNumber, int invoiceId, int clientId)
        {
            string fileName = $"MyBeautyShop - {invoiceNumber}.pdf";
            string filePath = Path.Combine(_invoiceFolderPath, fileName);

            var newInvoice = await GetInvoiceByIdAsync(invoiceId);
            var client = await _db.Users.FindAsync(clientId);

            if (newInvoice is null || client is null)
            {
                return "Error (INGX01). Invoice details not found. Generation of the invoice was broken on the server, Please email info@mybeautyshop.co.za with a copy of this error.";
            }

            var Items = newInvoice.InvoiceItems;       

            if (newInvoice is not null && Items is not null && client is not null)
            {
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var doc = new iTextSharp.text.Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                BaseColor BEAUTYPINK = new BaseColor(242, 115, 242);
                BaseColor LIGHTGREY = new BaseColor(217, 230, 242);

                var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                var titleFont2 = FontFactory.GetFont("Arial", 16, Font.BOLD, BEAUTYPINK);
                var subTitleFont1 = FontFactory.GetFont("Arial", 14, Font.BOLD);
                var subTitleFont2 = FontFactory.GetFont("Arial", 14, Font.BOLD, BEAUTYPINK);
                var subTitleFont3 = FontFactory.GetFont("Arial", 12, Font.BOLD);
                var subTitleFont4 = FontFactory.GetFont("Arial", 12, Font.NORMAL);
                var subTitleFont5 = FontFactory.GetFont("Arial", 12, Font.BOLD, BEAUTYPINK);
                var bodyFont = FontFactory.GetFont("Arial", 11, Font.NORMAL, BaseColor.BLACK);
                var bodyFontBold = FontFactory.GetFont("Arial", 11, Font.BOLD, BaseColor.BLACK);
                var HeaderFontBold = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE);
                var rightFont = FontFactory.GetFont("Arial", 11, Font.NORMAL, BaseColor.BLACK);

                var PaymentStatusFont = FontFactory.GetFont("Arial", 16);
                string paymentStatus;

                if (newInvoice.IsPaid == false)
                {
                    PaymentStatusFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.RED);
                    paymentStatus = "Not Paid";
                }
                else if (newInvoice.IsPaid == true)
                {
                    PaymentStatusFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.GREEN);
                    paymentStatus = "Paid";
                }
                else
                {
                    PaymentStatusFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.GRAY);
                    paymentStatus = "Status Missing";
                }

                //var paragraph = new Paragraph();

                string logoPath = string.Empty;

                Image logo = null;

                try
                {
                    logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InvoicePdfService/invoiceIMG/Logo2B.png");
                    logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(100f, 100f);
                }
                catch
                {
                    // If logo fails to load, continue without it
                    logo = null;
                    Console.WriteLine("Image path: " + logoPath);
                }

                //Create header table=====================
                PdfPTable headerTable = new PdfPTable(2)
                {
                    WidthPercentage = 100
                };
                headerTable.SetWidths(new float[] { 3f, 1f });
                PdfPCell companyDetailsCell = new PdfPCell(new Phrase($"myBeautyShop", subTitleFont1))
                {
                    Padding = 0,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(companyDetailsCell);
                // Right: Logo
                PdfPCell logoCell = new PdfPCell
                {
                    Padding = 4,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                if (logo is not null)
                {
                    logoCell.AddElement(logo);
                }
                headerTable.AddCell(logoCell);

                doc.Add(headerTable);
                //Add HeaderTabe==========================

                doc.Add(new Paragraph("TAX INVOICE\n\n", titleFont2));

                //Create header table=====================
                PdfPTable ClientTable = new PdfPTable(3)
                {
                    WidthPercentage = 100
                };

                ClientTable.SetWidths(new float[] { 3f, 2f, 3f });
                PdfPCell cellLeft = new PdfPCell(new Phrase("FROM", bodyFontBold))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    Padding = 0,
                };
                ClientTable.AddCell(cellLeft);

                PdfPCell cellMiddle = new PdfPCell(new Phrase(" ", bodyFontBold))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    Padding = 0,
                };
                ClientTable.AddCell(cellMiddle);

                PdfPCell cellRight = new PdfPCell(new Phrase("BILL TO", bodyFontBold))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    Padding = 0,
                };
                ClientTable.AddCell(cellRight);

                PdfPCell FromDetails = new PdfPCell(new Phrase("myBeautyShop\nE: info@mybeautyshop.co.za\nP: (+27) 12-345-6789", bodyFont))
                {
                    Padding = 0,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_TOP
                };
                ClientTable.AddCell(FromDetails);

                PdfPCell PStatus = new PdfPCell(new Phrase(paymentStatus, PaymentStatusFont))
                {
                    Padding = 0,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_TOP
                };
                ClientTable.AddCell(PStatus);

                PdfPCell ToDetails = new PdfPCell(new Phrase($"{client?.FullName}\n{client?.Email}\n{client?.PhoneNumber}", bodyFont))
                {
                    Padding = 0,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_TOP
                };
                ClientTable.AddCell(ToDetails);

                doc.Add(ClientTable);
                //Add ClientdetailsTabe==========================

                LineSeparator line = new LineSeparator(1f, 100f, BEAUTYPINK, Element.ALIGN_CENTER, -1);
                Chunk linebreak = new Chunk(line);
                Paragraph paraline = new Paragraph(linebreak);
                doc.Add(paraline);
                doc.Add(new Paragraph("\n", bodyFont));

                doc.Add(RightAlignParagraph($"Invoice NO. #{newInvoice.InvoiceNumber}", bodyFontBold));
                doc.Add(RightAlignParagraph($"Date: {newInvoice.InvoiceDate.ToString("dd/MM/yyyy")}\n", bodyFont));

                doc.Add(new Paragraph("\n", bodyFont));
                //create a line items table================================= 
                PdfPTable lineItemsTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                };
                // Set column widths (optional but good for layout control)
                lineItemsTable.SetWidths(new float[] { 4f, 1.5f, 1.5f, 2f });
                string[] headers = { "Description", "Quantity", "Rate" };
                foreach (var header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, HeaderFontBold))
                    {
                        BackgroundColor = BEAUTYPINK,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        Padding = 4,
                    };
                    lineItemsTable.AddCell(cell);
                }
                PdfPCell rightcell = new PdfPCell(new Phrase("Line Total", HeaderFontBold))
                {
                    BackgroundColor = BEAUTYPINK,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    Padding = 4,
                };
                lineItemsTable.AddCell(rightcell);
                //Add InvoiceItem
                foreach (var item in Items)
                {
                    lineItemsTable.AddCell(new PdfPCell(new Phrase(item.Name, bodyFontBold)) { Padding = 4, Border = iTextSharp.text.Rectangle.NO_BORDER });
                    lineItemsTable.AddCell(new PdfPCell(new Phrase($"{item.Quantity}", bodyFont)) { Padding = 4, Border = iTextSharp.text.Rectangle.NO_BORDER });
                    lineItemsTable.AddCell(new PdfPCell(new Phrase($"R {item.Price}", bodyFont)) { Padding = 4, Border = iTextSharp.text.Rectangle.NO_BORDER });
                    lineItemsTable.AddCell(new PdfPCell(new Phrase($"R {item.Quantity * item.Price}", bodyFont)) { Padding = 4, Border = iTextSharp.text.Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                }
                // Add the lineItemsTable to the document
                doc.Add(lineItemsTable);
                //End line items table==================================

                doc.Add(new Paragraph("\n", bodyFont));

                doc.Add(RightAlignParagraph($"Subtotal: R {newInvoice.Total}", subTitleFont3));
                doc.Add(RightAlignParagraph($"Tax: R 0.00", subTitleFont3));
                doc.Add(RightAlignParagraph($"Total: R {newInvoice.Total}\n\n", subTitleFont3));


                //doc.Add(new Paragraph($"Please kindly make all payments due to {_bankingDetails.AccountHolder} using the account details below.\n\n", bodyFont));
                //doc.Add(new Paragraph($"Banking Details: \n", bodyFontBold));
                //doc.Add(new Paragraph($"Bank Name: {_bankingDetails.BankName}", bodyFont));
                //doc.Add(new Paragraph($"Branch Code: {_bankingDetails.BranchCode}", bodyFont));
                //doc.Add(new Paragraph($"Account Number: {_bankingDetails.AccountNumber}", bodyFont));
                //doc.Add(new Paragraph($"Account Holder: {_bankingDetails.AccountHolder}", bodyFont));
                //doc.Add(new Paragraph("Please use the invoice number as a reference. \n\n", bodyFont));

                Paragraph thankYou = new Paragraph("Thank you for your business.", titleFont2);
                thankYou.Alignment = Element.ALIGN_CENTER;
                doc.Add(thankYou);
                doc.Add(paraline);

                doc.Close();
                return fileName; // or return filePath if you need full path
            }
            else
            {
                return "Error (INGX02). Could not generate the invoice, as some details are broken in the creation process. Please email admin@riversideholdings.co.za with a copy of this error.";
            }

        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            if (_db is null)
            {
                return null; // or throw exception
            }

            try
            {
                var invoice = await _db.Invoices
                    .Include(x => x.InvoiceItems)
                    .SingleOrDefaultAsync(x => x.Id == id);

                return invoice; // might be null if not found
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new Exception("Failed to fetch invoice", ex);
            }
        }
    }
}