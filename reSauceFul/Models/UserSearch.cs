using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace reSauceFul.Models
{
    public class UserSearch
    {
        [Required(ErrorMessage = "Please enter a some ingredients e.g chicken, lemon, whole milk")]
        [RegularExpression("^[a-zA-Z, ]*$", ErrorMessage = "Please enter ingredients e.g. chicken, whole milk, butter")]
        [Display(Name = "Enter Ingredients .e.g lemon, whole milk")]
        [DataType(DataType.MultilineText)]
        public string Ingredients { get; set; }
    }
}