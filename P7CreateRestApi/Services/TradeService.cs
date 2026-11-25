using Dot.Net.WebApi.Domain;
using Dot.Net.WebApi.Repositories;
using P7CreateRestApi.Models;

namespace Dot.Net.WebApi.Services
{
    public class TradeService : ITradeService
    {
        private readonly IDataRepository<Trade> _repository;

        public TradeService(IDataRepository<Trade> repository)
        {
            _repository = repository;
        }

        public async Task<List<TradeDto>> GetAllAsync()
        {
            List<Trade> trades = await _repository.GetAllAsync();

            return trades.Select(ToDto).ToList();
        }

        public async Task<TradeDto?> GetByIdAsync(int id)
        {
            Trade? trade = await _repository.GetByIdAsync(id);

            return trade == null ? null : ToDto(trade);
        }

        public async Task<TradeDto> CreateAsync(TradeDto dto, string? userName)
        {
            var trade = new Trade
            {
                Account = dto.Account,
                AccountType = dto.AccountType,
                BuyQuantity = dto.BuyQuantity,
                CreationName = userName,
                CreationDate = DateTime.UtcNow
            };

            await _repository.AddAsync(trade);

            return ToDto(trade);
        }

        public async Task<bool> UpdateAsync(int id, TradeDto dto, string? userName)
        {
            Trade? trade = await _repository.GetByIdAsync(id);

            if (trade == null)
            {
                return false;
            }

            trade.Account = dto.Account;
            trade.AccountType = dto.AccountType;
            trade.BuyQuantity = dto.BuyQuantity;
            trade.RevisionName = userName;
            trade.RevisionDate = DateTime.UtcNow;

            await _repository.UpdateAsync(trade);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Trade? trade = await _repository.GetByIdAsync(id);

            if (trade == null)
            {
                return false;
            }

            await _repository.DeleteAsync(trade);

            return true;
        }

        private static TradeDto ToDto(Trade trade)
        {
            return new TradeDto
            {
                TradeId = trade.TradeId,
                Account = trade.Account,
                AccountType = trade.AccountType,
                BuyQuantity = trade.BuyQuantity
            };
        }
    }
}
