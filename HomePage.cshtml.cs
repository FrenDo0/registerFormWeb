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
   
    public class HomePageModel : PageModel
    {
        
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public bool contains = false;
        public User user = new User();
        public String sessionUsername = "";
        public String errorMsg = "";
        
        public void OnGet()
        {
            
            sessionUsername = HttpContext.Session.GetString("sessionUsername");
            if(sessionUsername == null)
            {
                errorMsg = "Couldnt get session username !";
                Response.Redirect("/Authorization");
                return;
            }
            else
            {
                getUserInformation(sessionUsername);
                HttpContext.Session.SetString("sessionUserId", user.userId);
            }
        }

        public void OnPostDelete()
        {
            String id = HttpContext.Session.GetString("sessionUserId");
            changeUserStatus(id,user);
            HttpContext.Session.Clear();
            Response.Redirect("/Index");
        }
        public void OnPostLogOut()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/LogIn");
        }

        public void changeUserStatus(String userId,User user)
        {
            user.active = "FALSE";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE users SET user_active=@active WHERE user_id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@active", user.active);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "asd" + ex.StackTrace;
            }
        }
        public User getUserInformation(String username)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT * FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.userId = "" + reader.GetInt64(0);
                                user.Username = reader.GetString(1);
                                user.Password = reader.GetString(2);
                                user.FirstName = reader.GetString(3);
                                user.SecondName = reader.GetString(4);
                                user.Email = reader.GetString(5);
                                user.confirmedEmail = reader.GetString(6);
                                user.active = reader.GetString(7);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return user;
        }
    }
}
