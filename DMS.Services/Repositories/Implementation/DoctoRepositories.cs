using AutoMapper;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponseModel;
using DMS.Service.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace DMS.Service.Repository.Implementation;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorRepository> _logger;

    public DoctorRepository(DoctorsDbContext context, IMapper mapper, ILogger<DoctorRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

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
