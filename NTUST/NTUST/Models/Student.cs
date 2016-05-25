using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public class Student : Person
    {
        private ICollection<Enrollment> mEnrollments;

        [DisplayName("Enrollment Date")] // = Display(Name = "Enrollment Date")
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}", ApplyFormatInEditMode=true)]
        public DateTime EnrollmentDate { get; set; }

        public virtual ICollection<Enrollment> Enrollments
        {
            get
            {
                return mEnrollments ?? (mEnrollments = new List<Enrollment>());
            }
            set{ mEnrollments = value; }
        }


        /* 
         * ==========================================
         *              legacy code
         * ==========================================
         * 
         * This class inherit from "Person" and the following lines are unnecessary:
         * 
                public int ID { get; set; }

                [Required,
                 DisplayName("Last Name"), //The same as "Display(Name = "Last Name")"
                 StringLength(50, MinimumLength=1, //Min length
                                ErrorMessage="Last name cannot be longer than 50 characters.")]
                public string LastName { get; set; }

                [Required]
                [Column("FirstName"), // Rename(or Define) the table's field from "FirstMidName" to "FirstName"
                 Display(Name = "First Name"), // = DisplayName("First Name")
                 StringLength(50, MinimumLength = 1, //Min length
                                ErrorMessage="First name cannot be longer than 50 characters.")]
                public string FirstMidName { get; set; }

                [Display(Name = "Full Name")] // = DisplayName("Full Name")
                public string FullName
                {
                    get { return LastName + "  " + FirstMidName; }
                }
         */
    }
}