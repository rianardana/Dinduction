using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;
public class LoginVM
    {
        [Required]
        
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }