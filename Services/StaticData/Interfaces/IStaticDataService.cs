namespace BTECH_APP.Services.StaticData.Interfaces
{
    public interface IStaticDataService
    {
        Task RunStaticData();
        Task SaveProvince();
        Task SaveMunicipality();
        Task SaveBarangay();
    }
}
