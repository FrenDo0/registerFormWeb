using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace testWeb.Pages
{
    public class ResetPasswordModel : PageModel
    {
        public String errorMsg;
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public void OnGet()
        {

        }

        public void OnPost()
        {
            bool check = false;
            String email = "";
            email = Request.Form["email"];
            var randomPass = new Random();

            int pass = randomPass.Next(1000, 9999);
            String newPass = pass.ToString();
            String encrypted = encryptPassword(newPass);

            check = isExisting(email);
            if(check == true)
            {
                sendEmailWithNewPassword(newPass);
                changeUserPassword(email, encrypted);
                Response.Redirect("/LogIn");
            }
            else
            {
                errorMsg = "Please enter valid email address";
            }
        }
        public string encryptPassword(string pass)
        {
            byte[] storePass = ASCIIEncoding.ASCII.GetBytes(pass);
            string encryptedPass = Convert.ToBase64String(storePass);
            return encryptedPass;
        }
        public void changeUserPassword(String email,String newPass)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE users SET user_password=@pass WHERE user_email=@email";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@pass", newPass);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "asd" + ex.StackTrace;
            }
        }

        public async void sendEmailWithNewPassword(String newPassword)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("lddimitrov546@gmail.com");
                    mail.To.Add(new MailAddress("lddimitrov546@gmail.com"));
                    mail.Subject = "This is your new password";
                    mail.Body = "<h2>Your new password is: " + newPassword + "</h2>";
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
        public bool isExisting(String email)
        {
            bool check = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT COUNT(*) FROM users WHERE user_email=@email";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
    
                        Int32 count = (Int32)cmd.ExecuteScalar();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (count > 0)
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
