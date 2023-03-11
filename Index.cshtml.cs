using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Web.Helpers;

namespace testWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ReCaptcha _captcha;
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public bool contains = false;
        public String errorMsg;
        User user = new User();
        public String msg = "";
       

        public IndexModel(ReCaptcha captcha)
        {
            _captcha = captcha;
        }
        public async Task OnPostAsync()
        {
            String storedPass = Request.Form["password"];
            user.Username = Request.Form["username"];
            user.FirstName = Request.Form["firstName"];
            user.SecondName = Request.Form["secondName"];
            user.Email = Request.Form["email"];
            user.confirmedEmail = "FALSE";
            user.active = "TRUE";
            contains = true;
            bool checker = false;
            bool validMail = false;
            String confirmPass = Request.Form["confirmPass"];
            if(user.Email != "")
            {
                validMail = IsValidMail(user.Email);
            }
            if (user.Username.Length == 0)
            {
                errorMsg = "Enter username";
                checker = false;
            }
            else if (user.Username.Length < 3 || user.Username.Length > 20)
            {
                errorMsg = "Username must be between 3 and 20 symbols";
                checker = false;
            }
            else if (storedPass.Length < 6 || storedPass.Length > 20)
            {
                errorMsg = "Password must be between 6 and 20 symbols";
                checker = false;
            }
            else if (storedPass.Length == 0)
            {
                errorMsg = "Enter password";
                checker = false;
            }
            else if (confirmPass.Length == 0 || !confirmPass.Equals(storedPass))
            {
                errorMsg = "Confirm your password !";
                checker = false;
            }
            else if (user.FirstName.Length < 3 || user.FirstName.Length > 30)
            {
                errorMsg = "First name must be between 3 and 30 symbols";
                checker = false;
            }
            else if (user.FirstName.Length == 0)
            {
                errorMsg = "Enter first name";
                checker = false;
            }
            else if (user.SecondName.Length < 3 || user.SecondName.Length > 30)
            {
                errorMsg = "Second name must be between 3 and 30 symbols";
                checker = false;
            }
            else if (user.SecondName.Length == 0)
            {
                errorMsg = "Enter second name";
                checker = false;
            }
            else if (user.Email.Length < 5 || user.Email.Length > 30)
            {
                errorMsg = "Email address must be between 5 and 30 symbols";
                checker = false;
            }
            else if (user.Email.Length == 0)
            {
                errorMsg = "Enter email";
                checker = false;
            }
            
            else if (validMail == false)
            {
                errorMsg = "Invalid email address";
                checker = false;
            }
            else
            {
              
                checker = true;
            }
            //Използвам  google recaptcha https://codingsonata.com/google-recaptcha-v3-server-verification-in-asp-net-core-web-api/
            if (!Request.Form.ContainsKey("g-recaptcha-response"))
            {
                errorMsg = "Verify captcha";
                checker = false;
            }
            var captcha = Request.Form["g-recaptcha-response"].ToString();
            if (!await _captcha.IsValid(captcha))
            {
                errorMsg = "Verify captcha";
                checker = false;
            }

            if (checker == true)
            {
                //Използвам System.Web.Helpers където има клас Crypto в който има методи HashPassword(string password) и VerifyHashPassword(string hashedPass,string pass)
                //https://learn.microsoft.com/en-us/dotnet/api/system.web.helpers.crypto.hashpassword?view=aspnet-webpages-3.2

                user.Password = Crypto.HashPassword(storedPass);
                insertUser(user);
                User newUser = new User();
                newUser = getUserInformation(user.Username);

                //A GUID (globally unique identifier) is a 128-bit text string that represents an identification (ID).
                string generateCode = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                string cryptedCode = Crypto.HashPassword(generateCode);

                Int64 id = int.Parse(newUser.userId);
                String username = "";
                username = user.Username;
                DateTime date = DateTime.Now;
                sentCode(cryptedCode, id,date);
                sendEmailVerification(cryptedCode, id.ToString());
                msg = "Please confirm your email address !";

                //The session data itself is stored server side.
                //The only thing that is stored on the client's computer
                //is a cookie with a unique identifier so the server
                //knows which session to load at the server side. Users
                //cannot manipulate the data stored in the session itself.

                HttpContext.Session.SetString("sessionUsername", username);
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
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return user;
        }
        public bool IsValidMail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public void insertUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO users (user_username,user_password,user_firstName,user_secondName,user_email,user_confirmed,user_active) " +
                        "VALUES (@username,@pass,@fName,@sName,@email,@confirmed,@active)";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", user.Username);
                        cmd.Parameters.AddWithValue("@pass", user.Password);
                        cmd.Parameters.AddWithValue("@fName", user.FirstName);
                        cmd.Parameters.AddWithValue("@sName", user.SecondName);
                        cmd.Parameters.AddWithValue("@email", user.Email);
                        cmd.Parameters.AddWithValue("@confirmed", user.confirmedEmail);
                        cmd.Parameters.AddWithValue("@active", user.active);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "Already existing username or email !";
            }
        }

        public string encryptPassword(string pass)
        {
            byte[] storePass = ASCIIEncoding.ASCII.GetBytes(pass);
            string encryptedPass = Convert.ToBase64String(storePass);
            return encryptedPass;
        }

        public string decryptPass(string pass)
        {
            byte[] encryptedPass = Convert.FromBase64String(pass);
            string decryptedPass = ASCIIEncoding.ASCII.GetString(encryptedPass);
            return decryptedPass;
        }

        public void sentCode(String code,Int64 userId,DateTime date)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO confirmCodes (confirm_code,user_id,sended_date) " +
                        "VALUES (@code,@id,@date)";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@code",code);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.Parameters.AddWithValue("@date",date);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
        }
        public async void sendEmailVerification(String confirmCode,String userId)
        {
            String url = "https://localhost:44322/LogIn?code=" + Uri.EscapeDataString(confirmCode) +"&userId="+userId;
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
                errorMsg ="neshto" + ex.StackTrace + ex.Message;
            }
        }

        
    }
}
