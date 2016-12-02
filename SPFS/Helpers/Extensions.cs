using SPFS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Razor;

namespace SPFS.Helpers
{
    public static class Extensions
    {

        private static Dictionary<string, string> ExcelColumnMappings
        {
            get
            {
                return new Dictionary<string, string>() {
                    {"ERP_Supplier_ID","ERP_Supplier_ID"},
                    {"DUNS","DUNS"},
                    {"CID","CID"},
                    {"Inbound_parts","Inbound"},
                    {"OTR","OnTime Quantity" },
                    {"OTD","OnTime Quantity Due" },
                    {"PFR","Premium Freight" }
            };
            }
        }

        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();
            int i = 1;
            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties,i);           
                result.Add(item);
                i++;
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties,int rowId) where T : new()
        {
            T item = new T();
            List<ErrorDetails> errors = new List<ErrorDetails>();
            PropertyInfo errorProperty = null;
            foreach (var property in properties)
            {
                if (property.Name == "ErrorInformation") 
                {
                    errorProperty = property;
                }
                else if (property.Name == "ExcelDiferentiatorID")
                {
                    property.SetValue(item, rowId,null);
                }
                else
                {
                    var excelColumnName = ExcelColumnMappings.ContainsKey(property.Name) ? ExcelColumnMappings[property.Name] : property.Name;
                    if (row.Table.Columns.Contains(excelColumnName))
                    {
                        try
                        {
                            property.SetValue(item, Convert.ChangeType(row[excelColumnName], property.PropertyType), null);
                        }
                        catch (Exception ex)
                        {
                            string msg = string.Empty;
                            msg = string.Format("Failed to convert value {0} into Data Type {1}", new string[] {Convert.ToString(row[excelColumnName]),
                                Convert.ToString( property.PropertyType )});
                            errors.Add(new ErrorDetails { Key = property.Name, ErrorMessage = msg });
                        }
                    }
                }

            }
            if (errors != null && errors.Count > 0)
            {
                errorProperty.SetValue(item, Convert.ChangeType(errors, errorProperty.PropertyType), null);
            }
            return item;
        }


        public static string Left(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(0, length) : value;
        }
        public static string SplitCamelCase(this string inputString)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            if (!string.IsNullOrEmpty(inputString))
            {
                return myTI.ToTitleCase(inputString.ToLower());
            }
            else
                return inputString;
        }


    }
}