using System.Drawing;

namespace StaffInfo
{
    public  class Staff
    {
        internal string DistinguishId { get; set; }
        public string Name { get; set; }
        public string StaffId { get; set; }
        public string DomainName { get; set; }
        public string WindowsAccount { get; set; }
        public string SubName { get; set; }
        public string KnownAsGivenName { get; set; }
        public string LineManagerId { get; set; }
        public Staff Manager { get; set; }
        public string ManagerId { get; set; }
        public Staff AuthManager { get; set; }
        public string AuthManagerId { get; set; }
        public string Mail { get; set; }
        public string InternalMail { get; set; }
        public Image Photo { get; set; }
    }
}
