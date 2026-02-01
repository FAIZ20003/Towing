using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proffessional.Data;

namespace Proffessional.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // 🚀 Use one base query (NO TRACKING = FAST)
                var casesQuery = _context.TowingCases.AsNoTracking();

                // 📊 KPI COUNTS
                ViewBag.TotalCases = casesQuery.Count();
                ViewBag.OpenCases = casesQuery.Count(x => x.Status != "Closed");
                ViewBag.ClosedCases = casesQuery.Count(x => x.Status == "Closed");
                ViewBag.TodayCases = casesQuery.Count(x =>
                    x.CreatedDate >= today && x.CreatedDate < tomorrow
                );

                // 👤 STAFF WORKLOAD (OPTIMIZED)
                var staffWorkload = (
                    from s in _context.Staff.AsNoTracking()
                    join c in casesQuery
                        on s.StaffId equals c.AssignedStaffId into caseJoin
                    select new
                    {
                        StaffName = s.StaffName,
                        Total = caseJoin.Count(),
                        Open = caseJoin.Count(x => x.Status != "Closed"),
                        Closed = caseJoin.Count(x => x.Status == "Closed")
                    }
                ).ToList();

                return View(staffWorkload);
            }
            catch (Exception ex)
            {
                // ⚠️ Prevent dashboard crash
                ViewBag.TotalCases = 0;
                ViewBag.OpenCases = 0;
                ViewBag.ClosedCases = 0;
                ViewBag.TodayCases = 0;

                return View(new List<object>());
            }
        }

    }
}
