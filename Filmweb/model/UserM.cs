using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmweb.Model
{
    class UserM
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public MovieM FavouriteMovie { get; set; }
        public String FavouriteActor { get; set; }
    

    public UserM GetUserModel()
        {
            return new UserM
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Username = Username,
                Password = Password 
            };
        }

    }   
}
