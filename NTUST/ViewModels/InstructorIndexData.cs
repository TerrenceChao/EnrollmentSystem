using System.Collections.Generic;
using NTUST.Models;

namespace NTUST.ViewModels
{
    public class InstructorIndexData
    {
        public IEnumerable<Instructor> Instructors { set; get; }
        public IEnumerable<Course> Courses { set; get; }
        public IEnumerable<Enrollment> Enrollments { set; get; }
    }
}