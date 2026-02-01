using Microsoft.AspNetCore.Mvc;
using Proffessional.Data;

namespace Proffessional.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var count = _context.TowingCases.Count();
            return Content("✅ DB Connected Successfully. Total Cases: " + count);
        }
    }
}
