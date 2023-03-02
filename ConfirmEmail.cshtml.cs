using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace testWeb.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        public String sessionUsername;
        public String test;
        public String errorMsg;
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public void OnGet()
        {
          
        }

        public void OnPostSubmit()
        {
            String code = "";
            sessionUsername = HttpContext.Session.GetString("sessionUsername");
            Int64 userId = getUserId(sessionUsername);
            code = getCodeFromDatabase(userId);
            String email = Request.Form["confirm"];
            String encrypt = "";
            encrypt = encryptPassword(code.ToString());
            
            if (email != null)
            {
                sendEmailVerification(encrypt, userId.ToString());
            }
            else
            {
                errorMsg = "Enter your email !";
            }
        }
        public void OnPostBack()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/LogIn");
        }
        public string encryptPassword(string pass)
        {
            byte[] storePass = ASCIIEncoding.ASCII.GetBytes(pass);
            string encryptedPass = Convert.ToBase64String(storePass);
            return encryptedPass;
        }
        public async void sendEmailVerification(String confirmCode,String userId)
        {

            String url = "https://localhost:44322/LogIn?code=" + confirmCode + "&userId=" + userId;
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("lddimitrov546@gmail.com");
                    mail.To.Add(new MailAddress("lddimitrov546@gmail.com"));
                    mail.Subject = "Please confirm your profile";
                    mail.Body = "<p>Your confirmation link: " + url + " </p>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("lddimitrov546@gmail.com", "vtwshsisywndgyqx");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "neshto" + ex.StackTrace + ex.Message;
            }
        }
        public void changeUserStatus(String userId)
        {
            String status = "TRUE";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE users SET user_confirmed=@confirmed WHERE user_id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@confirmed", status);
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

        public string getCodeFromDatabase(Int64 userId)
        {
            String result = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT confirm_code FROM confirmCodes WHERE user_id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);

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
        public Int64 getUserId(String username)
        {
            Int64 userId = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT user_id FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = reader.GetInt64(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return userId;
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
    }
}
