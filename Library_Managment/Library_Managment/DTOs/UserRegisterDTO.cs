using System.ComponentModel.DataAnnotations;
using Library_Managment.Models;

namespace Library_Managment.DTOs
{
    public class UserRegisterDTO
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string mobilePhone { get; set; }
        public string password { get; set; }
        [Compare("password")]
        public string confirmPassword { get; set; }

        public UserType userType { get; set; }
        public AccountStatus accountStatus { get; set; }

        public DateTime createOn { get; set; }
    }
}
