using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NTUST.Models
{
    public class OfficeAssignment
    {
        /*
         * There's a one-to-zero-or-one relationship  between the Instructor and the OfficeAssignment entities. 
         * An office assignment only exists in relation to the instructor it's assigned to, and therefore its 
         * primary key is also its foreign key to the Instructor entity. But the Entity Framework can't automatically 
         * recognize InstructorID as the primary key of this entity because its name doesn't follow the ID or 
         * classnameID naming convention. Therefore, the Key attribute is used to identify it as the key:
         */
        [Key,
         ForeignKey("Instructor")]
        public int InstructorID { get; set; }

        [StringLength(50),
         Display(Name = "Office Location")]
        public string Location { get; set; }

        public virtual Instructor Instructor { get; set; }
    }
}