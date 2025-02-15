using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Invoice_Generator.Pages.Admin.UsersRoles
{
    [Authorize(Roles = "Admin")]
    public class ManageRolesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageRolesModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public string UserId { get; set; }
        [BindProperty]
        public string UserName { get; set; }
        [BindProperty]
        public List<RoleInfo> Roles { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            UserId = user.Id;
            UserName = user.UserName;

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.ToList();

            Roles = allRoles.Select(role => new RoleInfo
            {
                RoleId = role.Id,
                RoleName = role.Name,
                IsSelected = userRoles.Contains(role.Name)
            }).ToList();

            Log.Information($"Roles for user {user.UserName} fetched successfully.");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in Roles)
            {
                if (userRoles.Contains(role.RoleName) && !role.IsSelected)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                    Log.Information($"Role {role.RoleName} removed from user {user.UserName}.");
                }
                else if (!userRoles.Contains(role.RoleName) && role.IsSelected)
                {
                    await _userManager.AddToRoleAsync(user, role.RoleName);
                    Log.Information($"Role {role.RoleName} added to user {user.UserName}.");
                }
            }

            return RedirectToPage("./Index");
        }
    }
}