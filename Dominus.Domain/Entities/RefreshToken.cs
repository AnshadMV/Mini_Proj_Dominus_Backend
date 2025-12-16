using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominus.Domain.Entities
{
        public class RefreshToken
        {
            public int Id { get; set; }
            public string Token { get; set; } = string.Empty;
            public DateTime Expires { get; set; }
            public DateTime Created { get; set; }
            public DateTime? Revoked { get; set; }

            // Relationships
            public int UserId { get; set; }
            public User User { get; set; }

            public bool IsExpired => DateTime.UtcNow >= Expires;
            public bool IsRevoked => Revoked != null;
            public bool IsActive => !IsRevoked && !IsExpired;
        }

    
}







