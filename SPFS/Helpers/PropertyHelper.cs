
using System;
using System.Linq.Expressions;

namespace SPFS.Helpers
{
    /// <summary>
    /// Class PropertyHelper.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        /// To the property string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        public static string ToPropertyString<T, P>(Expression<Func<T, P>> property) where T : class
        {
            return ((MemberExpression)property.Body).Member.Name;
        }
    }
}