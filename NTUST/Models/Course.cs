using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public class Course
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None),
         Display(Name="Course number")]
        public int CourseID { get; set; }

        [StringLength(50, MinimumLength=3)]
        public string Title { get; set; }

        [Range(0, 5)]
        public int Credits { get; set; }

        public int DepartmentID { get; set; }


        /* Add an Optimistic Concurrency  Property to the Department Entity
         * related code is in DAL.SchoolContext.cs  
         *      modelBuilder.Entity<Course>...IsConcurrencyToken()   */
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Instructor> Instructors { get; set; }
    }
}