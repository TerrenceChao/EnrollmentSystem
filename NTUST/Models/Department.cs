using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [StringLength(50, MinimumLength = 3),
         Display(Name="Department")]
        public string Name { get; set; }


        [DataType(DataType.Currency),
         Column(TypeName = "money"), //The "field type" in the table is "money"
         ]
        public decimal Budget { get; set; }


        [DataType(DataType.Date),
         DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true),
         Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }


        public int? InstructorID { get; set; }




        /* Add an Optimistic Concurrency  Property to the Department Entity
         * related code is in DAL.SchoolContext.cs  
         *      modelBuilder.Entity<Department>...IsConcurrencyToken()   */
        [Timestamp]
        public byte[] RowVersion { get; set; }




        public virtual Instructor Administrator { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }
}