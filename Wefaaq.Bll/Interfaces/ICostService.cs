using Wefaaq.Bll.DTOs;

namespace Wefaaq.Bll.Interfaces;

/// <summary>
/// Business cost / expense service interface (المصروفات)
/// </summary>
public interface ICostService
{
    Task<IEnumerable<CostDto>> GetAllAsync();

    Task<IEnumerable<CostDto>> GetByDateRangeAsync(DateTime from, DateTime to);

    Task<CostDto> CreateAsync(CostCreateDto dto);

    Task<bool> DeleteAsync(Guid id);
}
