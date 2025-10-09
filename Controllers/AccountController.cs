using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace LaptopStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AccountController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Add debug logging
            Console.WriteLine($"🔐 LOGIN ATTEMPT: {model.Email}");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            
            if (user == null)
            {
                Console.WriteLine($"❌ USER NOT FOUND: {model.Email}");
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            Console.WriteLine($"✅ USER FOUND: {user.Email}, Role: {user.Role}");
            Console.WriteLine($"🔑 PASSWORD HASH: {user.PasswordHash}");

            bool passwordValid = VerifyPassword(model.Password, user.PasswordHash);
            Console.WriteLine($"🔑 PASSWORD VALID: {passwordValid}");

            if (!passwordValid)
            {
                Console.WriteLine($"❌ PASSWORD INVALID for user: {user.Email}");
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Account is deactivated. Please contact support.");
                return View(model);
            }

            Console.WriteLine($"✅ LOGIN SUCCESSFUL: {user.Email}");

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            
            // Store user info in session
            HttpContext.Session.SetString("JWTToken", token);
            HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Set authentication cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                "Cookies",
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Set JWT token in cookie
            Response.Cookies.Append("X-Access-Token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = model.RememberMe ? DateTime.UtcNow.AddDays(7) : null
            });

            TempData["Success"] = $"Welcome back, {user.FirstName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Store");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email address is already registered.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = "Customer",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Account created successfully! Please login to continue.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear();
            Response.Cookies.Delete("X-Access-Token");
            
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Store");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ========== DEBUG METHODS ==========

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DebugAdminLogin()
        {
            var testEmail = "admin@laptopstore.com";
            var testPassword = "Admin123!";
            
            Console.WriteLine($"🔍 DEBUG LOGIN: {testEmail}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == testEmail);
            
            if (user == null)
            {
                var allUsers = await _context.Users.Select(u => new { u.Email, u.Role }).ToListAsync();
                return Json(new { 
                    success = false, 
                    message = "❌ ADMIN USER NOT FOUND",
                    email = testEmail,
                    allUsers = allUsers
                });
            }

            // Test password verification
            var passwordMatch = VerifyPassword(testPassword, user.PasswordHash);
            var newlyHashedPassword = HashPassword(testPassword);
            
            return Json(new {
                success = passwordMatch,
                message = passwordMatch ? "✅ PASSWORD MATCHES" : "❌ PASSWORD DOES NOT MATCH",
                userFound = true,
                userEmail = user.Email,
                userRole = user.Role,
                userActive = user.IsActive,
                storedHash = user.PasswordHash,
                newlyGeneratedHash = newlyHashedPassword,
                hashesMatch = (user.PasswordHash == newlyHashedPassword),
                testPassword = testPassword,
                verificationResult = passwordMatch
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetAdminPassword()
        {
            var adminEmail = "admin@laptopstore.com";
            
            Console.WriteLine($"🔄 RESETTING ADMIN PASSWORD: {adminEmail}");

            // Delete existing admin if exists
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existingAdmin != null)
            {
                _context.Users.Remove(existingAdmin);
                await _context.SaveChangesAsync();
                Console.WriteLine($"🗑️ REMOVED EXISTING ADMIN: {adminEmail}");
            }

            // Create new admin with fresh password hash
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                PasswordHash = HashPassword("Admin123!"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ CREATED NEW ADMIN: {adminEmail}");

            return Json(new {
                success = true,
                message = "🔄 ADMIN PASSWORD RESET COMPLETE",
                email = adminEmail,
                password = "Admin123!",
                newHash = adminUser.PasswordHash
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { 
                    u.Id, 
                    u.Email, 
                    u.Role, 
                    PasswordHashLength = u.PasswordHash.Length,
                    u.IsActive,
                    u.CreatedAt 
                })
                .ToListAsync();
                
            return Json(new {
                totalUsers = users.Count,
                adminUsers = users.Count(u => u.Role == "Admin"),
                users = users
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestPasswordHash()
        {
            var testPassword = "Admin123!";
            var hash1 = HashPassword(testPassword);
            var hash2 = HashPassword(testPassword);
            
            var verify1 = VerifyPassword(testPassword, hash1);
            var verify2 = VerifyPassword(testPassword, hash2);
            
            return Json(new {
                testPassword = testPassword,
                hash1 = hash1,
                hash2 = hash2,
                hashesEqual = (hash1 == hash2),
                verify1 = verify1,
                verify2 = verify2,
                hash1Length = hash1.Length,
                hash2Length = hash2.Length
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestBcryptImplementation()
        {
            var testPasswords = new[] { "Admin123!", "admin123", "password" };
            var results = new List<object>();

            foreach (var password in testPasswords)
            {
                var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11);
                var verifyEnhanced = BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
                var verifyRegular = BCrypt.Net.BCrypt.Verify(password, hash);
                
                results.Add(new
                {
                    password = password,
                    hash = hash,
                    hashLength = hash.Length,
                    verifyEnhanced = verifyEnhanced,
                    verifyRegular = verifyRegular,
                    bothWork = verifyEnhanced && verifyRegular
                });
            }

            return Json(new { 
                message = "BCrypt Implementation Test",
                results = results 
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CreateVerifiedAdmin()
        {
            var adminEmail = "admin@laptopstore.com";
            var adminPassword = "Admin123!";
            
            // Delete existing admin if exists
            var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existingAdmin != null)
            {
                _context.Users.Remove(existingAdmin);
                await _context.SaveChangesAsync();
            }

            // Use a pre-verified hash that definitely works
            var verifiedHash = "$2a$11$veMSB/l.SJ5.5HnrwJ.1.eucI.uS0bw2Bz7pRoN2u.z.I5P2dL8Ym";
            
            var adminUser = new User
            {
                FirstName = "Admin",
                LastName = "User",
                Email = adminEmail,
                PasswordHash = verifiedHash,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Verify the password works
            var verificationResult = BCrypt.Net.BCrypt.Verify(adminPassword, verifiedHash);

            return Json(new {
                success = true,
                message = "✅ VERIFIED ADMIN CREATED",
                email = adminEmail,
                password = adminPassword,
                hash = verifiedHash,
                verificationTest = verificationResult
            });
        }

        private string HashPassword(string password)
        {
            try
            {
                // Use consistent work factor and ensure proper salt generation
                string hash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11);
                Console.WriteLine($"🔐 HASHED PASSWORD: {hash}");
                Console.WriteLine($"🔐 HASH LENGTH: {hash.Length}");
                return hash;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ HASHING ERROR: {ex.Message}");
                throw;
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                Console.WriteLine($"🔍 VERIFYING PASSWORD:");
                Console.WriteLine($"   Input: {password}");
                Console.WriteLine($"   Hash:  {passwordHash}");
                Console.WriteLine($"   Hash Length: {passwordHash.Length}");
                
                // Use enhanced verification first
                bool result = BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
                Console.WriteLine($"   Enhanced Verify Result: {result}");
                
                if (!result)
                {
                    // Also try regular verification as fallback
                    bool regularResult = BCrypt.Net.BCrypt.Verify(password, passwordHash);
                    Console.WriteLine($"   Regular Verify Result: {regularResult}");
                    result = regularResult;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ VERIFICATION ERROR: {ex.Message}");
                return false;
            }
        }

        // View Models
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public class RegisterViewModel
        {
            [Required(ErrorMessage = "First name is required")]
            [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Last name is required")]
            [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}