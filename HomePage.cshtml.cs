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

        public void OnPost()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/LogIn");
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
                                user.userId = "" + reader.GetInt32(0);
                                user.Username = reader.GetString(1);
                                user.Password = reader.GetString(2);
                                user.FirstName = reader.GetString(3);
                                user.SecondName = reader.GetString(4);
                                user.Email = reader.GetString(5);
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
