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
    /* Concurrency Conflicts 
     * A concurrency conflict occurs when one user displays an entity's data in order to edit it, 
     * and then another user updates the same entity's data before the first user's change is written 
     * to the database. If you don't enable the detection of such conflicts, whoever updates the database 
     * last overwrites the other user's changes. In many applications, this risk is acceptable: if there 
     * are few users, or few updates, or if isn't really critical if some changes are overwritten, the cost 
     * of programming for concurrency might outweigh the benefit. In that case, you don't have to configure 
     * the application to handle concurrency conflicts. 
     
     
     *      "Pessimistic Concurrency (Locking)"
     * If your application does need to prevent accidental data loss in concurrency scenarios, one way to do that 
     * is to use database locks. This is called pessimistic concurrency. For example, before you read a row from 
     * a database, you request a lock for read-only or for update access. If you lock a row for update access, 
     * no other users are allowed to lock the row either for read-only or update access, because they would get 
     * a copy of data that's in the process of being changed. If you lock a row for read-only access, others can 
     * also lock it for read-only access but not for update.
     * 
     * Managing locks has disadvantages. It can be complex to program. It requires significant database 
     * management resources, and it can cause performance problems as the number of users of an application increases. 
     * For these reasons, not all database management systems support pessimistic concurrency. The Entity Framework 
     * provides no built-in support for it, and this tutorial doesn't show you how to implement it.
    
     
     *      "Optimistic Concurrency" 
     * The alternative to pessimistic concurrency is optimistic concurrency. Optimistic concurrency means allowing 
     * concurrency conflicts to happen, and then reacting appropriately if they do. For example, John runs the 
     * Departments Edit page, changes the Budget amount for the English department from $350,000.00 to $0.00....
     * http://www.asp.net/mvc/overview/getting-started/getting-started-with-ef-using-mvc/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application
     * 
     * ...Detecting Concurrency Conflicts...
     */
    public class DepartmentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: /Department/
        public async Task<ActionResult> Index()
        {
            var departments = db.Departments.Include(d => d.Administrator);
            return View(await departments.ToListAsync());
        }

        // GET: /Department/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // GET: /Department/Create
        public ActionResult Create()
        {
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName");
            return View();
        }

        // POST: /Department/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include="DepartmentID,Name,Budget,StartDate,InstructorID")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // GET: /Department/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
        }

        // POST: /Department/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, byte[] rowVersion) //Edit([Bind(Include="DepartmentID,Name,Budget,StartDate,InstructorID")] Department department)
        {
            string[] fieldsToBind = new string[] { "Name", "Budget", "StartDate", "InstructorID", "RowVersion"};

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var departmentToUpdate = await db.Departments.FindAsync(id);
            if (departmentToUpdate == null) 
            {
                Department deleteDepartment = new Department();
                TryUpdateModel(deleteDepartment, "", fieldsToBind);
                ModelState.AddModelError(string.Empty,
                                        "Unable to save changes. The department was deleted by another user.");
                ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", deleteDepartment);
                return View(deleteDepartment);
            }

            if (TryUpdateModel(departmentToUpdate, fieldsToBind))
            {
                try
                {   //update Models.Department.RowVersion (the type is timestamp)
                    db.Entry(departmentToUpdate).OriginalValues["RowVersion"] = rowVersion;
                    await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                /* The process of concurrency conflict. */
                catch (DbUpdateConcurrencyException ex) //using System.Data.Entity.Infrastructure;
                {
                    //===================================================
                    //                  Very Importent
                    //===================================================
                    /* The value of "departmentToUpdate" was found, but the system throws the 
                     * "Concurrency Exception" when you deal with "db.SaveChangesAsync()". 
                     * 
                     * There're 2 situations: 
                     * (1)it was deleted. (2)it was modified and saved to DB already. */
                    var entry = ex.Entries.Single();
                    var clientValues = (Department)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();

                    //case 1: it was deleted.
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                                                "Unable to save changes. The department was deleted by another user.");
                    }
                    // case 2: it was modified and saved to DB already.
                    else
                    {
                        var databaseValues = (Department)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", "Current value:" + databaseValues.Name);

                        if (databaseValues.Budget != clientValues.Budget)
                            ModelState.AddModelError("Budget", "Current value:" 
                                                    + String.Format("{0:c}", databaseValues.Budget));

                        if (databaseValues.StartDate != clientValues.StartDate)
                            ModelState.AddModelError("StartDate", "Current value:"
                                                    + string.Format("{0:d}", databaseValues.StartDate));

                        if (databaseValues.InstructorID != clientValues.InstructorID)
                            ModelState.AddModelError("InstructorID", "Current value:"
                                                    + db.Instructors.Find(databaseValues.InstructorID).FullName);

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                                + "was modified by another user after you got the original value. The "
                                                + "edit operation was canceled and the current values in the database "
                                                + "have been displayed. If you still want to edit this record, click "
                                                + "the Save button again. Otherwise click the Back to List hyperlink.");

                        departmentToUpdate.RowVersion = databaseValues.RowVersion;
                    }
                }
                catch (RetryLimitExceededException /* dex */) //using System.Data.Entity.Infrastructure;
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", departmentToUpdate.InstructorID);
            return View(departmentToUpdate);

            /*
            if (ModelState.IsValid)
            {
                db.Entry(department).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.InstructorID = new SelectList(db.Instructors, "ID", "FullName", department.InstructorID);
            return View(department);
             */
        }

        // GET: /Department/Delete/5
        public async Task<ActionResult> Delete(int? id, bool? concurrencyError)
        {
            /* The method accepts an optional parameter(concurrencyError) that indicates whether the page 
             * is being redisplayed after a concurrency error. If this flag is true, an error message is sent 
             * to the view using a ViewBag property.
             */
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            Department department = await db.Departments.FindAsync(id);

            //It was deleted or ...
            if (department == null)
            {
                if (concurrencyError.GetValueOrDefault())
                    return RedirectToAction("Index");

                return HttpNotFound();
            }

            //It wad modified and saved to DB already.
            if (concurrencyError.GetValueOrDefault())
            {
                ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                                                + "was modified by another user after you got the original values. "
                                                + "The delete operation was canceled and the current values in the "
                                                + "database have been displayed. If you still want to delete this "
                                                + "record, click the Delete button again. Otherwise "
                                                + "click the Back to List hyperlink.";
            }
            return View(department);

            /* 
            if (department == null)
                return HttpNotFound();
            
            return View(department);
             */
        }

        // POST: /Department/Delete/5
        [HttpPost] //[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Department department)//DeleteConfirmed(int id)
        {
            try
            {
                db.Entry(department).State = EntityState.Deleted;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch(DbUpdateConcurrencyException)
            {   /* Concurrency Exception: 
                 * It could be "deleted" OR "modefied". */
                return RedirectToAction("Delete", new { concurrencyError = true, id = department.DepartmentID });
            }
            catch(DataException /*dex*/)
            {
                //Log the error (uncomment dex variable name after DataException and add a line here to write a log.
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(department);
            }

            /*
            Department department = await db.Departments.FindAsync(id);
            db.Departments.Remove(department);
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
