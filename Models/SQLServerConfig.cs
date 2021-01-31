using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileConverter.Models
{
    public class SQLServerConfig
    {
        [Required(ErrorMessage = "Please enter the server name")]
        [Display(Name = "Server name")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        public string Server { get; set; }

        [Required(ErrorMessage = "Please enter your user id")]
        [Display(Name = "User name")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(50)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter the database name")]
        [Display(Name = "Database")]
        [DataType(DataType.Text)]
        [StringLength(50)]
        public string Database { get; set; }
        public string ConString { get; set; }
        
    }
}
