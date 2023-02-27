using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlX.XDevAPI;

namespace testWeb.Pages
{
    public class LogInModel : PageModel
    {
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public bool contains = false;
        public bool login = false;
        public String errorMsg;
        
        public int test = 0;
        public void OnGet()
        {
            
        }
     
        public void OnPost()
        {
            String username = Request.Form["username"];
            String password = Request.Form["password"];
            test = selectId(username, password);
            if (test != 0)
            {
                HttpContext.Session.SetString("sessionUsername",username);
                Response.Redirect("/HomePage");
            }
            else
            {
                errorMsg = "Invalid username or password";
                return;
            }
            

        }
        public int selectId(String username,String password)
        {
            int id = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT user_id FROM users WHERE user_username=@username AND user_password=@pass";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                id = reader.GetInt32(0);
                                cmd.ExecuteReader();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sql exception in select method");
            }
            return id;
        }

    }
}
