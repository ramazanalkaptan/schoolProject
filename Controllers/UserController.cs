using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School.DataContext;
using School.Models;

namespace School.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly DataContextDb _context;
        public UserController(DataContextDb context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Registers.ToListAsync());
        }
        // Bu metot AJAX isteklerini işler ve JSON döner
        [HttpGet]
        public IActionResult GetUsers(string searchString)
        {
            var users = from u in _context.Registers
                        select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Register_Name.Contains(searchString) || s.Register_Email.Contains(searchString));
            }

            // JSON verisi olarak döndürüyoruz
            return Json(users.ToList());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var admin = await _context.Registers
                .FirstOrDefaultAsync(m => m.Register_Id == id);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Register_Id,Register_Name,Register_Email,Register_Password,Register_Password2")] Register register)
        {
            if (register.Register_Password != register.Register_Password2)
            {
                // Şifreler eşleşmiyor
                ModelState.AddModelError("Register_Password2", "Şifreler aynı değil!");
                return View(register);
            }
            var RegisterName = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Name == register.Register_Name);
            var RegisterEmail = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Email == register.Register_Email);
            if (RegisterName != null)
            {
                ModelState.AddModelError("Register_Name", "Böyle bir kullanıcı adı zaten var!");
            }
            if (RegisterEmail != null)
            {
                ModelState.AddModelError("Register_Email", "Böyle bir e-posta adı zaten var!");
            }
            if (RegisterName == null && RegisterEmail == null)
            {
                if (ModelState.IsValid)
                {
                    // Model geçerli
                    _context.Add(register);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(register);
        }
        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var register = await _context.Registers.FindAsync(id);
            if (register == null)
            {
                return NotFound();
            }
            return View(register);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Register_Id,Register_Name,Register_Email,Register_Password,Register_Password2")] Register register)
        {
            var registerControl = await _context.Registers.FindAsync(id);

            if (registerControl == null || id != register.Register_Id)
            {
                return NotFound();
            }

            bool isNameChanged = registerControl.Register_Name != register.Register_Name;
            bool isSurNameChanged = registerControl.Register_Email != register.Register_Email;

            // Eğer sadece bir tanesi değişmişse kontrol edelim
            if (isNameChanged)
            {
                // Aynı kullanıcı adında başka bir kayıt var mı kontrol et
                var adminName = await _context.Registers.FirstOrDefaultAsync(a => a.Register_Name == register.Register_Name);
                if (adminName != null)
                {
                    ModelState.AddModelError("Register_Name", "Böyle bir kullanıcı adı zaten var!");
                    return View(register);
                }
            }

            if (isSurNameChanged)
            {
                // Aynı e-posta adında başka bir kayıt var mı kontrol et
                var registerEmail = await _context.Registers.FirstOrDefaultAsync(a => a.Register_Email == register.Register_Email);
                if (registerEmail != null)
                {
                    ModelState.AddModelError("Register_Email", "Böyle bir e-posta adı zaten var!");
                    return View(register);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Şifreleri kontrol et
                    if (register.Register_Password != register.Register_Password2)
                    {
                        ModelState.AddModelError("Register_Password2", "Şifreler eşleşmiyor.");
                        return View(register);
                    }

                    // Sadece değişen alanları güncelle
                    registerControl.Register_Password = register.Register_Password;

                    if (isNameChanged)
                    {
                        registerControl.Register_Name = register.Register_Name;
                    }

                    if (isSurNameChanged)
                    {
                        registerControl.Register_Email = register.Register_Email;
                    }

                    _context.Update(registerControl);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegisterExists(register.Register_Id))
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

            return View(register);
        }
        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var register = await _context.Registers
                .FirstOrDefaultAsync(m => m.Register_Id == id);
            if (register == null)
            {
                return NotFound();
            }
            return View(register);
        }
        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var register = await _context.Registers.FindAsync(id);
            if (register != null)
            {
                _context.Registers.Remove(register);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool RegisterExists(int id)
        {
            return _context.Registers.Any(e => e.Register_Id == id);
        }
    }
}
