using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace Service;

internal sealed class EmployeeService : IEmployeeService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILoggerManager _logger;
    private readonly IMapper _mapper;

    public EmployeeService(IRepositoryManager repositoryManager, ILoggerManager logger, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeFromDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

        var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);

        return employeeDto;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(Guid companyId, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeesFromDb = await _repositoryManager.Employee.GetEmployeesAsync(companyId, trackChanges);
        var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

        return employeesDto;
    } 

    public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

        _repositoryManager.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
        await _repositoryManager.SaveAsync();

        var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

        return employeeToReturn;
    }

    public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges)
    {
        await CheckIfCompanyExists(companyId, trackChanges);

        var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

        _repositoryManager.Employee.DeleteEmployee(employeeForCompany!);
        await _repositoryManager.SaveAsync();
    }

    public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);

        var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

        _mapper.Map(employeeForUpdate, employeeEntity);
        await _repositoryManager.SaveAsync();
    }

    public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
    {
        await CheckIfCompanyExists(companyId, compTrackChanges);

        var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

        var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

        return (employeeToPatch!, employeeEntity!);
    }

    public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
    {
        _mapper.Map(employeeToPatch, employeeEntity);
        await _repositoryManager.SaveAsync();
    }

    private async Task<Employee?> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
    {
        var employeeFromDb = await _repositoryManager.Employee.GetEmployeeAsync(companyId, id, trackChanges);
        if (employeeFromDb == null)
            throw new EmployeeNotFoundException(id);
        return employeeFromDb;
    }

    private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
    {
        var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges);
        if (company == null)
            throw new CompanyNotFoundException(companyId);
    }
}

