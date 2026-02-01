using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Proffessional.Data;
using System.Linq;

namespace Proffessional.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()   // or Report / List / Export
        {
            ViewBag.StaffList = _context.Staff
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    s.StaffId,
                    s.StaffName
                })
                .ToList();

            return View();
        }

        // ================= EXPORT ALL CASES =================
        [HttpGet]
        public IActionResult ExportCases(DateTime? fromDate, DateTime? toDate, int? staffId)
        {
            var query =
                from c in _context.TowingCases
                join s in _context.Staff
                    on c.AssignedStaffId equals s.StaffId into staffJoin
                from s in staffJoin.DefaultIfEmpty()
                select new
                {
                    c.CaseId,
                    c.CustomerName,
                    c.CustomerContactNumber,
                    c.VehicleBrand,
                    c.RegistrationNo,
                    c.IncidentReason,
                    c.Status,
                    c.CreatedDate,
                    c.AssignedStaffId,                 // ✅ KEEP THIS
                    AssignedStaff = s != null ? s.StaffName : "Not Assigned"
                };

            // 📅 DATE FILTER
            if (fromDate.HasValue)
                query = query.Where(x => x.CreatedDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.CreatedDate.Date <= toDate.Value.Date);

            // 👤 STAFF FILTER (THIS IS THE FIX)
            if (staffId.HasValue && staffId.Value > 0)
                query = query.Where(x => x.AssignedStaffId == staffId.Value);

            var data = query
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Cases");

            // HEADERS
            string[] headers =
            {
        "Case ID","Customer","Contact","Vehicle",
        "Registration","Reason","Status","Assigned Staff","Created Date"
    };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1].Value = headers[i];

            // DATA
            for (int i = 0; i < data.Count; i++)
            {
                var r = data[i];
                sheet.Cells[i + 2, 1].Value = r.CaseId;
                sheet.Cells[i + 2, 2].Value = r.CustomerName;
                sheet.Cells[i + 2, 3].Value = r.CustomerContactNumber;
                sheet.Cells[i + 2, 4].Value = r.VehicleBrand;
                sheet.Cells[i + 2, 5].Value = r.RegistrationNo;
                sheet.Cells[i + 2, 6].Value = r.IncidentReason;
                sheet.Cells[i + 2, 7].Value = r.Status;
                sheet.Cells[i + 2, 8].Value = r.AssignedStaff;
                sheet.Cells[i + 2, 9].Value = r.CreatedDate.ToString("dd-MMM-yyyy");
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TowingCases_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

    }
}
