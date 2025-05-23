using Microsoft.AspNetCore.Identity;

namespace Library_Managment.Models
{
    public enum UserType
    {
        NONE, ADMIN, STUDENT
    }
    public enum AccountStatus
    {
        UNAPROOVED, ACTIVE, BLOCKED
    }

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public UserType UserType { get; set; } = UserType.NONE;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.UNAPROOVED;
        public DateTime CreatedOn { get; set; }
    }
}
