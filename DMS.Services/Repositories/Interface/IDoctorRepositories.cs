using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponseModel;

namespace DMS.Service.Repository.Interface;

public interface IDoctorRepository
{
    public List<DoctorResponseModel> GetDoctorsWithSearchAndPaging(string searchTerm, int pageNumber, int pageSize, out int totalRecords);
    public DoctorResponseModel? GetDoctorBySid(string sid);
    
    DoctorResponseModel AddDoctor(DoctorRequestWithoutSidModel doctor);
    
    public DoctorResponseModel UpdateDoctor(DoctorRequestWithoutSidModel data, string sid);
    
    bool SoftDeleteDoctor(string doctorSid);
    
}