using System.ComponentModel.DataAnnotations;

namespace SqlBuilderSamplesAndTests
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Username { get; set; }
    }
}