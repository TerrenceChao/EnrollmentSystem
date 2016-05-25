using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;
using NTUST.Models;
using NTUST.DAL;
using NTUST.ViewModels;
using PagedList;

namespace NTUST.Controllers
{
    public class StudentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        private void PopulateAssignedCourseData(Student student)
        {
            var allCourses = db.Courses.ToList();
            var viewModel = new HashSet<AssignedCourseData>();

            //Remove the existed Courses in Student.Enrollments 
            if (allCourses != null)
                foreach (Enrollment enrollment in student.Enrollments)
                    if(allCourses.Contains(enrollment.Course))
                        allCourses.Remove(enrollment.Course);


            //Set selected Courses
            foreach (Enrollment enrollment in student.Enrollments)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = enrollment.CourseID,
                    Title = enrollment.Course.Title,
                    Assigned = true //selected
                });
            }

            //Set unselected Courses
            foreach (Course course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = false //unselected
                });
            }

            //Two foreach loop and no nested: Big O(n)
            ViewBag.Courses = viewModel;
        }

        private void UpdateStudentCourses(string[] selectedCourses, Student studentToUpdate)
        {
            if (selectedCourses == null)
            {
                (studentToUpdate.Enrollments).Clear();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var studentCourses = new HashSet<int>(studentToUpdate.Enrollments.Select(c => c.CourseID));
            int updatedStudentID = studentToUpdate.ID;
            
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!studentCourses.Contains(course.CourseID))
                    {
                        Enrollment enrollment = new Enrollment();
                        enrollment.CourseID = course.CourseID;
                        enrollment.StudentID = updatedStudentID;
                        db.Enrollments.Add(enrollment); //or studentToUpdate.Enrollments.Add(enrollment);
                    }
                }
                else
                {
                    if (studentCourses.Contains(course.CourseID))
                    {
                        var removeEnrollment = db.Enrollments.Where(e => e.CourseID == course.CourseID
                                                                && e.StudentID == updatedStudentID)
                                                                .Single();
                        db.Enrollments.Remove(removeEnrollment);
                    }
                }
            }
        }

        // GET: /Student/
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            ////Student Collection
            var students = from s in db.Students select s;

            //Student filter by name
            if (!String.IsNullOrEmpty(searchString)) { 
                students = students.Where(s => s.LastName.Contains(searchString) ||
                                                        s.FirstMidName.Contains(searchString));               
            }

            //Insert enrollments for each Student
            foreach (Student s in students)
                s.Enrollments = db.Enrollments.Where(e => e.StudentID == s.ID).ToList();

            
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;

                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;

                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;

                default:
                    students = students.OrderBy(s => s.LastName);
                    break;

            }

            return View(students.ToList());
        }

        // GET: /Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: /Student/Create
        public ActionResult Create()
        {
            var student = new Student();
            student.Enrollments = new List<Enrollment>();
            PopulateAssignedCourseData(student);
            return View();
        }

        // POST: /Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ID,LastName,FirstMidName,EnrollmentDate")] Student student,
                                    string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                int studentID = student.ID;
                foreach (var course in selectedCourses)
                {
                   // var courseToAdd = db.Courses.Find(int.Parse(course));
                    var enrollment = new Enrollment();
                    enrollment.CourseID = int.Parse(course);//courseToAdd.CourseID;
                    enrollment.StudentID = studentID;
                    student.Enrollments.Add(enrollment);
                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    db.Students.Add(student);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error
                ModelState.AddModelError("", "Unable to save changes. Try again, and "
                        + "if the problem persists see your system administrator.");
            }
            PopulateAssignedCourseData(student);
            return View(student);
        }

        // GET: /Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            PopulateAssignedCourseData(student);
            return View(student);
        }

        // POST: /Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want  
        // to bind to, formore details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, string[] selectedCourses)//EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var studentToUpdate = db.Students.Find(id);
            if (TryUpdateModel(studentToUpdate, "",
                new string[] { "LastName", "FirstMidName", "EnrollmentDate" })) 
            {
                try
                {
                    UpdateStudentCourses(selectedCourses, studentToUpdate); /* New applied */
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and " 
                        + "if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(studentToUpdate); /* New applied */
            return View(studentToUpdate); 

        }

        // GET: /Student/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError=false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and " 
                    + "if the problem persists see your system administrator.";
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: /Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Student student = db.Students.Find(id);
                db.Students.Remove(student);
                db.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
