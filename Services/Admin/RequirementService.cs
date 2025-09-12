using AutoMapper;
using BTECH_APP.Entities.Admin;
using BTECH_APP.Helpers;
using BTECH_APP.Models;
using BTECH_APP.Models.Admin.Requirement;
using BTECH_APP.Services.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;
using static BTECH_APP.Enums;

namespace BTECH_APP.Services.Admin
{
    public class RequirementService : IRequirementService
    {
        private readonly BTECHDbContext _dbContext;
        private readonly UserContext _userContext;
        private readonly IMapper _mapper;

        public RequirementService(BTECHDbContext dbContext, UserContext userContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<List<ListRequirementModel>> List()
        {
            var query = from entity in _dbContext.Requirements.AsNoTracking()

                        join modifiedBy in _dbContext.Users.AsNoTracking()
                        on entity.ModifiedByUserId equals modifiedBy.UserId into modifiedByGroup
                        from modifiedBy in modifiedByGroup.DefaultIfEmpty()

                        join information in _dbContext.UserInformations.AsNoTracking()
                            on modifiedBy.PersonId equals information.PersonId into informationGroup
                        from information in informationGroup.DefaultIfEmpty()

                        where !entity.Deleted

                        orderby entity.CreatedDate descending

                        select new ListRequirementModel
                        {
                            RequirementId = entity.RequirementId,
                            Name = entity.Name,
                            IsForFreshmen = entity.IsForFreshmen,
                            IsForTransferee = entity.IsForTransferee,
                            IsForAlsGraduate = entity.IsForAlsGraduate,
                            IsActive = entity.IsActive,
                            IsRequired = entity.IsRequired,
                            Status = entity.IsActive ? "Active" : "Inactive",
                            ModifiedByUserId = entity.ModifiedByUserId,
                            ModifiedByPersonId = information.PersonId,
                            ModifiedByName = information.FullName,
                            ModifiedDate = entity.ModifiedDate,
                        };

            return await query.ToListAsync();
        }
        public async Task<List<LookupRequirementModel>> Lookup(ApplicantTypes applicantTypes)
        {
            var query = _dbContext.Requirements
               .AsNoTracking()
               .Where(x => !x.Deleted);

            if (applicantTypes == ApplicantTypes.Freshmen)
                query = query.Where(x => x.IsForFreshmen);
            else if (applicantTypes == ApplicantTypes.Transferee)
                query = query.Where(x => x.IsForTransferee);
            else
                query = query.Where(x => x.IsForAlsGraduate);

            return await query
                .Select(x => new LookupRequirementModel
                {
                    RequirementId = x.RequirementId,
                    Name = x.Name
                })
                .ToListAsync();
        }
        public async Task<SaveRequirementModel?> Find(int requirementId)
        {
            var query = from entity in _dbContext.Requirements.AsNoTracking()
                        where entity.RequirementId == requirementId
                        select new SaveRequirementModel
                        {
                            RequirementId = entity.RequirementId,
                            Name = entity.Name,
                            IsForFreshmen = entity.IsForFreshmen,
                            IsForTransferee = entity.IsForTransferee,
                            IsForAlsGraduate = entity.IsForAlsGraduate,
                            IsActive = entity.IsActive,
                            IsRequired = entity.IsRequired
                        };

            return await query.FirstOrDefaultAsync();
        }
        public async Task<(bool success, string errorMessage)> Save(SaveRequirementModel model)
        {
            RequirementEntity entity = _mapper.Map<RequirementEntity>(model);

            if (entity.RequirementId != 0)
            {
                var original = await _dbContext.Requirements.FirstOrDefaultAsync(x => x.RequirementId == entity.RequirementId);

                if (original == null)
                    return (false, "Invalid requirement not found!.");

                entity = _mapper.Map(model, original);
            }

            bool isUnique = await IsUnique(entity);

            if (!isUnique)
                return (false, "Requirement is already in the record!.");


            Helper.SetAuditFields(entity.RequirementId, entity, _userContext.CurrentUser.UserId);

            if (entity.RequirementId == 0)
                await _dbContext.Requirements.AddAsync(entity);
            else
                _dbContext.Requirements.Update(entity);

            await _dbContext.SaveChangesAsync();

            return (true, string.Empty);
        }
        public async Task Delete(int requirementId)
        {
            var entity = await _dbContext.Requirements.FirstOrDefaultAsync(x => x.RequirementId == requirementId);

            if (entity != null)
            {
                entity.Deleted = true;
                entity.DeletedDate = DateTime.UtcNow;
                entity.DeletedByUserId = _userContext.CurrentUser.UserId;

                _dbContext.SaveChanges();
            }
        }
        private async Task<bool> IsUnique(RequirementEntity entity)
        {
            return !await _dbContext.Requirements.AsNoTracking()
                .AnyAsync(x => x.RequirementId != entity.RequirementId
                         && x.Name.Contains(entity.Name ?? string.Empty)
                         && !x.Deleted
                         );
        }
    }
}
