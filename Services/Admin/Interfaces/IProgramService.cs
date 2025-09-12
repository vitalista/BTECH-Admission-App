using BTECH_APP.Models.Admin.Program;

namespace BTECH_APP.Services.Admin.Interfaces
{
    public interface IProgramService
    {
        Task<List<ListProgramModel>> List();
        Task<List<LookupProgramModel>> Lookup(int[]? excludeIds);
        Task<SaveProgramModel?> Find(int programId);
        Task<(bool success, string errorMessage)> Save(SaveProgramModel model);
        Task Delete(int programId);
    }
}
