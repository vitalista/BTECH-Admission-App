using AutoMapper;

namespace BTECH_APP
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Admin

            CreateMap<Entities.Admin.Dashboard.AcademicYearEntity, Models.Admin.Dashboard.SaveAcademicYearModel>()
              .ReverseMap();

            CreateMap<Entities.Admin.InstituteEntity, Models.Admin.Institute.SaveInstituteModel>()
              .ReverseMap();

            CreateMap<Entities.Admin.ProgamEntity, Models.Admin.Program.SaveProgramModel>()
              .ReverseMap();

            CreateMap<Entities.Admin.RequirementEntity, Models.Admin.Requirement.SaveRequirementModel>()
               .ReverseMap();

            CreateMap<Entities.Auth.UserEntity, Models.Admin.UserManagement.SaveUserManagementModel>()
               .ReverseMap();

            CreateMap<Entities.Auth.UserInformationEntity, Models.Admin.UserManagement.SaveUserManagementModel>()
               .ReverseMap();

            #endregion Admin

            #region Applicant

            CreateMap<Entities.Applicant.AddressEntity, Models.Applicant.SaveAddressApplicantModel>()
              .ReverseMap();

            CreateMap<Entities.Applicant.ApplicantEntity, Models.Applicant.SaveApplicantModel>()
              .ReverseMap();

            CreateMap<Entities.Applicant.PrevSchoolEntity, Models.Applicant.SavePrevSchoolApplicantModel>()
              .ReverseMap();

            CreateMap<Entities.Applicant.SelectedProgramEntity, Models.Applicant.SaveSelectedProgApplicantModel>()
              .ReverseMap();

            CreateMap<Entities.Auth.UserInformationEntity, Models.Applicant.SaveApplicantModel>()
             .ReverseMap();

            CreateMap<Entities.Applicant.ApplicantRequirementEntity, Models.Applicant.SaveRequirementsApplicantModel>()
            .ReverseMap();

            #endregion Applicant
        }
    }
}
