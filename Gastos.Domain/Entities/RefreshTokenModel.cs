using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gastos.Domain.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("expires")]
        public DateTime Expires { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("created_by_ip")]
        public string CreatedByIp { get; set; }

        [Column("is_revoked")]
        public bool IsRevoked { get; set; }

        [Column("revoked")]
        public DateTime? Revoked { get; set; }

        [Column("revoked_by_ip")]
        public string RevokedByIp { get; set; }

        [Column("reason_revoked")]
        public string ReasonRevoked { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }
    }
}
