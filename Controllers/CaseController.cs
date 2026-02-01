using Microsoft.AspNetCore.Mvc;
using Proffessional.Data;
using Proffessional.Models;
using Proffessional.Services;
using Proffessional.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Proffessional.Controllers
{
    public class CaseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaseController(ApplicationDbContext context)
        {
            _context = context;
        }



        // ================= CREATE =================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.StaffList = _context.Staff
                .Where(s => s.IsActive)
                .Select(s => new { s.StaffId, s.StaffName })
                .ToList();

            return View();
        }

        // ================= PARSE =================
  
        [HttpPost]
        public IActionResult ParseMessage([FromBody] ParseMessageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RawMessage))
                return Json(new List<TowingCase>());

            var cases = CaseParser.Parse(req.RawMessage); // MUST return List<TowingCase>

            return Json(cases);
        }

        // ================= SAVE =================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(IFormCollection form)
        {
            try
            {
                // 🔐 Get logged-in UserId (FK SAFE)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User not logged in");

                int createdByUserId = int.Parse(userIdClaim.Value);

                var caseId = form["CaseId"].ToString().Trim();

                if (string.IsNullOrWhiteSpace(caseId))
                    return Content("❌ Case ID is required");

                if (_context.TowingCases.Any(x => x.CaseId == caseId))
                    return Content("❌ Case ID already exists. Please enter a NEW Case ID.");

                if (string.IsNullOrWhiteSpace(form["AssignedStaffId"]))
                    return Content("❌ Please assign staff before saving.");

                int assignedStaffId = int.Parse(form["AssignedStaffId"]);

                var model = new TowingCase
                {
                    CaseId = caseId,
                    CustomerName = form["CustomerName"],
                    VehicleBrand = form["VehicleBrand"],
                    Model = form["Model"],
                    RegistrationNo = form["RegistrationNo"],
                    ChassisNo = form["ChassisNo"],
                    CustomerContactNumber = form["CustomerContactNumber"],
                    IncidentReason = form["IncidentReason"],
                    IncidentPlace = form["IncidentPlace"],

                    DropLocation = form["DropLocation"],
                    AssignedVendorName = form["AssignedVendorName"],
                    VendorContactNumber = form["VendorContactNumber"],
                    TowingType = form["TowingType"],

                    AssignedStaffId = assignedStaffId,

                    CreatedDate = DateTime.Now,
                    Status = "New",
                    CreatedBy = createdByUserId
                };

                _context.TowingCases.Add(model);
                _context.SaveChanges();
                
                _context.CaseHistory.Add(new CaseHistory
                {
                    CaseId = model.CaseId,
                    ActionType = "CaseCreated",
                    OldValue = "-",
                    NewValue = "Case created",
                    ChangedBy = "Admin",
                    ChangedAt = DateTime.Now
                });

                _context.SaveChanges();

                return Content("✅ CASE CREATED SUCCESSFULLY");
            }
            catch (Exception ex)
            {
                return Content("❌ ERROR: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }

        // ================= CHECK CASE ID =================

        [HttpGet]
        public IActionResult CheckCaseId(string caseId)
        {
            bool exists = _context.TowingCases.Any(x => x.CaseId == caseId);
            return Json(new { exists });
        }

        // ================= LIST =================
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public IActionResult List()
        {
            // ✅ ADD THIS (FOR DROPDOWN)
            ViewBag.StaffList = _context.Staff
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    s.StaffId,
                    s.StaffName
                })
                .ToList();

            var cases = (
                from c in _context.TowingCases
                join s in _context.Staff
                    on c.AssignedStaffId equals s.StaffId into staffJoin
                from s in staffJoin.DefaultIfEmpty()

                join h in _context.CaseHistory
                    on c.CaseId equals h.CaseId into historyJoin

                let lastHistory = historyJoin
                    .OrderByDescending(x => x.ChangedAt)
                    .FirstOrDefault()

                orderby c.CreatedDate descending

                select new
                {
                    c.CaseId,
                    c.CustomerName,
                    c.CustomerContactNumber,
                    c.VehicleBrand,
                    c.RegistrationNo,
                    c.IncidentReason,
                    c.IncidentPlace,
                    c.DropLocation,
                    c.TowingType,
                    c.Status,
                    c.CreatedDate,

                    AssignedStaffName = s != null ? s.StaffName : "Not Assigned",

                    LastAction = lastHistory != null ? lastHistory.ActionType : "CaseCreated",
                    LastUpdatedAt = lastHistory != null ? lastHistory.ChangedAt : c.CreatedDate
                }
            ).ToList();

            return View(cases);
        }

        [Authorize(Roles = "Admin,Staff,ReadOnly")]
        public IActionResult ViewOnly(string caseId)
        {
            var model = _context.TowingCases
                .FirstOrDefault(x => x.CaseId == caseId);

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(string caseId)
        {
            if (string.IsNullOrEmpty(caseId))
                return RedirectToAction("List");

            var model = _context.TowingCases
                .FirstOrDefault(x => x.CaseId == caseId);

            if (model == null)
                return NotFound();

            ViewBag.StaffList = _context.Staff
                .Where(s => s.IsActive)
                .Select(s => new { s.StaffId, s.StaffName })
                .ToList();

            return View(model);
        }


        [HttpPost]
        public IActionResult Edit(TowingCase form)
        {
            try
            {
                var existing = _context.TowingCases
                    .FirstOrDefault(x => x.CaseId == form.CaseId);

                if (existing == null)
                    return Content("❌ Case not found");

                if (form.AssignedStaffId == 0)
                    return Content("❌ Please assign staff");

                string changedBy = "Admin";
                DateTime changedAt = DateTime.Now;

                // 🔁 HELPER METHOD TO LOG FIELD CHANGES
                void LogChange(string fieldName, string oldValue, string newValue)
                {
                    if ((oldValue ?? "") != (newValue ?? ""))
                    {
                        _context.CaseHistory.Add(new CaseHistory
                        {
                            CaseId = existing.CaseId,
                            ActionType = fieldName,
                            OldValue = oldValue,
                            NewValue = newValue,
                            ChangedBy = changedBy,
                            ChangedAt = changedAt
                        });
                    }
                }

                // 🔍 TRACK ALL FIELD CHANGES
                LogChange("CustomerName", existing.CustomerName, form.CustomerName);
                LogChange("VehicleBrand", existing.VehicleBrand, form.VehicleBrand);
                LogChange("Model", existing.Model, form.Model);
                LogChange("RegistrationNo", existing.RegistrationNo, form.RegistrationNo);
                LogChange("ChassisNo", existing.ChassisNo, form.ChassisNo);
                LogChange("CustomerContactNumber", existing.CustomerContactNumber, form.CustomerContactNumber);
                LogChange("IncidentReason", existing.IncidentReason, form.IncidentReason);
                LogChange("IncidentPlace", existing.IncidentPlace, form.IncidentPlace);
                LogChange("DropLocation", existing.DropLocation, form.DropLocation);
                LogChange("AssignedVendorName", existing.AssignedVendorName, form.AssignedVendorName);
                LogChange("VendorContactNumber", existing.VendorContactNumber, form.VendorContactNumber);
                LogChange("TowingType", existing.TowingType, form.TowingType);

                // 🔍 STAFF CHANGE (LOG NAME, NOT ID)
                if (existing.AssignedStaffId != form.AssignedStaffId)
                {
                    var oldStaff = _context.Staff
                        .Where(s => s.StaffId == existing.AssignedStaffId)
                        .Select(s => s.StaffName)
                        .FirstOrDefault();

                    var newStaff = _context.Staff
                        .Where(s => s.StaffId == form.AssignedStaffId)
                        .Select(s => s.StaffName)
                        .FirstOrDefault();

                    LogChange("AssignedStaff", oldStaff, newStaff);
                }

                // 🔍 STATUS CHANGE
                LogChange("Status", existing.Status, form.Status);

                // ✅ UPDATE MAIN ENTITY
                existing.CustomerName = form.CustomerName;
                existing.VehicleBrand = form.VehicleBrand;
                existing.Model = form.Model;
                existing.RegistrationNo = form.RegistrationNo;
                existing.ChassisNo = form.ChassisNo;
                existing.CustomerContactNumber = form.CustomerContactNumber;
                existing.IncidentReason = form.IncidentReason;
                existing.IncidentPlace = form.IncidentPlace;
                existing.DropLocation = form.DropLocation;
                existing.AssignedVendorName = form.AssignedVendorName;
                existing.VendorContactNumber = form.VendorContactNumber;
                existing.TowingType = form.TowingType;
                existing.AssignedStaffId = form.AssignedStaffId;
                existing.Status = form.Status;

                _context.SaveChanges();

                return Content("✅ CASE UPDATED SUCCESSFULLY");
            }
            catch (Exception ex)
            {
                return Content("❌ ERROR: " + ex.Message);
            }
        }


        [HttpPost]
        public IActionResult UpdateStatus([FromBody] StatusUpdateDto dto)
        {
            var tc = _context.TowingCases.FirstOrDefault(x => x.CaseId == dto.CaseId);
            if (tc == null) return Content("❌ Case not found");



            tc.Status = dto.Status;
            _context.SaveChanges();

            return Content("✅ Status updated");
        }


        [HttpGet]
        public IActionResult History(string caseId)
        {
            var history = _context.CaseHistory
                .Where(h => h.CaseId == caseId)
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new
                {
                    h.ActionType,
                    h.OldValue,
                    h.NewValue,
                    h.ChangedBy,
                    ChangedAt = h.ChangedAt.ToString("dd-MMM-yyyy hh:mm tt")
                })
                .ToList();

            return Json(history);
        }

        [HttpPost]
        public IActionResult Delete(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId))
                return Content("❌ Case ID missing");

            var tc = _context.TowingCases.FirstOrDefault(x => x.CaseId == caseId);
            if (tc == null)
                return Content("❌ Case not found");

            _context.CaseHistory.Add(new CaseHistory
            {
                CaseId = tc.CaseId,
                ActionType = "CaseDeleted",
                OldValue = tc.Status,
                NewValue = "Deleted",
                ChangedBy = "Admin",
                ChangedAt = DateTime.Now
            });

            _context.TowingCases.Remove(tc);
            _context.SaveChanges();

            return Content("✅ CASE DELETED SUCCESSFULLY");
        }


        [HttpGet]
        public IActionResult FullcaseHistory(string caseId = null)
        {
            var history = _context.CaseHistory
                .Where(h => caseId == null || h.CaseId == caseId)
                .OrderByDescending(h => h.ChangedAt)
                .ToList();

            return View(history);
        }


        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UploadCaseImagesFromList(
         string caseId,
         string sectionType,
         List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                return BadRequest("No images");

            foreach (var file in images)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var img = new TowingCaseImage
                {
                    CaseId = caseId,
                    SectionType = sectionType,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    ImageData = ms.ToArray()
                };

                _context.TowingCaseImages.Add(img);

                // ✅ HISTORY LOG
                _context.CaseHistory.Add(new CaseHistory
                {
                    CaseId = caseId,
                    ActionType = "ImageUploaded",
                    OldValue = "-",
                    NewValue = $"{sectionType} image uploaded ({file.FileName})",
                    ChangedBy = User.Identity?.Name ?? "Staff",
                    ChangedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult GetCaseImages(string caseId)
        {
            var images = _context.TowingCaseImages
                .Where(x => x.CaseId == caseId)
                .OrderBy(x => x.UploadedAt)
                .Select(x => new
                {
                    ImageId = x.ImageId,       // ✅ PascalCase
                    SectionType = x.SectionType
                })
                .ToList();

            return Json(images);
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Image(int id)
        {
            var img = _context.TowingCaseImages
                .AsNoTracking()
                .FirstOrDefault(x => x.ImageId == id);

            if (img == null || img.ImageData == null || img.ImageData.Length == 0)
                return NotFound();

            return File(
                img.ImageData,
                img.ContentType ?? "image/jpeg",
                enableRangeProcessing: true   // ✅ VERY IMPORTANT
            );
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult DownloadImage(int id)
        {
            var img = _context.TowingCaseImages
                .AsNoTracking()
                .FirstOrDefault(x => x.ImageId == id);

            if (img == null || img.ImageData == null)
                return NotFound();

            return File(
                img.ImageData,
                img.ContentType ?? "application/octet-stream",
                img.FileName ?? $"Image_{id}.jpg"   // forces download
            );
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [IgnoreAntiforgeryToken]
        public IActionResult DeleteImage([FromBody] DeleteImageDto dto)
        {
            if (dto == null || dto.ImageId <= 0)
                return BadRequest("Invalid image id");

            var img = _context.TowingCaseImages
                .AsNoTracking()
                .FirstOrDefault(x => x.ImageId == dto.ImageId);

            if (img == null)
                return Ok(); // already deleted

            // ✅ HISTORY LOG
            _context.CaseHistory.Add(new CaseHistory
            {
                CaseId = img.CaseId,
                ActionType = "ImageDeleted",
                OldValue = $"{img.SectionType} image ({img.FileName})",
                NewValue = "Deleted",
                ChangedBy = User.Identity?.Name ?? "Staff",
                ChangedAt = DateTime.Now
            });

            _context.TowingCaseImages.Remove(
                new TowingCaseImage { ImageId = img.ImageId });

            _context.SaveChanges();

            return Ok();
        }



    }
}
