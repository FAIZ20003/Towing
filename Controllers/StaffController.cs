using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proffessional.Data;
using Proffessional.Models;

[Authorize(Roles = "Admin")]
public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create(string StaffName, string PhoneNumber, string Username, string Password)
    {
        if (string.IsNullOrWhiteSpace(StaffName))
        {
            TempData["Error"] = "Staff name is required";
            return RedirectToAction("Create");
        }

        if (_context.Users.Any(u => u.Username == Username))
        {
            TempData["Error"] = "Username already exists";
            return RedirectToAction("Create");
        }

        var staff = new Staff
        {
            StaffName = StaffName.Trim(),
            PhoneNumber = PhoneNumber.Trim(),
            IsActive = true
        };

        _context.Staff.Add(staff);
        _context.SaveChanges();

        var user = new User
        {
            Username = Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
            Role = "Staff",
            IsActive = true,
            StaffId = staff.StaffId
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        // 🔔 LOG STAFF CREATION
        _context.CaseHistory.Add(new CaseHistory
        {
            CaseId = $"STAFF-{staff.StaffId}",
            ActionType = "StaffCreated",
            OldValue = "-",
            NewValue = $"Staff: {staff.StaffName}, Phone: {staff.PhoneNumber}",
            ChangedBy = User.Identity?.Name ?? "Admin",
            ChangedAt = DateTime.Now
        });

        _context.SaveChanges();


        TempData["Success"] = "Staff created successfully with login access";
        return RedirectToAction("Create");
    }


    [HttpGet]
    public IActionResult List()
    {
        var staff = _context.Staff
            .OrderBy(s => s.StaffName)
            .ToList();

        return View(staff);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var staff = _context.Staff.FirstOrDefault(x => x.StaffId == id);
        if (staff == null)
            return NotFound();

        return View(staff);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Edit(Staff form)
    {
        if (!ModelState.IsValid)
            return View(form);

        var existing = _context.Staff.First(x => x.StaffId == form.StaffId);

        existing.StaffName = form.StaffName;
        existing.PhoneNumber = form.PhoneNumber;
        existing.IsActive = form.IsActive;

        _context.SaveChanges();

        TempData["PopupMessage"] = "Staff updated successfully";
        TempData["PopupType"] = "success";

        return RedirectToAction("List", new { id = form.StaffId });
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int StaffId)
    {
        var staff = _context.Staff.FirstOrDefault(s => s.StaffId == StaffId);
        if (staff == null)
        {
            TempData["PopupMessage"] = "Staff not found.";
            TempData["PopupType"] = "error";
            return RedirectToAction("List");
        }

        // 🔒 CHECK IF STAFF HAS CASES
        bool hasCases = _context.TowingCases
            .Any(c => c.AssignedStaffId == StaffId);

        if (hasCases)
        {
            TempData["PopupMessage"] = "This staff has assigned cases and cannot be deleted.";
            TempData["PopupType"] = "warning";
            return RedirectToAction("List");
        }

        // 🔐 Remove linked user if exists
        var user = _context.Users.FirstOrDefault(u => u.StaffId == StaffId);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        _context.Staff.Remove(staff);
        _context.SaveChanges();

        TempData["PopupMessage"] = "Staff deleted successfully.";
        TempData["PopupType"] = "success";
        return RedirectToAction("List");
    }





}
