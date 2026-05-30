using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Usuarios
{
    public class CreateModel(AppDbContext db, PasswordHasher<Usuario> passwordHasher) : PageModel
    {
        [BindProperty]
        public Usuario Usuario { get; set; } = new();

        [BindProperty]
        public string Senha { get; set; } = string.Empty;

        public void OnGet()
        {
            Usuario.Ativo = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Senha) || Senha.Length < 6)
            {
                ModelState.AddModelError(nameof(Senha), "Informe uma senha com pelo menos 6 caracteres.");
            }

            if (await db.Usuarios.AnyAsync(u => u.Email == Usuario.Email))
            {
                ModelState.AddModelError($"{nameof(Usuario)}.{nameof(Usuario.Email)}", "E-mail já cadastrado.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Usuario.SenhaHash = passwordHasher.HashPassword(Usuario, Senha);
            db.Usuarios.Add(Usuario);
            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
