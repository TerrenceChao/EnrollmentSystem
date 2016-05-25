using System.Web.Mvc;
using System.Linq;
using NTUST.DAL;
using NTUST.ViewModels;

namespace NTUST.Controllers
{
    public class HomeController : Controller
    {
        private SchoolContext db = new SchoolContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";
            IQueryable<EnrollmentDateGroup> data =  from students in db.Students
                                                    group students by students.EnrollmentDate into dateGroup
                                                    select new EnrollmentDateGroup
                                                    {
                                                        EnrollmentDate = dateGroup.Key,
                                                        StudentCount = dateGroup.Count()
                                                    };
                                                    
            return View(data.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}