using System.ComponentModel.DataAnnotations;

namespace Dinduction.Web.Models;
public class SectionVM
    {
        public int Id { get; set; }
        [Display(Name = "Section Name")]
        public string SectionName { get; set; }
    }