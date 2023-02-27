using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace testWeb.Pages
{
    public class IndexModel : PageModel
    {
        public String connectionString = "Data Source=.\\tew_sqlexpress;Initial Catalog=storeNumbers;Integrated Security=True";
        public bool contains = false;
        public String errorMsg;
        public void OnPost()
        {
            User user = new User();          
            user.Username = Request.Form["username"];
            user.Password = Request.Form["password"];
            user.FirstName = Request.Form["firstName"];
            user.SecondName = Request.Form["secondName"];
            user.Email = Request.Form["email"];
            contains = true;
            bool checker = false;
            String confirmPass = Request.Form["confirmPass"];
           

            if(user.Username.Length == 0)
            {
                errorMsg = "Enter username";
                checker = false;
            }
            else if (user.Password.Length == 0)
            {
                errorMsg = "Enter password";
                checker = false;
            }
            else if (confirmPass.Length == 0 || !confirmPass.Equals(user.Password))
            {
                errorMsg = "Confirm your password !";
                checker = false;
            }
            else if (user.FirstName.Length == 0)
            {
                errorMsg = "Enter first name";
                checker = false;
            }
            else if (user.SecondName.Length == 0)
            {
                errorMsg = "Enter second name";
                checker = false;
            }
            else if (user.Email.Length == 0)
            {
                errorMsg = "Enter email";
                checker = false;
            }
            else
            {
                checker = true;
            }
            
            if(checker == true)
            {
                insertUser(user);
            }
        }

        public void insertUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    String sql = "INSERT INTO users (user_username,user_password,user_firstName,user_secondName,user_email) " +
                        "VALUES (@username,@pass,@fName,@sName,@email)";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@username", user.Username);
                        cmd.Parameters.AddWithValue("@pass", user.Password);
                        cmd.Parameters.AddWithValue("@fName", user.FirstName);
                        cmd.Parameters.AddWithValue("@sName", user.SecondName);
                        cmd.Parameters.AddWithValue("@email", user.Email);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = "Already existing username or email !";
            }
        }
    }
}
