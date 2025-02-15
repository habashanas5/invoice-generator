using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Invoice_Generator.Pages.Admin.Roles
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public List<IdentityRole> Roles { get; set; } = new();
        [BindProperty]
        public AddRole NewRole { get; set; } = new();

        public async Task OnGetAsync()
        {
            Roles = await _roleManager.Roles.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await _roleManager.Roles.ToListAsync();
                return Page();
            }

            if (await _roleManager.RoleExistsAsync(NewRole.Name))
            {
                ModelState.AddModelError("NewRole.Name", "Role already exists!");
                Roles = await _roleManager.Roles.ToListAsync();
                Log.Warning("Admin attempted to add a role '{RoleName}', but it already exists.", NewRole.Name);
                return Page();
            }

            await _roleManager.CreateAsync(new IdentityRole(NewRole.Name.Trim()));
            Log.Information("Admin created a new role '{RoleName}'.", NewRole.Name);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            await _roleManager.DeleteAsync(role);
            Log.Information("Admin deleted the role '{RoleName}' with ID '{RoleId}'.", role.Name, id);
            return RedirectToPage();
        }
    }
}
