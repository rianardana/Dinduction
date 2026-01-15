using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;
public class UserVM
    {
        public int Id { get; set; }
        
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Department { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "Current Password is required.")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "New Password is required.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Display(Name = "Confirm New Password")]
        public string ConfNewPassword { get; set; }
        [Display(Name = "Employee Name")]

        public string EmployeeName { get; set; }
        public SelectList ListRole { get; set; }
        [Display(Name ="Role")]
        public int RoleId { get; set; }
        [Display(Name = "Role")]
        public string RoleName  { get; set; }
        [Display(Name = "Start Training")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartTraining { get; set; }
        [Display(Name = "End Training")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndTraining { get; set; }
    }