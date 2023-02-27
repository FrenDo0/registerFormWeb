using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testWeb.Pages
{
    public class User
    {
       

        public String Username { get; set; }
        public String Password { get; set; }
        public String FirstName { get; set; }
        public String SecondName { get; set; }
        public String Email { get; set; }
        public String userId { get; set; }


        public User() { }

        /*public User(string username,string password,string firstName,string secondName,string email)
        {
            this.Username = username;
            this.Password = password;
            this.FirstName = firstName;
            this.SecondName = secondName;
            this.Email = email;
        }*/
        
    }
}
