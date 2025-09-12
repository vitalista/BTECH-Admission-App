using AutoMapper;
using BTECH_APP.Entities.Admin;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.Program;
using BTECH_APP.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BTECH_APP.Services.Admin
{
    public class ProgramService : IProgramService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;

        public ProgramService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<List<ListProgramModel>> List()
        {
            var query = from entity in _dbContext.Progams.AsNoTracking()

                        join modifiedBy in _dbContext.Users.AsNoTracking()
                        on entity.ModifiedByUserId equals modifiedBy.UserId into modifiedByGroup
                        from modifiedBy in modifiedByGroup.DefaultIfEmpty()

                        join information in _dbContext.UserInformations.AsNoTracking()
                            on modifiedBy.PersonId equals information.PersonId into informationGroup
                        from information in informationGroup.DefaultIfEmpty()

                        join institute in _dbContext.Institutes.AsNoTracking()
                        on entity.InstituteId equals institute.InstituteId

                        where !entity.Deleted

                        orderby entity.CreatedDate descending

                        select new ListProgramModel
                        {
                            ProgramId = entity.ProgramId,
                            InstituteName = institute.Name,
                            InstituteAcronym = institute.Acronym,
                            Name = entity.Name,
                            Acronym = entity.Acronym,
                            IsActive = entity.IsActive,
                            Status = entity.IsActive ? "Active" : "Inactive",
                            ModifiedByUserId = entity.ModifiedByUserId,
                            ModifiedByPersonId = information.PersonId,
                            ModifiedByName = information.FullName,
                            ModifiedDate = entity.ModifiedDate,
                        };

            return await query.ToListAsync();
        }
        public async Task<List<LookupProgramModel>> Lookup(int[]? excludeIds)
        {
            var query = _dbContext.Progams
                .AsNoTracking()
                .Where(x => !x.Deleted);

            if (excludeIds != null && excludeIds.Length > 0)
            {
                query = query.Where(x => !excludeIds.Contains(x.ProgramId));
            }

            return await query
                .Select(x => new LookupProgramModel
                {
                    ProgramId = x.ProgramId,
                    Name = $"{x.Name} ({x.Acronym})",
                })
                .ToListAsync();
        }

        public async Task<SaveProgramModel?> Find(int programId)
        {
            var query = from entity in _dbContext.Progams.AsNoTracking()
                        where entity.ProgramId == programId
                        select new SaveProgramModel
                        {
                            ProgramId = entity.InstituteId,
                            InstituteId = entity.InstituteId,
                            Name = entity.Name,
                            Acronym = entity.Acronym,
                            IsActive = entity.IsActive,
                        };

            return await query.FirstOrDefaultAsync();
        }
        public async Task<(bool success, string errorMessage)> Save(SaveProgramModel model)
        {
            ProgamEntity entity = _mapper.Map<ProgamEntity>(model);

            if (entity.ProgramId != 0)
            {
                var original = await _dbContext.Progams.FirstOrDefaultAsync(x => x.ProgramId == entity.ProgramId);

                if (original == null)
                    return (false, "Invalid program not found!.");

                entity = _mapper.Map(model, original);
            }

            bool isUnique = await IsUnique(entity);

            if (!isUnique)
                return (false, "Program is already in the record!.");

            Helper.SetAuditFields(entity.ProgramId, entity, _userContext.CurrentUser.UserId);

            if (entity.InstituteId == 0)
                await _dbContext.Progams.AddAsync(entity);
            else
                _dbContext.Progams.Update(entity);

            await _dbContext.SaveChangesAsync();

            return (true, string.Empty);
        }
        public async Task Delete(int programId)
        {
            var entity = await _dbContext.Progams.FirstOrDefaultAsync(x => x.ProgramId == programId);

            if (entity != null)
            {
                entity.Deleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.DeletedByUserId = _userContext.CurrentUser.UserId;

                _dbContext.SaveChanges();
            }
        }
        private async Task<bool> IsUnique(ProgamEntity entity)
        {
            return !await _dbContext.Progams.AsNoTracking()
                .AnyAsync(x => x.ProgramId != entity.ProgramId
                         && x.Name.Contains(entity.Name ?? string.Empty)
                         && x.Acronym.Contains(entity.Acronym ?? string.Empty)
                         && x.InstituteId.Equals(entity.InstituteId)
                         && !x.Deleted
                         );
        }
    }
}