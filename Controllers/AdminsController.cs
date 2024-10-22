using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School.DataContext;
using School.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace School.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminsController : Controller
    {
        private readonly DataContextDb _context;

        public AdminsController(DataContextDb context)
        {
            _context = context;
        }

        // GET: Admins
        
        public async Task<IActionResult> Index()
        {
            return View(await _context.Admins.ToListAsync());
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.Admin_Id == id);

            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Admin_Id,Admin_Name,Admin_SurName,Admin_Password,Admin_Password2")] Admin admin)
        {
            if (admin.Admin_Password != admin.Admin_Password2)
            {
                ModelState.AddModelError("Admin_Password2", "Şifreler aynı değil!");
                return View(admin);
            }

            var adminName = await _context.Admins
                .FirstOrDefaultAsync(a => a.Admin_Name == admin.Admin_Name);

            var adminEmail = await _context.Admins
                .FirstOrDefaultAsync(a => a.Admin_SurName == admin.Admin_SurName);

            if (adminName != null)
            {
                ModelState.AddModelError("Admin_Name", "Böyle bir kullanıcı adı zaten var!");
            }

            if (adminEmail != null)
            {
                ModelState.AddModelError("Admin_SurName", "Böyle bir e-posta adresi zaten var!");
            }

            if (adminName == null && adminEmail == null)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(admin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(admin);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Admin_Id,Admin_Name,Admin_SurName,Admin_Password,Admin_Password2")] Admin admin)
        {
            var adminControl = await _context.Admins.FindAsync(id);

            if (adminControl == null || id != admin.Admin_Id)
            {
                return NotFound();
            }

            bool isNameChanged = adminControl.Admin_Name != admin.Admin_Name;
            bool isSurNameChanged = adminControl.Admin_SurName != admin.Admin_SurName;

            if (isNameChanged)
            {
                var adminName = await _context.Admins.FirstOrDefaultAsync(a => a.Admin_Name == admin.Admin_Name);
                if (adminName != null)
                {
                    ModelState.AddModelError("Admin_Name", "Böyle bir kullanıcı adı zaten var!");
                    return View(admin);
                }
            }

            if (isSurNameChanged)
            {
                var adminEmail = await _context.Admins.FirstOrDefaultAsync(a => a.Admin_SurName == admin.Admin_SurName);
                if (adminEmail != null)
                {
                    ModelState.AddModelError("Admin_SurName", "Böyle bir e-posta adresi zaten var!");
                    return View(admin);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (admin.Admin_Password != admin.Admin_Password2)
                    {
                        ModelState.AddModelError("Admin_Password2", "Şifreler eşleşmiyor.");
                        return View(admin);
                    }

                    adminControl.Admin_Password = admin.Admin_Password;

                    if (isNameChanged)
                    {
                        adminControl.Admin_Name = admin.Admin_Name;
                    }

                    if (isSurNameChanged)
                    {
                        adminControl.Admin_SurName = admin.Admin_SurName;
                    }

                    _context.Update(adminControl);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Admin_Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(admin);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.Admin_Id == id);

            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Admin_Id == id);
        }

        [AllowAnonymous]
        public IActionResult AdminLogin()
        {
            var login = new AdminLogin();

            // Eğer çerezlerde kullanıcı adı ve şifre varsa bunları doldur
            if (Request.Cookies.ContainsKey("AdminLogin_Name"))
            {
                login.AdminLogin_Name = Request.Cookies["AdminLogin_Name"];
            }

            if (Request.Cookies.ContainsKey("AdminLogin_Password"))
            {
                login.AdminLogin_Password = Request.Cookies["AdminLogin_Password"];
            }
            return View(login);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminLogin adminLogin,bool? RememberMe)
        {
            if (ModelState.IsValid)
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Admin_Name == adminLogin.AdminLogin_Name && a.Admin_Password == adminLogin.AdminLogin_Password);

                if (admin != null)
                {
                    // Beni hatırla seçeneği işaretli ise çerez oluşturuyoruz.
                    if (RememberMe == true) // null değilse true kontrolü
                    {
                        CookieOptions options = new CookieOptions();
                        options.Expires = DateTime.Now.AddDays(7); // 7 gün boyunca hatırla

                        Response.Cookies.Append("AdminLogin_Name", adminLogin.AdminLogin_Name, options);
                        Response.Cookies.Append("AdminLogin_Password", adminLogin.AdminLogin_Password, options);
                    }
                    else
                    {
                        // Beni hatırla işaretli değilse çerezleri siliyoruz.
                        Response.Cookies.Delete("AdminLogin_Name");
                        Response.Cookies.Delete("AdminLogin_Password");
                    }
                    //Kullanıcı bilgilerini oluştur
                    var claims = new List<Claim>
                   {
                        new Claim(ClaimTypes.Name, admin.Admin_Name),
                        new Claim(ClaimTypes.NameIdentifier, admin.Admin_Id.ToString()),
                        new Claim(ClaimTypes.Role,"Admin")
                   };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // Cookie ömrünü buradan ayarlayabilirsiniz
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddMinutes(10)
                    };
                    // Kimlik doğrulama işlemini gerçekleştir
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    return RedirectToAction("Index", "Admins");
                }
                else
                {
                    ModelState.AddModelError("AdminLogin_Password", "Kullanıcı Adı veya Şifre hatalı");
                }
            }
            return View(adminLogin);
        }
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Logout()
        {
            if (User.IsInRole("Admin"))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);  
            }
            return RedirectToAction("AdminLogin", "Admins");
        }
    }
}
