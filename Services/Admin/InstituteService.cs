using AutoMapper;
using BTECH_APP.Entities.Admin;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.Institute;
using BTECH_APP.Services.Admin.Interfaces;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BTECH_APP.Services.Admin
{
    public class InstituteService : IInstituteService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;

        public InstituteService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<List<ListInstituteModel>> List()
        {
            var query = from entity in _dbContext.Institutes.AsNoTracking()

                        join modifiedBy in _dbContext.Users.AsNoTracking()
                        on entity.ModifiedByUserId equals modifiedBy.UserId into modifiedByGroup
                        from modifiedBy in modifiedByGroup.DefaultIfEmpty()

                        join information in _dbContext.UserInformations.AsNoTracking()
                            on modifiedBy.PersonId equals information.PersonId into informationGroup
                        from information in informationGroup.DefaultIfEmpty()

                        where !entity.Deleted

                        orderby entity.CreatedDate descending

                        select new ListInstituteModel
                        {
                            InstituteId = entity.InstituteId,
                            Name = entity.Name,
                            Acronym = entity.Acronym,
                            IsActive = entity.IsActive,
                            Status = entity.IsActive ? "Active" : "Inactive",
                            FilePath = entity.FilePath ?? string.Empty,
                            FileType = entity.FileType,
                            ModifiedByUserId = entity.ModifiedByUserId,
                            ModifiedByPersonId = information.PersonId,
                            ModifiedByName = information.FullName,
                            ModifiedDate = entity.ModifiedDate,
                        };

            return await query.ToListAsync();
        }
        public async Task<List<LookupInstituteModel>> Lookup()
        {
            var query = from entity in _dbContext.Institutes.AsNoTracking()
                        where !entity.Deleted && entity.IsActive
                        select new LookupInstituteModel
                        {
                            InstituteId = entity.InstituteId,
                            Name = $"{entity.Name} ({entity.Acronym})",
                        };

            return await query.ToListAsync();
        }
        public async Task<SaveInstituteModel?> Find(int instituteId)
        {
            var query = from entity in _dbContext.Institutes.AsNoTracking()
                        where entity.InstituteId == instituteId
                        select new SaveInstituteModel
                        {
                            InstituteId = entity.InstituteId,
                            Name = entity.Name,
                            Acronym = entity.Acronym,
                            IsActive = entity.IsActive,
                            FileName = entity.FileName,
                            FilePath = entity.FilePath ?? string.Empty,
                            FileType = entity.FileType,
                        };

            return await query.FirstOrDefaultAsync();
        }
        public async Task<(bool success, string errorMessage)> Save(SaveInstituteModel model, IBrowserFile? file)
        {

            InstituteEntity entity = _mapper.Map<InstituteEntity>(model);

            if (entity.InstituteId != 0)
            {
                var original = await _dbContext.Institutes.FirstOrDefaultAsync(x => x.InstituteId == entity.InstituteId);

                if (original == null)
                    return (false, "Invalid institute not found!.");

                entity = _mapper.Map(model, original);
            }

            bool isUnique = await IsUnique(entity);

            if (!isUnique)
                return (false, "Institute is already in the record!.");

            if (entity.InstituteId == 0 && (file == null || file.Size == 0))
            {
                return (false, "Institute Logo is required");
            }
            else
            {
                if (file != null && file.Size != 0)
                {
                    var uploadFile = await Helper.UploadFile(file, "Institute");

                    entity.FileName = uploadFile.FileName;
                    entity.FileType = uploadFile.FileType;
                    entity.FilePath = uploadFile.FilePath;
                }
            }

            Helper.SetAuditFields(entity.InstituteId, entity, _userContext.CurrentUser.UserId);

            if (entity.InstituteId == 0)
                await _dbContext.Institutes.AddAsync(entity);
            else
                _dbContext.Institutes.Update(entity);

            await _dbContext.SaveChangesAsync();

            return (true, string.Empty);
        }
        public async Task Delete(int instituteId)
        {
            var entity = await _dbContext.Institutes.FirstOrDefaultAsync(x => x.InstituteId == instituteId);

            if (entity != null)
            {
                entity.Deleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.DeletedByUserId = _userContext.CurrentUser.UserId;

                _dbContext.SaveChanges();
            }

        }
        private async Task<bool> IsUnique(InstituteEntity entity)
        {
            return !await _dbContext.Institutes.AsNoTracking()
                .AnyAsync(x => x.InstituteId != entity.InstituteId
                         && x.Name.Contains(entity.Name ?? string.Empty)
                         && x.Acronym.Contains(entity.Acronym ?? string.Empty)
                         && !x.Deleted
                         );
        }
    }
}