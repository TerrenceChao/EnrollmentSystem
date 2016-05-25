using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NTUST.Models;
using NTUST.DAL;
using System.Data.Entity.Infrastructure;

namespace NTUST.Controllers
{
    public class CourseController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: /Course/
        public async Task<ActionResult> Index(int? SelectedDepartment)
        {
            var departments = await db.Departments.OrderBy(q => q.Name).ToListAsync();
            ViewBag.SelectedDepartment = new SelectList(departments, "DepartmentID", "Name", SelectedDepartment);
            int departmentID = SelectedDepartment.GetValueOrDefault();

            IQueryable<Course> courses = db.Courses
                                        .Where(c => !SelectedDepartment.HasValue || c.DepartmentID == departmentID)
                                        .OrderBy(d => d.CourseID)
                                        .Include(d => d.Department);
            var sql = courses.ToString();
            return View(await courses.ToListAsync());
        }

        // GET: /Course/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        //Importent !
        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in db.Departments
                                   orderby d.Name
                                   select d;
            ViewBag.DepartmentID = new SelectList(departmentsQuery, "DepartmentID", "Name", selectedDepartment);
        }

        // GET: /Course/Create
        public ActionResult Create(bool? createError=false)
        {
            if (createError.GetValueOrDefault())
                ViewBag.ErrorMessage = "There's a duplicate Course Number, " +
                                        "please insert another one";

            //ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            PopulateDepartmentsDropDownList();
            return View();
        }

        // POST: /Course/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="CourseID,Title,Credits,DepartmentID")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check if there is a duplicate "Course Number":
                    var existedCourseID = await db.Courses.FindAsync(course.CourseID);
                    if (existedCourseID != null)
                        return RedirectToAction("Create", new { createError = true});
                    

                    db.Courses.Add(course);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // GET: /Course/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        // POST: /Course/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(int? id, byte[] rowVersion) //Edit([Bind(Include="CourseID,Title,Credits,DepartmentID")] Course course)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string[] fieldsToBind = new string[] { "CourseID", "Title", "Credits", "DepartmentID", "RowVersion" };

            var courseToUpdate = await db.Courses.FindAsync(id);
            //it was deleted in the monent
            if (courseToUpdate == null)
            {
                Course deleteCourse = new Course();
                TryUpdateModel(deleteCourse, "Cos", fieldsToBind);
                ModelState.AddModelError(string.Empty,
                                        "Unable to save changes. This course was deleted by another user.");
                PopulateDepartmentsDropDownList(deleteCourse);
                return View(deleteCourse);
            }

            if (TryUpdateModel(courseToUpdate, "", fieldsToBind))
            {
                try
                {
                    //update Models.Course.RowVersion(the type is timestamp)
                    db.Entry(courseToUpdate).OriginalValues["RowVersion"] = rowVersion;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    //===================================================
                    //                  Very Importent
                    //===================================================
                    /* The value of "courseToUpdate" was found, but the system throws the 
                     * "Concurrency Exception" when you deal with "db.SaveChangesAsync()". 
                     * 
                     * There're 2 situations: 
                     * (1)it was deleted. (2)it was modified and saved to DB already. */
                    var entry = ex.Entries.Single();
                    var clientValues = (Course)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();

                    //case 1: it was deleted.
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                                                "Unable to save changes. The course was deleted by another user.");
                    }
                    // case 2: it was modified and saved to DB already.
                    else
                    {
                        var databaseValues = (Course)databaseEntry.ToObject();
                        if (databaseValues.CourseID != clientValues.CourseID)
                            ModelState.AddModelError("CourseID", "Current value:" + databaseValues.CourseID);

                        if (databaseValues.Title != clientValues.Title)
                            ModelState.AddModelError("Title", "Current value:" + databaseValues.Title);

                        if (databaseValues.Credits != clientValues.Credits)
                            ModelState.AddModelError("Credits", "Current value:" + databaseValues.Credits);

                        if (databaseValues.DepartmentID != clientValues.DepartmentID)
                            ModelState.AddModelError("DepartmentID", "Current value:"
                                                    + db.Departments.Find(databaseValues.DepartmentID).Name);

                        
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                + "was modified by another user after you got the original value. The "
                                                + "edit operation was canceled and the current values in the database "
                                                + "have been displayed. If you still want to edit this record, click "
                                                + "the Save button again. Otherwise click the Back to List hyperlink.");


                        courseToUpdate.RowVersion = databaseValues.RowVersion;
                    }
                }
                catch (RetryLimitExceededException /*dex*/)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);

            //if (ModelState.IsValid)
            //{
            //    db.Entry(course).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            //return View(course);
        }

        // GET: /Course/Delete/5
        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            

            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                if (concurrencyError.GetValueOrDefault())
                    return RedirectToAction("Index");

                return HttpNotFound();
            }

            //It wad modified and saved to DB already.
            if (concurrencyError.GetValueOrDefault())
            {
                /*
                 ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                                                + "was modified by another user after you got the original values. "
                                                + "The delete operation was canceled and the current values in the "
                                                + "database have been displayed. If you still want to delete this "
                                                + "record, click the Delete button again. Otherwise "
                                                + "click the Back to List hyperlink.";
                 */

                ModelState.AddModelError(string.Empty, "The record you attempted to delete "
                                                + "was modified by another user after you got the original value. The "
                                                + "delete operation was canceled and the current values in the database "
                                                + "have been displayed. If you still want to delete this record, click "
                                                + "the Delete button again. Otherwise click the Back to List hyperlink.");
            }
            return View(course);
        }

        // POST: /Course/Delete/5
        [HttpPost]  //[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Course course)//DeleteConfirmed(int id)
        {
            try
            {
                db.Entry(course).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch(DbUpdateConcurrencyException)
            {/* Concurrency Exception: 
                 * It could be "deleted" OR "modefied". */
                return RedirectToAction("Delete", new { id=course.CourseID, concurrencyError = true});
            }
            catch (DataException /*dex*/)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(course);
            }
            /*
             Course course = await db.Courses.FindAsync(id);
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
             */
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
