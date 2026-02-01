using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Proffessional.Data;
using BCrypt.Net;
using System.Security.Claims;
using Proffessional.Models;
using Microsoft.AspNetCore.Authorization;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = _context.Users.FirstOrDefault(x =>
            x.Username == username && x.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // 🔐 CREATE CLAIMS
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // ✅ REQUIRED
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

        if (user.StaffId.HasValue)
        {
            claims.Add(new Claim("StaffId", user.StaffId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        // 📝 LOGIN HISTORY LOG
        _context.CaseHistory.Add(new CaseHistory
        {
            CaseId = $"LOGIN-{user.UserId}",
            ActionType = "UserLogin",
            OldValue = "-",
            NewValue = $"Username: {user.Username}, Role: {user.Role}",
            ChangedBy = user.Username,
            ChangedAt = DateTime.Now
        });

        _context.SaveChanges();

        return RedirectToAction("Index", "Dashboard");
    }


    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Unknown";

        _context.CaseHistory.Add(new CaseHistory
        {
            CaseId = "LOGOUT",
            ActionType = "UserLogout",
            OldValue = "-",
            NewValue = $"User logged out: {username}",
            ChangedBy = username,
            ChangedAt = DateTime.Now
        });

        _context.SaveChanges();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction("Login");
    }

}
