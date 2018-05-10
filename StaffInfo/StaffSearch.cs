using System;
using System.DirectoryServices;
using System.Drawing;
using System.IO;

namespace StaffInfo
{
    public  class StaffSearch
    {
        private const string DEFAULT_LDAP_SERVER_STRING = "LDAP://glue.systems.uk.hsbc:3269/OU=HSBCPeople,DC=InfoDir,DC=Prod,DC=HSBC";
        private readonly string _ldapServer;
        #region Columns
        private const string COLUMN_CN = "cn";
        private const string COLUMN_DISPLAY_NAME = "displayname";
        private const string COLUMN_EMPLOYEE_ID = "employeeid";
        private const string COLUMN_DOMAIN_NAME = "hsbc-ad-domainname";
        private const string COLUMN_SAM_ACCOUNT_NAME = "hsbc-ad-samaccountname";
        private const string COLUMN_KNOWN_AS_LAST_NAME = "hsbc-ad-knownaslastname";
        private const string COLUMN_KNOWN_AS_GIVEN_NAME = "hsbc-ad-knownasgivenname";
        private const string COLUMN_LINE_MANAGER_ID = "hsbc-ad-linemanagerid";
        private const string COLUMN_MANAGER_NAME = "hsbc-ad-managername";
        private const string COLUMN_MANAGER_EMPLID = "hsbc-ad-manageremplid";
        private const string COLUMN_AUTH_MANAGER_NAME = "hsbc-ad-authmanagername";
        private const string COLUMN_AUTH_MANAGER_EMPLID = "hsbc-ad-authmanageremplid";
        private const string COLUMN_IDENTITY_GUID = "hsbc-ad-identityguid";
        private const string COLUMN_MAIL_ADDRESS = "mailaddress";
        private const string COLUMN_MAIL = "mail";
        private const string COLUMN_JPEG_PHOTO = "jpegphoto";
        #endregion
        public string CurrStaffId
        {
            get
            {
                return Environment.UserName;
            }
        }

        public StaffSearch(string serverString = null)
        {
            if (string.IsNullOrEmpty(serverString))
            {
                _ldapServer = DEFAULT_LDAP_SERVER_STRING;
                return;
            }
            _ldapServer = serverString;
        }
        public Staff GetCurStaffInfoByStaffId()
        {
            string query = string.Format("(&(objectClass=top)(hsbc-ad-SAMAccountName={0}))", CurrStaffId);
            var staff = SearchFull(query);
            return staff;
        }
        public Staff GetStaffInfoByName(string name)
        {
            string query = string.Format("(&(objectClass=top)(displayName={0}))", name);
            var staff = SearchFull(query);
            return staff;
        }
        public Staff GetStaffInfoByInternalMail(string mail)
        {
            string query = string.Format("(&(objectClass=top)(mailaddress={0}))", mail);
            var staff = SearchFull(query);
            return staff;
        }
        public Staff GetManagerBySubordinateStaffId(string staffId)
        {
            string query = string.Format("(&(objectClass=top)(employeeid={0}))", staffId);
            var staff = SearchFull(query);

            return staff.Manager;
        }
        public string GetManagerIdBySubordinateName(string name)
        {
            string query = string.Format("(&(objectClass=top)(displayName={0}))", name);

            var staff = SearchFull(query);
            return staff.Manager.StaffId;
        }

        public string GetManagerInternalMailBySubordinateName(string name)
        {
            string query = string.Format("(&(objectClass=top)(displayName={0}))", name);

            var staff = SearchFull(query);
            return staff.Manager.InternalMail;
        }

