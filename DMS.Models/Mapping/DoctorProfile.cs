using AutoMapper;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponseModel;

namespace DMS.Models.Mapping;

public class DoctorProfile : Profile
{
    public DoctorProfile()
    {
        CreateMap<DoctorRequestWithoutSidModel, Doctor>();
        CreateMap<Doctor, DoctorResponseModel>();
    }
}
