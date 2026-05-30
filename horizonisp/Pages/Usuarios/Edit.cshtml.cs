using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;
using horizonisp.Models;

namespace horizonisp.Pages.Usuarios
{
    public class EditModel(AppDbContext db, PasswordHasher<Usuario> passwordHasher) : PageModel
    {
        [BindProperty]
        public Usuario Usuario { get; set; } = new();

        [BindProperty]
        public string? NovaSenha { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario is null)
            {
                return NotFound();
            }

            Usuario = usuario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(NovaSenha) && NovaSenha.Length < 6)
            {
                ModelState.AddModelError(nameof(NovaSenha), "A senha deve ter pelo menos 6 caracteres.");
            }

            var existente = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == Usuario.Id);
            if (existente is null)
            {
                return NotFound();
            }

            if (await db.Usuarios.AnyAsync(u => u.Email == Usuario.Email && u.Id != Usuario.Id))
            {
                ModelState.AddModelError($"{nameof(Usuario)}.{nameof(Usuario.Email)}", "E-mail já cadastrado.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            existente.Nome = Usuario.Nome;
            existente.Email = Usuario.Email;
            existente.Perfil = Usuario.Perfil;
            existente.Ativo = Usuario.Ativo;

            if (!string.IsNullOrWhiteSpace(NovaSenha))
            {
                existente.SenhaHash = passwordHasher.HashPassword(existente, NovaSenha);
            }

            await db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
