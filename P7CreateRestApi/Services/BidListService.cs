using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public class BidListService : IBidListService
    {
        private readonly IDataRepository<BidList> _repository;

        public BidListService(IDataRepository<BidList> repository)
        {
            _repository = repository;
        }

        public async Task<List<BidListDto>> GetAllAsync()
        {
            List<BidList> bidLists = await _repository.GetAllAsync();

            return bidLists.Select(ToDto).ToList();
        }

        public async Task<BidListDto?> GetByIdAsync(int id)
        {
            BidList? bidList = await _repository.GetByIdAsync(id);

            return bidList == null ? null : ToDto(bidList);
        }

        public async Task<BidListDto> CreateAsync(BidListDto dto, string? userName)
        {
            var bidList = new BidList
            {
                Account = dto.Account,
                BidType = dto.BidType,
                BidQuantity = dto.BidQuantity,
                CreationName = userName,
                CreationDate = DateTime.UtcNow
            };

            await _repository.AddAsync(bidList);

            return ToDto(bidList);
        }

        public async Task<bool> UpdateAsync(int id, BidListDto dto, string? userName)
        {
            BidList? bidList = await _repository.GetByIdAsync(id);

            if (bidList == null)
            {
                return false;
            }

            bidList.Account = dto.Account;
            bidList.BidType = dto.BidType;
            bidList.BidQuantity = dto.BidQuantity;
            bidList.RevisionName = userName;
            bidList.RevisionDate = DateTime.UtcNow;

            await _repository.UpdateAsync(bidList);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            BidList? bidList = await _repository.GetByIdAsync(id);

            if (bidList == null)
            {
                return false;
            }

            await _repository.DeleteAsync(bidList);

            return true;
        }

        private static BidListDto ToDto(BidList bidList)
        {
            return new BidListDto
            {
                BidListId = bidList.BidListId,
                Account = bidList.Account,
                BidType = bidList.BidType,
                BidQuantity = bidList.BidQuantity
            };
        }
    }
}
