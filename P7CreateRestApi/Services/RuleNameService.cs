using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    // The SQL and JSON fields are stored as plain text: this API never executes them.
    public class RuleNameService : IRuleNameService
    {
        private readonly IDataRepository<RuleName> _repository;

        public RuleNameService(IDataRepository<RuleName> repository)
        {
            _repository = repository;
        }

        public async Task<List<RuleNameDto>> GetAllAsync()
        {
            List<RuleName> ruleNames = await _repository.GetAllAsync();

            return ruleNames.Select(ToDto).ToList();
        }

        public async Task<RuleNameDto?> GetByIdAsync(int id)
        {
            RuleName? ruleName = await _repository.GetByIdAsync(id);

            return ruleName == null ? null : ToDto(ruleName);
        }

        public async Task<RuleNameDto> CreateAsync(RuleNameDto dto)
        {
            var ruleName = new RuleName();
            Apply(dto, ruleName);

            await _repository.AddAsync(ruleName);

            return ToDto(ruleName);
        }

        public async Task<bool> UpdateAsync(int id, RuleNameDto dto)
        {
            RuleName? ruleName = await _repository.GetByIdAsync(id);

            if (ruleName == null)
            {
                return false;
            }

            Apply(dto, ruleName);

            await _repository.UpdateAsync(ruleName);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            RuleName? ruleName = await _repository.GetByIdAsync(id);

            if (ruleName == null)
            {
                return false;
            }

            await _repository.DeleteAsync(ruleName);

            return true;
        }

        // The id is never copied from the DTO: it comes from the database or from the route.
        private static void Apply(RuleNameDto dto, RuleName ruleName)
        {
            ruleName.Name = dto.Name;
            ruleName.Description = dto.Description;
            ruleName.Json = dto.Json;
            ruleName.Template = dto.Template;
            ruleName.SqlStr = dto.SqlStr;
            ruleName.SqlPart = dto.SqlPart;
        }

        private static RuleNameDto ToDto(RuleName ruleName)
        {
            return new RuleNameDto
            {
                Id = ruleName.Id,
                Name = ruleName.Name,
                Description = ruleName.Description,
                Json = ruleName.Json,
                Template = ruleName.Template,
                SqlStr = ruleName.SqlStr,
                SqlPart = ruleName.SqlPart
            };
        }
    }
}
