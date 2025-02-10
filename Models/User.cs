using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace oAuthJWT.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string GoogleId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? CurrentToken { get; set; }
        public DateTime? TokenExpiration { get; set; }
    }
}