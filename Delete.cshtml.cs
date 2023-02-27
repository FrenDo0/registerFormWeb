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
    public class DeleteModel : PageModel
    {
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public String sessionUsername = "";
        public String errorMsg = "";
        public String userId = "";
        public void OnGet()
        {
            sessionUsername = HttpContext.Session.GetString("sessionUsername");
            if (sessionUsername == null)
            {
                errorMsg = "Couldnt get session username !";
                Response.Redirect("/Authorization");
                return;
            }
            
        }
        public void OnPost()
        {
            userId = HttpContext.Session.GetString("sessionUserId");
            deleteUserById(userId);
            if(errorMsg.Length == 0)
            {
                Response.Redirect("/Index");
                HttpContext.Session.Clear();
            }
        }

        public void deleteUserById(String userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "DELETE FROM users WHERE user_id=@userId";
                    using(SqlCommand cmd =new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
        }
    }
}
