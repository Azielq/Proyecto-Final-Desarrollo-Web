using System;
using System.ComponentModel.DataAnnotations;

namespace FarmaU.Models
{
    public class MyEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}