        private Staff SearchFull(string query, Staff referenceStaff = null)
        {
            var staff = new Staff();
            using (DirectoryEntry entry = new DirectoryEntry(_ldapServer))
            {
                using (DirectorySearcher ds = new DirectorySearcher(entry, query, null, SearchScope.Subtree))
                {
                    ds.PropertiesToLoad.Add(COLUMN_CN);
                    ds.PropertiesToLoad.Add(COLUMN_DISPLAY_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_EMPLOYEE_ID);
                    ds.PropertiesToLoad.Add(COLUMN_DOMAIN_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_SAM_ACCOUNT_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_KNOWN_AS_LAST_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_KNOWN_AS_GIVEN_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_LINE_MANAGER_ID);
                    ds.PropertiesToLoad.Add(COLUMN_MANAGER_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_MANAGER_EMPLID);
                    ds.PropertiesToLoad.Add(COLUMN_AUTH_MANAGER_NAME);
                    ds.PropertiesToLoad.Add(COLUMN_AUTH_MANAGER_EMPLID);
                    ds.PropertiesToLoad.Add(COLUMN_IDENTITY_GUID);
                    ds.PropertiesToLoad.Add(COLUMN_MAIL_ADDRESS);
                    ds.PropertiesToLoad.Add(COLUMN_MAIL);
                    ds.PropertiesToLoad.Add(COLUMN_JPEG_PHOTO);

                    SearchResult staffInfo = ds.FindOne();  
                    if (staffInfo != null)
                    {
                        staff = new Staff
                        {
                            Name = GetPropertyValue(staffInfo.Properties[COLUMN_DISPLAY_NAME]),
                            StaffId = GetPropertyValue(staffInfo.Properties[COLUMN_EMPLOYEE_ID]),
                            DomainName = GetPropertyValue(staffInfo.Properties[COLUMN_DOMAIN_NAME]),
                            WindowsAccount = GetPropertyValue(staffInfo.Properties[COLUMN_SAM_ACCOUNT_NAME]),
                            SubName = FirstCharToUpper(GetPropertyValue(staffInfo.Properties[COLUMN_KNOWN_AS_LAST_NAME])),
                            KnownAsGivenName = FirstCharToUpper(GetPropertyValue(staffInfo.Properties[COLUMN_KNOWN_AS_GIVEN_NAME])),
                            AuthManagerId = GetPropertyValue(staffInfo.Properties[COLUMN_AUTH_MANAGER_EMPLID]),
                            LineManagerId = GetPropertyValue(staffInfo.Properties[COLUMN_LINE_MANAGER_ID]),
                            ManagerId = GetPropertyValue(staffInfo.Properties[COLUMN_MANAGER_EMPLID]),
                            Mail = GetPropertyValue(staffInfo.Properties[COLUMN_MAIL]),
                            InternalMail = GetPropertyValue(staffInfo.Properties[COLUMN_MAIL_ADDRESS])
                        };

                        if (staffInfo.Properties[COLUMN_JPEG_PHOTO].Count > 0)
                        {
                            MemoryStream ms = null;
                            try
                            {
                                byte[] picbyte = (byte[])staffInfo.Properties[COLUMN_JPEG_PHOTO][0];
                                ms = new MemoryStream(picbyte);
                                staff.Photo = Image.FromStream(ms);
                            }
                            finally
                            {
                                if (ms != null)
                                    ms.Close();
                            }
                        }

                        if (referenceStaff != null && (referenceStaff.ManagerId == staff.StaffId || referenceStaff.AuthManagerId == staff.StaffId))
                        {
                            return staff;
                        }

                        if (!string.IsNullOrEmpty(staff.ManagerId))
                        {
                            staff.Manager = SearchFull(string.Format("(&(objectClass=top)(employeeid={0}))", staff.ManagerId), staff);
                        }

                        if (!string.IsNullOrEmpty(staff.AuthManagerId) && staff.ManagerId != staff.AuthManagerId)
                            staff.AuthManager = SearchFull(string.Format("(&(objectClass=top)(employeeid={0}))", staff.AuthManagerId), staff);

                        if (staff.ManagerId == staff.AuthManagerId)
                        {
                            staff.AuthManager = staff.Manager;
                        }
                    }
                }
            }
            return staff;
        }
        private string FirstCharToUpper(object v)
        {
            if (v != null && !string.IsNullOrEmpty(v.ToString()))
            {
                var str = v.ToString().ToLower();

                if (str.Length > 1)
                    return string.Format("{0}{1}", str.Substring(0, 1).ToUpper(), str.Substring(1, str.Length - 1));

                return str.ToUpper();
            }
            return "";
        }
        private string GetPropertyValue(ResultPropertyValueCollection resultPropertyValueCollection)
        {
            if (resultPropertyValueCollection.Count > 0)
                return resultPropertyValueCollection[0].ToString();

            return string.Empty;
        }
    }
}
