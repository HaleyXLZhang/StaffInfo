using Microsoft.VisualStudio.TestTools.UnitTesting;
using StaffInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffInfo.Tests
{
    [TestClass()]
    public class StaffSearchTests
    {
        [TestMethod()]
        public void GetManagerIdBySubordinateNameTest()
        {
            StaffSearch staffSearch = new StaffSearch();
          var item=  staffSearch.GetManagerBySubordinateStaffId("43981976");
        }
    }
}