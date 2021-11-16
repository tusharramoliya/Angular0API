using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities
{
    public class UserViewModel
    {
        public long id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public string token { get; set; }
        public string message { get; set; }
    }

    public class CreateUserRequest
    {
        public long Id { get; set; }
        public string firstname { get; set; }

        public string lastname { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
        public string message { get; set; }
    }

    public class CreateUserResponse
    {
        public bool issuccess { get; set; } = false;
        public string message { get; set; }
    }

    public class DeleteUserResponse
    {
        public bool issuccess { get; set; } = false;
        public string message { get; set; }
    }
}