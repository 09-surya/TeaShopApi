using Microsoft.Data.SqlClient;
using WebApiTeaShopManageMent.Models;

namespace WebApiTeaShopManageMent.DAL
{
    public class MenuDal
    {
        private string sqlConnection = "Server=SURYA;Database=TeashopeManageMent;Trusted_Connection=True;TrustServerCertificate=True;";
        private string selectMenuQuery = "SELECT * FROM Menu";

        public List<Menu> GetMenu()
        {
            var menu = new List<Menu>();

            using (SqlConnection connection = new SqlConnection(sqlConnection))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(selectMenuQuery, connection))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        menu.Add(new Menu
                        {
                            MenuId = Convert.ToInt32(reader["MenuId"]),
                            Item = reader["Item"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }

            return menu;
        }
        public bool insertMenu(Menu menu)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = @"INSERT INTO Menu (Item,Price) 
                 VALUES (@Item,@Price)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Item", menu.Item);
                    cmd.Parameters.AddWithValue("@Price", menu.Price);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool menuUpdate(Menu menu)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = @"UPDATE Menu 
                         SET  Item=@Item,Price=@Price
                         WHERE MenuId=@MenuId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Item", menu.Item);
                    cmd.Parameters.AddWithValue("@Price", menu.Price);
                    cmd.Parameters.AddWithValue("@MenuId", menu.MenuId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool menuDelete(int id)
        {
            using (SqlConnection conn = new SqlConnection(sqlConnection))
            {
                conn.Open();
                string query = "DELETE FROM Menu WHERE MenuId = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
