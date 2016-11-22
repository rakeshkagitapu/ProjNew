using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SPFS.Helpers
{
    public static class SPFSConstants
    {
        public const int UserPageSize =8;
        public const int SupplierPageSize = 20;
        public const int SitePageSize = 20;
        public const int Excelrecords = 30;
    }

    public static class SPFSHelpers
    {
        public static int GetPageNumberOnRecordCount(int pageNo, int? recordCount, int pageSize)
        {
            int PageNumber = 1;

            if (((recordCount - 1) / pageSize) + 1 >= pageNo)
                PageNumber = pageNo;

            return PageNumber;
        }
    }
}