using System.ComponentModel.DataAnnotations;

namespace Allup.Models
{
    public class RegisterViewModel
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}
