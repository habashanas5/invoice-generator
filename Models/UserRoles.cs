namespace Invoice_Generator.Models
{
    public class UserRoles
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleInfo> Roles { get; set; }
    }
}
