namespace VW.PCI.Api.Client.Models.Requests
{
    public class UpdateUserRequest
    {
        public string RecId { get; set; }
        public string UserName { get; set; }
        public string AsClient { get; set; }
        public string SystemID { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PasswordType { get; set; }
        public int LoginQuestionIndex { get; set; }
        public string LoginQuestionAnswer { get; set; }
        public string ActiveStatus { get; set; }
        public string HierarchyIds { get; set; }
        public bool PciAccess { get; set; }
    }
}
