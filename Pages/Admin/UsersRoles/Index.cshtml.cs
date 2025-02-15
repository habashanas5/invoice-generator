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
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserInfo> Users { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            Users = new List<UserInfo>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserInfo
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }
            Log.Information("Successfully fetched user data.");
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                Log.Information($"User with ID {id} not found.");
                return NotFound();
            }

            await _userManager.DeleteAsync(user);
            Log.Information($"User with ID {id} deleted successfully.");
            return RedirectToPage();
        }
    }
}