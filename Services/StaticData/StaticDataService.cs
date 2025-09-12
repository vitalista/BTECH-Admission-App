using BTECH_APP.Entities.StaticData;
using BTECH_APP.Models.StaticData;
using BTECH_APP.Services.StaticData.Interfaces;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace BTECH_APP.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public StaticDataService(BTECHDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _env = webHostEnvironment;
        }

        public async Task RunStaticData()
        {
            await SaveProvince();
            await SaveMunicipality();
            await SaveBarangay();
        }

        public async Task SaveProvince()
        {
            bool hasData = await _dbContext.Provinces.AsNoTracking().AnyAsync();

            if (!hasData)
            {
                var filePath = Path.Combine(_env.WebRootPath, "StaticData", "table_province.csv");

                if (File.Exists(filePath))
                {
                    List<SaveProvinceModel> datas = new();

                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true
                    }))
                    {
                        datas = csv.GetRecords<SaveProvinceModel>().ToList();
                    }

                    var provinces = datas.Select(data => new ProvinceEntity
                    {
                        ProvinceId = data.ProvinceId,
                        Name = data.Name
                    }).ToList();

                    await _dbContext.Provinces.AddRangeAsync(provinces);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }


        public async Task SaveMunicipality()
        {
            bool hasData = await _dbContext.Municipalities.AsNoTracking().AnyAsync();

            if (!hasData)
            {
                var filePath = Path.Combine(_env.WebRootPath, "StaticData", "table_municipality.csv");

                if (File.Exists(filePath))
                {
                    List<SaveMunicipalityModel> datas = new();

                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true
                    }))
                    {
                        datas = csv.GetRecords<SaveMunicipalityModel>().ToList();
                    }

                    var municipalities = datas.Select(data => new MunicipalityEntity
                    {
                        MunicipalityId = data.MunicipalityId,
                        ProvinceId = data.ProvinceId,
                        Name = data.Name
                    }).ToList();

                    await _dbContext.Municipalities.AddRangeAsync(municipalities);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task SaveBarangay()
        {
            bool hasData = await _dbContext.Barangays.AsNoTracking().AnyAsync();

            if (!hasData)
            {
                var filePath = Path.Combine(_env.WebRootPath, "StaticData", "table_barangay.csv");

                if (File.Exists(filePath))
                {
                    List<SaveBarangayModel> datas = new();

                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true
                    }))
                    {
                        datas = csv.GetRecords<SaveBarangayModel>().ToList();
                    }

                    var barangays = datas.Select(data => new BarangayEntity
                    {
                        BarangayId = data.BarangayId,
                        MunicipalityId = data.MunicipalityId,
                        Name = data.Name
                    }).ToList();

                    await _dbContext.Barangays.AddRangeAsync(barangays);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
