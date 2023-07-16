using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class Privlidge
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string RoleId { get; set; }
        public int ActionId { get; set; }
        public bool Allow { get; set; }
        //[ForeignKey("RoleId")]
        //public virtual Role Role { get; set; }
        [ForeignKey("ActionId")]
        public virtual MenuAction Action { get; set; }
    }
}
