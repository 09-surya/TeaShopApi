using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WebApiTeaShopManageMent.Models;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace WebApiTeaShopManageMent.DAL
{
    public class TeaShopDal
    {

        //private string sqlConnection = "Host=containers-us-west-87.railway.app;Port=5432;Database=railway;Username=user123;Password=secretPass;";
        private string sqlConnection = "Server=SURYA;Database=TeashopeManageMent;Trusted_Connection=True;TrustServerCertificate=True;";
        private string insertOrderQuery = "INSERT INTO Orders(Item, Qty, Price, Date) VALUES(@Item, @Qty, @Price, @Date)";
        private string selectOrdersQuery = "SELECT * FROM Orders";
        private string totalSalesQuery = "SELECT ISNULL(SUM(Qty * Price), 0) FROM Orders WHERE CAST(Date AS DATE) = CAST(GETDATE() AS DATE)";

        // Insert Order
        public bool PlaceOrder(Order order)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = @"INSERT INTO Orders (CustomerName, Item, Qty, Price, Date) 
                 VALUES (@CustomerName, @Item, @Qty, @Price, @Date)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MenuId", order.MenuId);
                    cmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    cmd.Parameters.AddWithValue("@Item", order.Item);
                    cmd.Parameters.AddWithValue("@Qty", order.Qty);
                    cmd.Parameters.AddWithValue("@Price", order.Price);
                    cmd.Parameters.AddWithValue("@Date",
                        order.Date == default(DateTime) ? DateTime.Now : order.Date);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        // Update Order
        public bool UpdateOrder(Order order)
        {
            using (var conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = @"UPDATE Orders 
                         SET CustomerName=@CustomerName, Item=@Item, Qty=@Qty, Price=@Price, Date=@Date
                         WHERE OrderId=@OrderId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    cmd.Parameters.AddWithValue("@Item", order.Item);
                    cmd.Parameters.AddWithValue("@Qty", order.Qty);
                    cmd.Parameters.AddWithValue("@Price", order.Price);
                    cmd.Parameters.AddWithValue("@Date", order.Date);
                    cmd.Parameters.AddWithValue("@OrderId", order.OrderId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Get Today's Orders
        public List<Order> GetTodayOrders()
        {
            var orders = new List<Order>();

            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(selectOrdersQuery, connection))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderId = Convert.ToInt32(reader["OrderId"]),
                            Item = reader["Item"].ToString(),
                            Qty = Convert.ToInt32(reader["Qty"]),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Date = Convert.ToDateTime(reader["Date"]),
                            CustomerName= reader["CustomerName"].ToString(),
                        });
                    }
                }
            }

            return orders;
        }

        // Get Today's Total Sales
        public decimal GetTotalSales()
        {
            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(totalSalesQuery, connection))
                {
                    object result = cmd.ExecuteScalar();
                    return Convert.ToDecimal(result);
                }
            }
        }

        public bool DeleteOrder(int id)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = "DELETE FROM Orders WHERE OrderId = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public List<Order> GetOrdersByDate(DateTime date)
        {
            var orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = "SELECT * FROM Orders WHERE CAST(Date AS DATE) = @date";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@date", date.Date);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderId = Convert.ToInt32(reader["OrderId"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            Item = reader["Item"].ToString(),
                            Qty = Convert.ToInt32(reader["Qty"]),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Date = Convert.ToDateTime(reader["Date"])
                        });
                    }
                }
            }
            return orders;
        }
        public decimal GetTotalSales(DateTime? date)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = date.HasValue
                    ? "SELECT ISNULL(SUM(Qty * Price), 0) FROM Orders WHERE CAST(Date AS DATE) = @date"
                    : "SELECT ISNULL(SUM(Qty * Price), 0) FROM Orders";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (date.HasValue)
                        cmd.Parameters.AddWithValue("@date", date.Value.Date);

                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
        }
        public (List<Order>, decimal) GetOrdersByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = new List<Order>();
            decimal totalSales = 0;

            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("Sp_GetOrdersByDateRange", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                Item = reader.GetString(reader.GetOrdinal("Item")),
                                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date"))
                            });
                        }

                        if (reader.NextResult() && reader.Read() && !reader.IsDBNull(0))
                        {
                            totalSales = reader.GetDecimal(0);
                        }
                    }
                }
            }

            return (orders, totalSales);
        }
        public (List<Order> orders, int totalCount) GetOrdersPaged(DateTime? date, int pageNumber, int pageSize)
        {
            var orders = new List<Order>();
            int totalCount = 0;

            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string sql = @"
            ;WITH Ordered AS (
                SELECT 
                    OrderId, CustomerName, Item, Qty, Price, Date,
                    ROW_NUMBER() OVER (ORDER BY Date DESC) AS RowNum
                FROM Orders
                WHERE (@Date IS NULL OR CAST(Date AS date) = @Date)
            )
            SELECT * FROM Ordered
            WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);

            SELECT COUNT(*) 
            FROM Orders
            WHERE (@Date IS NULL OR CAST(Date AS date) = @Date);
        ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", (object?)date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new Order
                            {
                                OrderId = (int)reader["OrderId"],
                                CustomerName = reader["CustomerName"].ToString(),
                                Item = reader["Item"].ToString(),
                                Qty = Convert.ToInt32(reader["Qty"]),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Date = Convert.ToDateTime(reader["Date"])
                            });
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            totalCount = reader.GetInt32(0);
                        }
                    }
                }
            }

            return (orders, totalCount);
        }

    }
}
