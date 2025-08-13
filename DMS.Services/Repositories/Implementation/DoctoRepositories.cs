using AutoMapper;
using DMS.Common;
using DMS.Models.CommonModel;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponseModel;
using DMS.Models.SpDbContext;
using DMS.Service.Repository.Interface;
using DMS.Services.RepositoryFactory;
using DMS.Services.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace DMS.Service.Repository.Implementation;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorRepository> _logger;
    private readonly DoctorManagementSpContext _spContext;
    private readonly IUnitOfWork _unitOfWork;


    public DoctorRepository(DoctorsDbContext context, IMapper mapper, ILogger<DoctorRepository> logger , DoctorManagementSpContext spContext , IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    
    
    //Getall With Dynamic
    public async Task<Page> List(Dictionary<string, object> parameters)
    {
        try
        {   
            var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
            string sqlQuery = "sp_GetDoctorListXML {0}";
            object[] param = { xmlParam };
            var result = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }
    
    //GetBYSID using UnitOfWork
    public async Task<DoctorResponseModel?> GetByDoctorSID(string doctorSid)
    {
        var doctor = await _unitOfWork.GetRepository<Doctor>()
            .SingleOrDefaultAsync(x =>
                x.DoctorSid == doctorSid && x.Status == (int)DoctorStatus.Active);

        if (doctor == null)
            return null; 

        return new DoctorResponseModel
        {
            DoctorSid = doctor.DoctorSid,
            FullName = doctor.FullName,
            Email = doctor.Email,
            Phone = doctor.Phone,
            Gender = doctor.Gender,
            YearsOfExperience = doctor.YearsOfExperience,
            Status = doctor.Status,
        };
    }
    
    //ADD Using the Dynamic
    public async Task<DoctorResponseModel> InsertDoctor(DoctorRequestWithoutSidModel doctor)
    {
        try
        {
            var newdoctor = new Doctor
            {
                DoctorSid = string.Concat("DOC", Guid.NewGuid().ToString()),
                FullName = doctor.FullName,
                Email = doctor.Email,
                Phone = doctor.Phone,
                Gender = doctor.Gender,
                YearsOfExperience = doctor.YearsOfExperience,
                Status = (int)DoctorStatus.Active
            };
            
            await _unitOfWork.GetRepository<Doctor>().InsertAsync(newdoctor);
            await  _unitOfWork.CommitAsync();
            
            return new DoctorResponseModel
            {
                DoctorSid = newdoctor.DoctorSid,
                FullName = newdoctor.FullName,
                Email = newdoctor.Email,
                Phone = newdoctor.Phone,
                Gender = newdoctor.Gender,
                YearsOfExperience = newdoctor.YearsOfExperience,
                Status = newdoctor.Status,
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    //Update using the Dynamic
    public async Task<DoctorResponseModel> UpdateDoctordynamic(string doctorSid,DoctorRequestWithoutSidModel doctor)
    {
        var newdoctor = await _unitOfWork.GetRepository<Doctor>()
            .SingleOrDefaultAsync(x =>
                x.DoctorSid == doctorSid && x.Status == (int)DoctorStatus.Active);
        
        if (doctor == null)
            return null;
        
        newdoctor.FullName = doctor.FullName;
        newdoctor.Email = doctor.Email;
        newdoctor.Phone = doctor.Phone;
        newdoctor.Gender = doctor.Gender;
        newdoctor.YearsOfExperience = doctor.YearsOfExperience;
        newdoctor.Status = (int)DoctorStatus.Active;
        newdoctor.ModifiedAt = DateTime.Now;
        
        _unitOfWork.GetRepository<Doctor>().Update(newdoctor);
        await _unitOfWork.CommitAsync();

        return new DoctorResponseModel
        {
            FullName = newdoctor.FullName,
            Email = newdoctor.Email,
            Phone = newdoctor.Phone,
            Gender = newdoctor.Gender,
            YearsOfExperience = newdoctor.YearsOfExperience,
            Status = newdoctor.Status,
        };
    }
    
    //Delete  using the dynamic
    public async Task<bool> DeleteDoctordynamic(string doctorSid)
    {
        var doctors = await _unitOfWork.GetRepository<Doctor>().GetAllAsync();
        var doctor = doctors.FirstOrDefault(x => x.DoctorSid == doctorSid && x.Status == (int)DoctorStatus.Active);
        
        if (doctor == null)
            return false;
        
        doctor.Status = (int)DoctorStatus.Deleted;
        doctor.ModifiedAt = DateTime.Now;
        
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();

        return true;
    }
    
    
    //GetAll Without dynamic
    public List<DoctorResponseModel> GetDoctorsWithSearchAndPaging(string searchTerm, int pageNumber, int pageSize, out int totalCount)
    {
        _logger.LogInformation("Fetching doctors with searchTerm: {SearchTerm}, pageNumber: {PageNumber}, pageSize: {PageSize}", searchTerm, pageNumber, pageSize);

        var query = _context.Doctors.Where(d => d.Status == (int)DoctorStatus.Active);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(d =>
                d.FullName.ToLower().Contains(searchTerm.ToLower()) ||
                d.Email.ToLower().Contains(searchTerm.ToLower()));
        }

        totalCount = query.Count();
        _logger.LogInformation("Total doctors found: {TotalCount}", totalCount);

        var doctors = query
            .OrderBy(d => d.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Returning {DoctorCount} doctors for current page.", doctors.Count);
        return _mapper.Map<List<DoctorResponseModel>>(doctors);
    }
    
    //GetBySID Without dynamic
    public DoctorResponseModel? GetDoctorBySid(string sid)
    {
        _logger.LogInformation("Fetching doctor by SID: {Sid}", sid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == sid);

        if (doctor == null)
        {
            _logger.LogWarning("Doctor with SID {Sid} not found.", sid);
            return null;
        }

        _logger.LogInformation("Doctor with SID {Sid} retrieved successfully.", sid);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    //Add without dynamic
    public DoctorResponseModel AddDoctor(DoctorRequestWithoutSidModel doctor)
    {
        _logger.LogInformation("Adding new doctor: {DoctorName}, Email: {Email}", doctor.FullName, doctor.Email);

        try
        {
            var newDoctor = _mapper.Map<Doctor>(doctor);
            newDoctor.DoctorSid = Guid.NewGuid().ToString();
            newDoctor.Status = (int)DoctorStatus.Active;

            _context.Doctors.Add(newDoctor);
            _context.SaveChanges();

            _logger.LogInformation("Doctor added successfully with SID: {Sid}", newDoctor.DoctorSid);
            return _mapper.Map<DoctorResponseModel>(newDoctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding a new doctor.");
            throw;
        }
    }
    
    //update without dynamic
    public DoctorResponseModel UpdateDoctor(DoctorRequestWithoutSidModel data, string sid)
    {
        _logger.LogInformation("Updating doctor with SID: {Sid}", sid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == sid);

        if (doctor == null)
        {
            _logger.LogWarning("Doctor with SID {Sid} not found for update.", sid);
            return null;
        }

        try
        {
            _mapper.Map(data, doctor);
            doctor.ModifiedAt = DateTime.Now;
            _context.SaveChanges();

            _logger.LogInformation("Doctor with SID {Sid} updated successfully.", sid);
            return _mapper.Map<DoctorResponseModel>(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating doctor with SID: {Sid}", sid);
            throw;
        }
    }
    
    //Delete without dynamic
    public bool SoftDeleteDoctor(string doctorSid)
    {
        _logger.LogInformation("Soft deleting doctor with SID: {Sid}", doctorSid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == doctorSid);

        if (doctor == null)
        {
            _logger.LogWarning("Doctor with SID {Sid} not found for delete.", doctorSid);
            return false;
        }

        try
        {
            doctor.Status = (int)DoctorStatus.Deleted;
            _context.SaveChanges();

            _logger.LogInformation("Doctor with SID {Sid} deleted successfully.", doctorSid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting doctor with SID: {Sid}", doctorSid);
            throw;
        }
    }
}


