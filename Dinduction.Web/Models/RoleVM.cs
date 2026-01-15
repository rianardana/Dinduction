using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;

    public class RoleVM
    {
        public int Id { get; set; }
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
