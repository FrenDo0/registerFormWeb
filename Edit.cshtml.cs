using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace testWeb.Pages
{
    public class EditModel : PageModel
    {
        public User user2 = new User();
        public String errorMsg = "";
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        
        public void OnGet()
        {
            if (HttpContext.Session.GetString("sessionUsername") == null)
            {
                Response.Redirect("/Authorization");
            }
            else
            {
                String id = HttpContext.Session.GetString("sessionUserId");
                if (id == null)
                {
                    errorMsg = "Something is wrong with session id";
                    return;
                }
                else
                {
                    getUserInformation(id);
                }
            }
        }

        public void OnPost()
        {
            String id = HttpContext.Session.GetString("sessionUserId");
            
            User user = new User();
            user.Username = Request.Form["username"];
            user.Password = Request.Form["password"];
            user.FirstName = Request.Form["firstName"];
            user.SecondName = Request.Form["secondName"];
            user.Email = Request.Form["email"];
            String confirmPass = Request.Form["confirmPassword"];
            
            bool checker = false;

            if (user.Username == null)
            {
                errorMsg = "Enter username";
                checker = false;
            }
            else if (user.Password == null)
            {
                errorMsg = "Enter password";
                checker = false;
            }
            else if (user.Password.Length != 0 && !user.Password.Equals(confirmPass))
            {
                errorMsg = "Password is not matching";
                checker = false;
            }
            else if (user.FirstName == null)
            {
                errorMsg = "Enter first name";
                checker = false;
            }
            else if (user.SecondName == null)
            {
                errorMsg = "Enter second name";
                checker = false;
            }
            else if (user.Email == null)
            {
                errorMsg = "Enter email";
                checker = false;
            }
            else
            {
                checker = true;
            }

            if (checker == true)
            {
                saveEdit(id,user);
                Response.Redirect("/HomePage");
            }
        }
        public void saveEdit(String userId,User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE users SET user_username=@username,user_password=@pass,user_firstName=@fName,user_secondName=@sName,user_email=@email WHERE user_id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                            cmd.Parameters.AddWithValue("@username", user.Username);
                            cmd.Parameters.AddWithValue("@pass", user.Password);
                            cmd.Parameters.AddWithValue("@fName", user.FirstName);
                            cmd.Parameters.AddWithValue("@sName", user.SecondName);
                            cmd.Parameters.AddWithValue("@email", user.Email);
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.ExecuteNonQuery(); 
                    }
                }
            }
            catch(Exception ex)
            {
                errorMsg = "asd" + ex.StackTrace;
            }
        }
        public User getUserInformation(String id)
        {
            
            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM users WHERE user_id=@id";
                    using(SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user2.userId = "" + reader.GetInt32(0);
                                user2.Username = reader.GetString(1);
                                user2.Password = reader.GetString(2);
                                user2.FirstName = reader.GetString(3);
                                user2.SecondName = reader.GetString(4);
                                user2.Email = reader.GetString(5);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("SQL exception");
            }
            return user2;
        }
    }
}
