using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ATSCADA_Library.Helpers.Sorting
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> entities, string searchTerm, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(propertyName))
                return entities;

            var parameter = Expression.Parameter(typeof(T), "p");
            var property = Expression.Property(parameter, propertyName);

            Expression searchExpression;

            if (property.Type == typeof(string))
            {
                // Xử lý tìm kiếm cho chuỗi
                var lowerCaseSearchTerm = searchTerm.Trim().ToLower();
                var toLowerCall = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
                searchExpression = Expression.Call(toLowerCall, typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                                                   Expression.Constant(lowerCaseSearchTerm));
            }
            else if (property.Type == typeof(int) && int.TryParse(searchTerm, out var intValue))
            {
                // Xử lý tìm kiếm cho kiểu int
                var constant = Expression.Constant(intValue);
                searchExpression = Expression.Equal(property, constant);
            }
            else
            {
                // Không hỗ trợ tìm kiếm với kiểu dữ liệu này
                throw new NotSupportedException($"The property type '{property.Type}' is not supported for searching.");
            }

            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            return entities.Where(lambda);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> entities, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return entities;

            var orderParams = orderByQueryString.Trim().Split(',');
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var orderQueryBuilder = new StringBuilder();

            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                var propertyFromQueryName = param.Split(" ")[0];
                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

                if (objectProperty == null)
                    continue;

                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                orderQueryBuilder.Append($"{objectProperty.Name} {direction}, ");
            }

            var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            if (string.IsNullOrWhiteSpace(orderQuery))
                return entities;

            return entities.OrderBy(orderQuery);
        }
    }
}
