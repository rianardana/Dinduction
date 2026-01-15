using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dinduction.Web.Models;
public class TrainerVM
    {
        public int Id { get; set; }
        [Display(Name = "Trainer Name")]
        public string UserName { get; set; }
        public string TrainerName { get; set; }
        [Display(Name = "Section Name")]
        public int SectionId { get; set; }
        [Display(Name = "User Name")]
        public int UserId { get; set; }
        [Display(Name = "Section Name")]
        public string SectionName { get; set; }
        public string Signature { get; set; }
        public SelectList ListSection { get; set; }
        public SelectList ListUser { get; set; }
    }