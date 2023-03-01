using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        public String accountDeleted;
        public String confirmed;
        public void OnGet()
        {
            
        }
     
        public void OnPost()
        {
            String username = Request.Form["username"];
            String password = Request.Form["password"];
            String encryptedPass = encryptPassword(password);
           
            bool checker = isExisting(username, encryptedPass);

            if (checker == true)
            {
                accountDeleted = isActive(username);
                if (accountDeleted.Equals("TRUE"))
                {
                    confirmed = isConfirmed(username);
                    if (confirmed.Equals("TRUE"))
                    {
                        HttpContext.Session.SetString("sessionUsername", username);
                        Response.Redirect("/HomePage");
                    }
                    else
                    {
                        HttpContext.Session.SetString("sessionUsername", username);
                        Response.Redirect("/ConfirmEmail");
                    }
                   
                }
                else
                {
                    errorMsg = "Your account was deleted !";
                }
            }
            else
            {
                errorMsg = "Invalid username or password";
                return;
            }
            

        }
        public string encryptPassword(string pass)
        {
            byte[] storePass = ASCIIEncoding.ASCII.GetBytes(pass);
            string encryptedPass = Convert.ToBase64String(storePass);
            return encryptedPass;
        }

        public string isActive(String username)
        {
            String result = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT user_active FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                       
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return result;
        }

        public string isConfirmed(String username)
        {
            String result = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT user_confirmed FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return result;
        }
        public bool isExisting(String username, String password)
        {
            bool check = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT COUNT(*) FROM users WHERE user_username=@username AND user_password=@pass";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@pass", password);
                        Int32 count = (Int32)cmd.ExecuteScalar();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if(count > 0)
                                {
                                    check = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return check;
        }
    }
}
