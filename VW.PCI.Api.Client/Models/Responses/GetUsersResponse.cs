using System;
using System.Collections.Generic;

namespace VW.PCI.Api.Client.Models.Responses
{
    public class GetUsersResponse
    {
        public List<UserItem> Data { get; set; }
    }

    public class UserItem
    {
        public string RecId { get; set; }
        public string ASClient { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PasswordType { get; set; }
        public DateTime? PasswordDTS { get; set; }
        public string PasswordTmpType { get; set; }
        public DateTime? PasswordTmpDTS { get; set; }
        public int? LoginAttempts { get; set; }
        public DateTime? LastLoginDTS { get; set; }
        public DateTime? PrevLoginDTS { get; set; }
        public string TandCStat { get; set; }
        public DateTime? TandCStatDTS { get; set; }
        public string UserSecRole { get; set; }
        public bool? SuperUser { get; set; }
        public bool? PCISuperUser { get; set; }
        public bool? DDSCSUser { get; set; }
        public string UserAccsType { get; set; }
        public string UserAccsLevel { get; set; }
        public string ActiveStatus { get; set; }
        public bool? IsMerchant { get; set; }
        public DateTime? CreatedDTS { get; set; }
        public DateTime? UpdatedDTS { get; set; }
        public string Email { get; set; }
        public string FtoStatus { get; set; }
        public DateTime? FtoCreateDate { get; set; }
        public int? LoginsM01 { get; set; }
        public int? LoginsM02 { get; set; }
        public int? LoginsM03 { get; set; }
        public int? LoginsM04 { get; set; }
        public int? LoginsM05 { get; set; }
        public int? LoginsM06 { get; set; }
        public int? LoginsM07 { get; set; }
        public int? LoginsM08 { get; set; }
        public int? LoginsM09 { get; set; }
        public int? LoginsM10 { get; set; }
        public int? LoginsM11 { get; set; }
        public int? LoginsM12 { get; set; }
        public int? LoginsYTD { get; set; }
        public int? LoginsPriorYear { get; set; }
        public string ClientIPAddr { get; set; }
        public string HostIPAddr { get; set; }
        public string BrowserType { get; set; }
        public DateTime? ActiveStatusDTS { get; set; }
        public DateTime? LockedOutDTS { get; set; }
        public string HierarchyLevel { get; set; }
        public string HierarchyId { get; set; }
    }
}
