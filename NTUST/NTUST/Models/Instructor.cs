using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public class Instructor : Person
    {
        private ICollection<Course> mCourses;

        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        public virtual ICollection<Course> Courses
        {
            get
            {
                return mCourses ?? (mCourses = new List<Course>());
            }
            set
            {
                mCourses = value;
            }
        }
        public virtual OfficeAssignment OfficeAssignment { get; set; }

        /* 
         * ==========================================
         *              legacy code
         * ==========================================
         * 
         * This class inherit from "Person" and the following lines are unnecessary:
         * 
                public int ID { get; set; }

                [Required,
                 Display(Name = "Last Name"),
                StringLength(50)]
                public string LastName { get; set; }

                [Required,
                 Column("FirstName"), //Define field'name in Table is "FirstName",
                 Display(Name = "First Name"),
                 StringLength(50)]
                public string FirstMidName { get; set; }

                [Display(Name = "Full Name")]
                public string FullName
                {
                    get { return LastName + "  " + FirstMidName; }
                }
         */
    }
}