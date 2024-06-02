using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;

namespace Repository;

public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
{
    public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
    {
    }

    public async Task<Employee?> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges) =>
        await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges).SingleOrDefaultAsync();

    //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges) =>
    //    await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
    //    .OrderBy(e => e.Name)
    //    .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
    //    .Take(employeeParameters.PageSize)
    //    .ToListAsync();

    // For small data this is good solution, but for millions of rows bottom one is much faster, because we do not load all data from sql to our api

    //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    //{
    //    var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
    //        .OrderBy(e => e.Name)
    //        .ToListAsync();

    //    return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize);
    //}

    // Before extracting filter logic separately as an extension method and adding search logic

    //public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    //{
    //    var employees = await FindByCondition(e => e.CompanyId.Equals(companyId) && 
    //    (e.Age >= employeeParameters.MinAge && e.Age <= employeeParameters.MaxAge), trackChanges)
    //        .OrderBy(e => e.Name)
    //        .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
    //        .Take(employeeParameters.PageSize)
    //        .ToListAsync();

    //    var count = await FindByCondition(e => e.CompanyId.Equals(companyId) &&
    //    (e.Age >= employeeParameters.MinAge && e.Age <= employeeParameters.MaxAge), trackChanges).CountAsync();

    //    return new PagedList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize);
    //}

    public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
    {
        var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
            .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
            .Search(employeeParameters.SearchTerm)
            .OrderBy(e => e.Name)
            .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
            .Take(employeeParameters.PageSize)
            .ToListAsync();

        var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
            .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
            .Search(employeeParameters.SearchTerm)
            .CountAsync();

        return new PagedList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize);
    }


    public void CreateEmployeeForCompany(Guid companyId, Employee employee)
    {
        employee.CompanyId = companyId;
        Create(employee);
    }

    public void DeleteEmployee(Employee employee) => Delete(employee);
}