using Microsoft.AspNetCore.Identity;

namespace Allup.DAL.Entities
{
    public class User : IdentityUser
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
    }
}
