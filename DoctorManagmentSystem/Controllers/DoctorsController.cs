using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Models.ResponseModel;
using DMS.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private IDoctorRepository _doctorRepository;
    private ILogger<DoctorController> _logger;

    public DoctorController(IDoctorRepository doctorRepository, ILogger<DoctorController> logger)
    {
        _doctorRepository = doctorRepository;
        _logger = logger;
    }
    
    
    //Get all paging and serching
    [HttpGet("getall")]
    public IActionResult SearchDoctors([FromQuery] string? term, [FromQuery] int page = 1, [FromQuery] int size = 5)
    {
        _logger.LogInformation("Searching doctors with SID: {SID}", term);
        try
        {
            var doctors = _doctorRepository.GetDoctorsWithSearchAndPaging(term ?? "", page, size, out int totalCount);

            var response = new PaginatedResponse<DoctorResponseModel>
            {
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = size,
                Data = doctors
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching doctors");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    //Get by SID
    [HttpGet("sid/{doctorSid}")]
    public IActionResult GetDoctorBySid(string doctorSid)
    {
        _logger.LogInformation("Getting doctor with SID: {SID}", doctorSid);
        try
        {
            var doctor = _doctorRepository.GetDoctorBySid(doctorSid);

            if (doctor == null)
                return NotFound($"Doctor with SID '{doctorSid}' not found.");
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting doctor");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    //insert and update 
    [HttpPost("{doctorSid?}")]
    public ActionResult<DoctorResponseModel> InsertOrUpdateDoctor(
        [FromBody] DoctorRequestWithoutSidModel model,
        [FromRoute] string? doctorSid)
    {
        _logger.LogInformation("InsertOrUpdateDoctor called. SID: {SID}", doctorSid ?? "null");

        try
        {
            DoctorResponseModel? doctor = string.IsNullOrEmpty(doctorSid)
                ? _doctorRepository.AddDoctor(model)
                : _doctorRepository.UpdateDoctor(model, doctorSid);

            if (doctor == null)
            {
                _logger.LogWarning("Doctor could not be inserted or updated. SID: {SID}", doctorSid ?? "null");
                return NotFound("Doctor could not be inserted or updated.");
            }

            _logger.LogInformation("Successfully inserted or updated doctor. SID: {SID}", doctorSid ?? "new");
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while inserting/updating doctor. SID: {SID}", doctorSid ?? "null");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    
    
    // Soft delete a doctor (set status = 3)
    [HttpDelete("{doctorSid}")]
    public ActionResult SoftDeleteDoctor(string doctorSid)
    {
        _logger.LogInformation("Deleting doctor with SID: {SID}", doctorSid);
        try
        {
            var result = _doctorRepository.SoftDeleteDoctor(doctorSid);

            if (!result)
                return NotFound($"Doctor with SID '{doctorSid}' not found.");
            
            _logger.LogInformation("Successfully Deleted doctor");
            return Ok("Doctor deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting doctor with SID: {DoctorSid}", doctorSid);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    

}

