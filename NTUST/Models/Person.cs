using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public abstract class Person
    {
        public int ID { get; set; }

        [Required,
         StringLength(50, MinimumLength=1),
         Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required,
         StringLength(50, MinimumLength = 1,
                    ErrorMessage = "First name cannot be longer than 50 characters."),
         Column("FirstName"), //Define the field name is "FirstName" in DB
         Display(Name = "First Name")]
        public string FirstMidName { get; set; }

        [Display(Name="Full Name")]
        public string FullName
        {
            get { return LastName + "  " + FirstMidName;  }
        }

    }
}