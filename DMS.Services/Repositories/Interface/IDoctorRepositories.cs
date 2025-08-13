using DMS.Models.CommonModel;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponseModel;

namespace DMS.Service.Repository.Interface;

public interface IDoctorRepository
{
    Task<Page> List(Dictionary<string, object> parameters);

    Task<DoctorResponseModel?> GetByDoctorSID(string doctorSID);
    
    Task<DoctorResponseModel> InsertDoctor(DoctorRequestWithoutSidModel doctor);

    Task<DoctorResponseModel> UpdateDoctordynamic(string doctorSid, DoctorRequestWithoutSidModel doctor);
    
    Task<bool> DeleteDoctordynamic(string doctorSID);
    public List<DoctorResponseModel> GetDoctorsWithSearchAndPaging(string searchTerm, int pageNumber, int pageSize, out int totalRecords);
    public DoctorResponseModel? GetDoctorBySid(string sid);
    
    DoctorResponseModel AddDoctor(DoctorRequestWithoutSidModel doctor);
    
    public DoctorResponseModel UpdateDoctor(DoctorRequestWithoutSidModel data, string sid);
    
    bool SoftDeleteDoctor(string doctorSid);
    
}