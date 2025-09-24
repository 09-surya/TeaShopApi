using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;
using WebApiTeaShopManageMent.DAL;
using WebApiTeaShopManageMent.Models;

namespace WebApiTeaShopManageMent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private string sqlConnection = "Server=SURYA;Database=TeashopeManageMent;Trusted_Connection=True;TrustServerCertificate=True;";
        private readonly TeaShopDal _dal = new TeaShopDal();

        // ✅ Place Order
        [HttpPost("place")]
        public IActionResult PlaceOrder([FromBody] Order order)
        {
            if (order == null) return BadRequest("Order data is null");

            if (order.Date == DateTime.MinValue)
                order.Date = DateTime.Now;

            _dal.PlaceOrder(order);
            return Ok(new { message = "Order placed successfully!" });
        }
        // ✅ Update Order
        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, [FromBody] Order order)
        {
            if (order == null || id != order.OrderId)
                return BadRequest("Invalid order");

            bool ok = _dal.UpdateOrder(order);
            if (ok) return Ok(new { message = "Order updated successfully!" });
            return StatusCode(500, "Update failed");
        }

        [HttpGet("bydate")]
        public IActionResult GetOrdersByDate(DateTime date)
        {
            var orders = _dal.GetOrdersByDate(date);
            return Ok(orders);
        }

        // ✅ Get Today’s Orders
        [HttpGet("today")]
        public IActionResult GetTodayOrders()
        {
            var orders = _dal.GetTodayOrders();
            return Ok(orders);
        }

        // ✅ Get Total Sales
        [HttpGet("totalsales")]
        public IActionResult GetTotalSales(DateTime? date = null)
        {
            var total = _dal.GetTotalSales(date);
            return Ok(new { totalSales = total });
        }

        [HttpGet("bydaterange")]
        public IActionResult GetOrdersByDateRange(DateTime fromDate, DateTime toDate)
        {
            var (orders, totalSales) = _dal.GetOrdersByDateRange(fromDate, toDate);
            return Ok(new { orders, totalSales });
        }

        // ✅ Delete Order
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            if (id <= 0)
                return BadRequest(new { Message = "Invalid Order ID" });

            bool result = _dal.DeleteOrder(id);
            if (result)
                return Ok(new { Success = true, Message = "Order deleted successfully" });
            else
                return NotFound(new { Success = false, Message = "Order not found" });
        }

        [HttpGet("export")]
        public IActionResult ExportOrders([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var (orders, totalSales) = _dal.GetOrdersByDateRange(fromDate, toDate);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Orders");

                // === LOGO ===
                string logoPath = @"D:\TeaCoffee.jpg";
                if (System.IO.File.Exists(logoPath))
                {
                    var picture = ws.Drawings.AddPicture("Logo", new FileInfo(logoPath));
                    picture.SetPosition(0, 0, 0, 0); // top-left corner (row 1 col 1)
                    var imageMerged = ws.Cells["A1:B4"];
                    imageMerged.Merge = true;
                    picture.SetSize(180, 77);
                }

                // === TITLE ===
                var titleRange = ws.Cells["C1:H4"];
                titleRange.Merge = true;
                titleRange.Value = "Antony Tea Shop Management";
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.Size = 20;
                titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // === TABLE HEADERS starting at row 6 ===
                ws.Cells[6, 1].Value = "Order ID";
                ws.Cells[6, 2].Value = "Customer Name";
                ws.Cells[6, 3].Value = "Item Name";
                ws.Cells[6, 4].Value = "Qty";
                ws.Cells[6, 5].Value = "Price";
                ws.Cells[6, 6].Value = "Price Total";
                ws.Cells[6, 7].Value = "Date";

                using (var headerRange = ws.Cells[6, 1, 6, 7])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Font.Color.SetColor(Color.White);
                    headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(181, 101, 29)); 
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                // === DATA ===
                int row = 7;
                foreach (var o in orders)
                {
                    decimal lineTotal = o.Qty * o.Price;

                    ws.Cells[row, 1].Value = o.OrderId;
                    ws.Cells[row, 2].Value = o.CustomerName;
                    ws.Cells[row, 3].Value = o.Item;
                    ws.Cells[row, 4].Value = o.Qty;
                    ws.Cells[row, 5].Value = o.Price;
                    ws.Cells[row, 6].Value = lineTotal;
                    ws.Cells[row, 7].Value = o.Date.ToString("yyyy-MM-dd");

                    ws.Cells[row, 4].Style.Numberformat.Format = "0";
                    ws.Cells[row, 5].Style.Numberformat.Format = "0.00";
                    ws.Cells[row, 6].Style.Numberformat.Format = "0.00";

                    row++;
                }

                // === TOTAL SALES ===
                ws.Cells[row, 5].Value = "Total Sales";
                ws.Cells[row, 5].Style.Font.Bold = true;
                ws.Cells[row, 6].Value = totalSales;
                ws.Cells[row, 6].Style.Numberformat.Format = "0.00";
                ws.Cells[row, 6].Style.Font.Bold = true;
                ws.Cells[row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(208, 163, 108));
                ws.Cells[row, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(208, 163, 108));

                // Autofit all
                ws.Cells[1, 1, row, 7].AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Orders_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
            }
        }

        [HttpGet("paged")]
        public IActionResult GetOrdersPaged([FromQuery] DateTime? date, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (orders, totalCount) = _dal.GetOrdersPaged(date, page, pageSize);
            return Ok(new { orders, totalCount });
        }

        [HttpGet]
        [HttpGet("{customerName?}")]
        public async Task<IActionResult> GetCustomerTotals(string? customerName = null)
        {
            var result = new List<CustomerTotal>();

            using (var conn = new SqlConnection(sqlConnection))
            {
                using (var cmd = new SqlCommand("Sp_GetCustomerTotals", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (!string.IsNullOrEmpty(customerName))
                        cmd.Parameters.AddWithValue("@CustomerName", customerName);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new CustomerTotal
                            {
                                CustomerName = reader["CustomerName"].ToString(),
                                TotalAmount = reader["TotalAmount"] != DBNull.Value
                                    ? (decimal)reader["TotalAmount"]
                                    : 0
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }

        public class CustomerTotal
    {
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
}

