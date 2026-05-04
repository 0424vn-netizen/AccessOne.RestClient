namespace VW.PCI.Api.Client.Models.Requests
{
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string AsClient { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string PasswordType { get; set; }
        public int LoginQuestionIndex { get; set; }
        public string LoginQuestionAnswer { get; set; }
        public string ActiveStatus { get; set; }
        public string HierarchyIds { get; set; }
        public string CreatedBy { get; set; }
        public string UserSecRole { get; set; }
        public bool PciAccess { get; set; }
        public string EntityID { get; set; }
        public string EntityTypeID { get; set; }
    }
}
