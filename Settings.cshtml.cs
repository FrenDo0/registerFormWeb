using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace testWeb.Pages
{
    public class SettingsModel : PageModel
    {
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public String errorMsg = "";
        public void OnGet()
        {
        }

        public void OnPostLinkExpiration()
        {
            String minutesStr = Request.Form["expireTime"];
            if(minutesStr.Length != 0)
            {
                int minutes = int.Parse(minutesStr);
                changeExpirationTime(minutes);
            }
        }
        public void OnPostIncorrectPass()
        {
            String nums = Request.Form["number"];
            String sec = Request.Form["time"];
            if(nums.Length != 0)
            {
                int numbers = int.Parse(nums);
                changeAttempts(numbers);
            }
            if(sec.Length != 0)
            {
                int seconds = int.Parse(sec);
                changeTimeOut(seconds);
            }
        }

        public void changeExpirationTime(int minutes)
        {
            int id = 1;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE wrongPassSettings SET expiration_time=@minutes WHERE id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("minutes", minutes);
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
        }
        public void changeTimeOut(int seconds)
        {
            int id = 1;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE wrongPassSettings SET time_out=@seconds WHERE id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("seconds", seconds);
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
        }
        public void changeAttempts(int numberAttempts)
        {
            int id = 1;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE wrongPassSettings SET number_wrong_tries=@attempts WHERE id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("attempts",numberAttempts);
                        cmd.Parameters.AddWithValue("id",id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
        }
    }
}
