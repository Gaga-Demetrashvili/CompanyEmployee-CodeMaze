using Entities.Models;
using System.Linq.Expressions;

namespace Repository.Extensions;

internal static class RepositoryEmployeeExtensions
{
    public static IQueryable<Employee> FilterEmployees(this IQueryable<Employee> employees,
        uint minAge, uint maxAge) => employees.Where(e => (e.Age >= minAge && e.Age <= maxAge));

    public static IQueryable<Employee> Search(this IQueryable<Employee> employees,
        string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return employees;
        }

        var lowerCaseTerm = searchTerm.ToLower();

        return employees.Where(e => e.Name!.ToLower().Contains(lowerCaseTerm));
    }
}
