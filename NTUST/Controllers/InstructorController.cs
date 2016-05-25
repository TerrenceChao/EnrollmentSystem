using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using NTUST.Models;
using NTUST.DAL;
using NTUST.ViewModels;
using System.Data.Entity.Infrastructure;

namespace NTUST.Controllers
{
    public class InstructorController : Controller
    {
        private SchoolContext db = new SchoolContext();


        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (Course course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }
        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<Course>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.Courses.Select(c => c.CourseID));

            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                        instructorToUpdate.Courses.Add(course);
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                        instructorToUpdate.Courses.Remove(course);
                }
            }
        }

        // GET: /Instructor/
        public ActionResult Index(int? id, int? courseID) //Both id & courseID could be NULL
        {
            //var instructors = db.Instructors.Include(i => i.OfficeAssignment);
            //return View(instructors.ToList());
            var viewModel = new InstructorIndexData();


            //using System.Data.Entity; 
            //Or try not using it. XD
            viewModel.Instructors = db.Instructors
                                    .Include(i => i.OfficeAssignment)
                                    .Include(i => i.Courses.Select(c => c.Department))
                                    .OrderBy(i => i.LastName);

            if (id != null)
            {
                ViewBag.InstructorID = id.Value;//ViewBag is used on Index.cshtml
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.ID == id.Value).Single().Courses;
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;//ViewBag is used on Index.cshtml

                //// Case 1:  Lazy loading
                //viewModel.Enrollments = viewModel.Courses.Where(
                //    c => c.CourseID == courseID.Value).Single().Enrollments;


                // Case 2: Explicit loading 
                /* Run the Instructor Index page now and you'll see "no difference" in what's displayed on the page, 
                 * although you've changed how the data is retrieved.
                 * Notice that you use the "Collection method" to load a collection property, but for a property 
                 * that holds just one entity, you use the Reference method.
                */
                var selectedCourse = viewModel.Courses.Where(c => c.CourseID==courseID).Single();
                db.Entry(selectedCourse).Collection(c => c.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(c => c.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }

        // GET: /Instructor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // GET: /Instructor/Create
        public ActionResult Create()
        {
            var instructor = new Instructor();
            instructor.Courses = new List<Course>();
            PopulateAssignedCourseData(instructor);
            return View();

            //ViewBag.ID = new SelectList(db.OfficeAssignments, "InstructorID", "Location");
            //return View();
        }

        // POST: /Instructor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="LastName,FirstMidName,HireDate, OfficeAssignment")] Instructor instructor,
                                    string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                //Modify the "Instructor" model and you can remove the line bellow:
                //instructor.Courses = new List<Course>(); 
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    instructor.Courses.Add(courseToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.ID = new SelectList(db.OfficeAssignments, "InstructorID", "Location", instructor.ID);
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: /Instructor/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Instructor instructor = db.Instructors.Find(id);

            //Eager loading
            /* This code drops the ViewBag statement(ViewBag.ID = new ...) and adds eager loading for the associated 
             * OfficeAssignment entity. You can't perform eager loading with the Find method, so the Where and Single methods 
             * are used instead to select the instructor.*/
            Instructor instructor = db.Instructors
                                    .Include(i => i.OfficeAssignment)
                                    .Include(i => i.Courses)
                                    .Where(i => i.ID == id)
                                    .Single();

            PopulateAssignedCourseData(instructor);

            if (instructor == null)
            {
                return HttpNotFound();
            }
            //ViewBag.ID = new SelectList(db.OfficeAssignments, "InstructorID", "Location", instructor.ID);
            return View(instructor);
        }

        // POST: /Instructor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, string[] selectedCourses) //Edit([Bind(Include="ID,LastName,FirstMidName,HireDate")] Instructor instructor)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Eager loading
            var instructorToUpdate = db.Instructors
                                        .Where(i => i.ID == id)
                                        .Include(i => i.OfficeAssignment)
                                        .Include(i => i.Courses)
                                        .Single();

            /* Updates the retrieved Instructor entity with values from the model binder. The TryUpdateModel overload used 
             * enables you to whitelist the properties you want to include. 
             * "This prevents over-posting". */
            if(TryUpdateModel(instructorToUpdate, "",
                new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
            {
                try
                {
                    //spelling check
                    /* If the office location is blank, sets the Instructor.OfficeAssignment property to null so that the related row 
                     * in the OfficeAssignment table will be deleted. */
                    if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                        instructorToUpdate.OfficeAssignment = null;

                    UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                    db.SaveChanges();
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(instructorToUpdate);
            return RedirectToAction("Index");

            //if (ModelState.IsValid)
            //{
            //    db.Entry(instructor).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //ViewBag.ID = new SelectList(db.OfficeAssignments, "InstructorID", "Location", instructor.ID);
            //return View(instructor);
        }

        // GET: /Instructor/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // POST: /Instructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Instructor instructor = db.Instructors
                                     .Include(i => i.OfficeAssignment)
                                     .Where(i => i.ID == id)
                                     .Single();

            db.Instructors.Remove(instructor);

            var department = db.Departments
                                .Where(d => d.InstructorID == id)
                                .SingleOrDefault();

            if (department != null)
                department.InstructorID = null;

            db.SaveChanges();
            return RedirectToAction("Index");

            //Instructor instructor = db.Instructors.Find(id);
            //db.Instructors.Remove(instructor);
            //db.SaveChanges();
            //return RedirectToAction("Index");
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
