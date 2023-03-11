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
using System.Web.Helpers;
using System.Data;
using System.IO;

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
        public String getVerificationCode;
        public String verified;
        public String getUserIdStr;
        public String notVerified;
        public String test;
        public int numberAttempts = 0;

        public void OnGet()
        {
            getVerificationCode = Request.Query["code"];
            getUserIdStr = Request.Query["userId"];
            
            if(getVerificationCode != null && getUserIdStr != null)
            {
               
                int uId = int.Parse(getUserIdStr);
                Int64 userId = uId;
                DateTime sendedDate = new DateTime();
                DateTime currentDate = DateTime.Now;
                String confirmCode = getUserCode(userId);
               
                if(confirmCode.Equals(getVerificationCode.ToString()))
                {
                    
                    sendedDate = getSendedDate(userId);
                    TimeSpan timeSpan = new TimeSpan();
                    timeSpan = currentDate.Subtract(sendedDate);

                    int minutes = expirationTime();

                    if(timeSpan.TotalMinutes > minutes)
                    {
                        test = "code has expired";
                    }
                    else
                    {
                        changeUserStatus(getUserIdStr);
                        verified = "You verified your email. Please log in";
                    }
                }
                
            }
            
        }

     
        public void OnPost()
        {
            String username = Request.Form["username"];
            String password = Request.Form["password"];
            String hashPass = "";
            bool correctPass = false;
            bool checkUserExisting = false;
            bool checkUser = checkUsername(username);

            Int64 userId = getUserId(username);
            DateTime currentDate = new DateTime();
            currentDate = DateTime.Now;
            bool checkAttempt = checkIfUserIsExistingInIncorrectAttempts(userId);
            int maxAttempts = numberOfAttemptsSettings();

            
            DateTime lastTime = getTime(userId);
            TimeSpan timePassed = currentDate - lastTime;
            Int64 rowId = getRowId(userId);
            DateTime userLockedOut = getUserLockOutTime(userId);
            DateTime releaseTimeOut = DateTime.Now;
            bool isUserLocked = userLockedOut > releaseTimeOut;

            if (checkUser == true)
            {
                hashPass = getPassByUsername(username);
                correctPass = Crypto.VerifyHashedPassword(hashPass,password);
                if(correctPass == true)
                {
                    checkUserExisting = isExisting(username,hashPass);
                }
                
            }
            else
            {
                errorMsg = "Invalid username !";
            }
            if (checkUser == true && checkUserExisting == true && correctPass == true)
            {
                accountDeleted = isActive(username);
                if (accountDeleted.Equals("TRUE"))
                {
                    confirmed = isConfirmed(username);
                    if (confirmed.Equals("TRUE"))
                    {
                        if(checkAttempt == true && isUserLocked == true)
                        {
                            errorMsg = "Your account has been locked until " + userLockedOut + " Please try again later";
                        }
                        else
                        {
                            HttpContext.Session.SetString("sessionUsername", username);
                            Response.Redirect("/HomePage");
                        }
                    }
                    else
                    {
                        notVerified = "Please verify your email. If you didnt received email click here to send new one";
                        HttpContext.Session.SetString("sessionUsername", username);

                    }
                }
                else
                {
                    errorMsg = "This username is not existing";
                }
            }
            else if(checkUser == true && checkUserExisting == false)
            {
                numberAttempts++;
                errorMsg = "Incorrect password. Please try again.";
               

                if(checkAttempt == false)
                {
                    incorrectAttempt(userId, numberAttempts, currentDate);
                }
                else
                {
                    if (!isUserLocked)
                    {
                        if (checkAttempt == true && timePassed.TotalMinutes <= 3)
                        {
                            int numberOfAttempts = checkAttempts(rowId);
                            if (numberOfAttempts < maxAttempts)
                            {
                                numberOfAttempts++;
                                updateIncorrectAttempt(rowId, numberOfAttempts, currentDate);
                            }
                            else
                            {
                                test = "You have tried so many times with wrong passowrd. Now you are locked out";
                                int seconds = userLockOut();
                                DateTime lockOutTo = currentDate.AddSeconds(seconds);
                                lockOutTime(rowId, lockOutTo);
                            }

                        }
                        else
                        {
                            incorrectAttempt(userId, numberAttempts, currentDate);
                        }
                    }
                    else
                    {
                        test = "You are locked out until " + userLockedOut;
                    }
                }                
            }
            else
            {
                errorMsg = "Invalid username";
            }
            
        }

        public String getUserCode(Int64 userId)
        {
            String code = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT TOP 1 confirm_code FROM confirmCodes WHERE user_id=@userId ORDER BY code_id DESC ";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                code = reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace + " " + ex.Message;
            }
            return code;
        }
        public DateTime getSendedDate(Int64 userId)
        {
            DateTime time = new DateTime();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT TOP 1 sended_date FROM confirmCodes WHERE user_id=@userId ORDER BY code_id DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                time = reader.GetDateTime(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace + " " + ex.Message;
            }
            return time;
        }
        public bool isConfirmCodeSended(Int64 userId, String confirmationCode)
        {
            bool check = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT COUNT(*) FROM confirmCodes WHERE user_id=@userId AND confirm_code=@confirmCode";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@confirmCode", confirmationCode);

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
                errorMsg = ex.StackTrace + " " + ex.Message;
            }
            return check;
        }
        public int expirationTime()
        {
            int time = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT expiration_time FROM wrongPassSettings";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                time = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return time;
        }
        public DateTime getUserLockOutTime(Int64 userId)
        {
            DateTime time = new DateTime();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT TOP 1 timeout_to FROM incorrectAttempts WHERE user_id=@userId ORDER BY Id DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                               time = reader.GetDateTime(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace + " " + ex.Message;
            }
            return time;
        }
        public bool checkIfUserIsExistingInIncorrectAttempts(Int64 userId)
        {
            bool check = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT COUNT(*) FROM incorrectAttempts WHERE user_id=@userId";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
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
                errorMsg = ex.StackTrace + " " + ex.Message;
            }
            return check;
        }
        public String getPassByUsername(String username)
        {
            String pass = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT user_password FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pass = reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return pass;
        }
        public void updateIncorrectAttempt(Int64 rowId, int attempt,DateTime time)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE incorrectAttempts SET number_attempts=@attempts,time=@time WHERE Id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@attempts", attempt);
                        cmd.Parameters.AddWithValue("@time",time);
                        cmd.Parameters.AddWithValue("@id", rowId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "asd" + ex.StackTrace;
            }
        }
        public Int64 getRowId(Int64 userId)
        {
            Int64 rowId = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT TOP 1 Id FROM incorrectAttempts WHERE user_id=@userId ORDER BY Id DESC ";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                rowId = reader.GetInt64(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return rowId;
        }
        public DateTime getTime(Int64 userId)
        {
            DateTime time = new DateTime();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT TOP 1 time FROM incorrectAttempts WHERE user_id=@userId ORDER BY Id DESC ";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("userId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                time = reader.GetDateTime(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return time;
        }
        public void lockOutTime(Int64 rowId, DateTime time)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "UPDATE incorrectAttempts SET timeout_to=@timeout WHERE Id=@id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@timeout", time);
                        cmd.Parameters.AddWithValue("@id", rowId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "asd" + ex.StackTrace + " " + ex.Message;
            }
        }
        public int checkAttempts(Int64 rowId)
        {
            int attempts = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT number_attempts FROM incorrectAttempts WHERE Id=@rowId";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("rowId", rowId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                attempts = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return attempts;
        }

        public void incorrectAttempt(Int64 userId,int attempts,DateTime currentDate)
        {
            DateTime defaultDate = DateTime.Now;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO incorrectAttempts (user_id,number_attempts,time,timeout_to) " +
                        "VALUES (@userId,@numberAttempts,@time,@timeout)";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("numberAttempts", attempts);
                        cmd.Parameters.AddWithValue("time",currentDate);
                        cmd.Parameters.AddWithValue("@timeout",defaultDate);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
        }
        public int userLockOut()
        {
            int time = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT time_out FROM wrongPassSettings";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                time = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return time;
        }
        public int numberOfAttemptsSettings()
        {
            int number = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT number_wrong_tries FROM wrongPassSettings";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                number = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.StackTrace;
            }
            return number;
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
                        cmd.Parameters.AddWithValue("username",username);
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

        public bool checkUsername(String username)
        {
            bool check = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "SELECT COUNT(*) FROM users WHERE user_username=@username";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                       
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
