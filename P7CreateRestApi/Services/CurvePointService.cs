using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public class CurvePointService : ICurvePointService
    {
        private readonly IDataRepository<CurvePoint> _repository;

        public CurvePointService(IDataRepository<CurvePoint> repository)
        {
            _repository = repository;
        }

        public async Task<List<CurvePointDto>> GetAllAsync()
        {
            List<CurvePoint> curvePoints = await _repository.GetAllAsync();

            return curvePoints.Select(ToDto).ToList();
        }

        public async Task<CurvePointDto?> GetByIdAsync(int id)
        {
            CurvePoint? curvePoint = await _repository.GetByIdAsync(id);

            return curvePoint == null ? null : ToDto(curvePoint);
        }

        public async Task<CurvePointDto> CreateAsync(CurvePointDto dto)
        {
            DateTime now = DateTime.UtcNow;
            var curvePoint = new CurvePoint
            {
                CurveId = dto.CurveId,
                Term = dto.Term,
                CurvePointValue = dto.CurvePointValue,
                AsOfDate = now,
                CreationDate = now
            };

            await _repository.AddAsync(curvePoint);

            return ToDto(curvePoint);
        }

        public async Task<bool> UpdateAsync(int id, CurvePointDto dto)
        {
            CurvePoint? curvePoint = await _repository.GetByIdAsync(id);

            if (curvePoint == null)
            {
                return false;
            }

            curvePoint.CurveId = dto.CurveId;
            curvePoint.Term = dto.Term;
            curvePoint.CurvePointValue = dto.CurvePointValue;
            curvePoint.AsOfDate = DateTime.UtcNow;

            await _repository.UpdateAsync(curvePoint);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            CurvePoint? curvePoint = await _repository.GetByIdAsync(id);

            if (curvePoint == null)
            {
                return false;
            }

            await _repository.DeleteAsync(curvePoint);

            return true;
        }

        private static CurvePointDto ToDto(CurvePoint curvePoint)
        {
            return new CurvePointDto
            {
                Id = curvePoint.Id,
                CurveId = curvePoint.CurveId,
                Term = curvePoint.Term,
                CurvePointValue = curvePoint.CurvePointValue
            };
        }
    }
}
