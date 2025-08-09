using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WarehouseManage.Constants;
using WarehouseManage.Data;
using WarehouseManage.DTOs.GoodsIssue;
using WarehouseManage.DTOs.GoodsReceipt;
using WarehouseManage.Interfaces;
using WarehouseManage.Model;

namespace WarehouseManage.Services;

public class GoodsIssueService : IGoodsIssueService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly IGoodsIssueRepository _goodsIssueRepository;

    public GoodsIssueService(
        WarehouseDbContext context, 
        IMapper mapper,
        IGoodsIssueRepository goodsIssueRepository)
    {
        _context = context;
        _mapper = mapper;
        _goodsIssueRepository = goodsIssueRepository;
    }

    public async Task<PagedResult<GoodsIssueDto>> GetGoodsIssuesAsync(GoodsIssueFilterDto filter)
    {
        var result = await _goodsIssueRepository.GetGoodsIssuesAsync(filter);
        var dtos = _mapper.Map<List<GoodsIssueDto>>(result.Items);
        
        return new PagedResult<GoodsIssueDto>(dtos, result.TotalCount, result.PageNumber, result.PageSize);
    }

    public async Task<GoodsIssueDto?> GetGoodsIssueByIdAsync(int id)
    {
        var goodsIssue = await _goodsIssueRepository.GetGoodsIssueByIdAsync(id);
        return goodsIssue != null ? _mapper.Map<GoodsIssueDto>(goodsIssue) : null;
    }

    public async Task<GoodsIssueDto?> GetGoodsIssueByNumberAsync(string issueNumber)
    {
        var goodsIssue = await _goodsIssueRepository.GetGoodsIssueByNumberAsync(issueNumber);
        return goodsIssue != null ? _mapper.Map<GoodsIssueDto>(goodsIssue) : null;
    }

    public async Task<GoodsIssueDto> CreateGoodsIssueAsync(CreateGoodsIssueDto dto, int createdByUserId)
    {
        // Validate
        if (!await ValidateGoodsIssueAsync(dto))
            throw new ArgumentException("Invalid goods issue data");

        // Get user role to determine initial status
        var user = await _context.Users.FindAsync(createdByUserId);
        if (user == null)
            throw new ArgumentException("User not found");

        // Create goods issue entity
        var goodsIssue = _mapper.Map<GoodsIssue>(dto);
        
        // Set system fields
        goodsIssue.IssueNumber = await _goodsIssueRepository.GenerateNextIssueNumberAsync();
        goodsIssue.CreatedByUserId = createdByUserId;
        goodsIssue.IssueDate = DateTime.UtcNow;
        goodsIssue.CreatedAt = DateTime.UtcNow;
        goodsIssue.UpdatedAt = DateTime.UtcNow;

        // Set initial status based on user role
        goodsIssue.Status = GetInitialStatusByRole(user.Role!);
        
        if (goodsIssue.Status == GoodsIssueConstants.Status.Approved)
        {
            // Manager/Admin created - auto approve
            goodsIssue.ApprovedByUserId = createdByUserId;
            goodsIssue.ApprovedDate = DateTime.UtcNow;
        }

        // Calculate total amount
        goodsIssue.TotalAmount = goodsIssue.GoodsIssueDetails.Sum(d => d.Quantity * d.UnitPrice);

        var result = await _goodsIssueRepository.CreateGoodsIssueAsync(goodsIssue);
        
        // Load related data for response
        var createdGoodsIssue = await _goodsIssueRepository.GetGoodsIssueByIdAsync(result.GoodsIssueId);
        return _mapper.Map<GoodsIssueDto>(createdGoodsIssue);
    }

    public async Task<GoodsIssueDto> UpdateGoodsIssueAsync(UpdateGoodsIssueDto dto)
    {
        var existingGoodsIssue = await _goodsIssueRepository.GetGoodsIssueByIdAsync(dto.GoodsIssueId);
        if (existingGoodsIssue == null)
            throw new ArgumentException(GoodsIssueConstants.ErrorMessages.GoodsIssueNotFound);

        // Can only update Draft and AwaitingApproval status
        if (existingGoodsIssue.Status != GoodsIssueConstants.Status.Draft && 
            existingGoodsIssue.Status != GoodsIssueConstants.Status.AwaitingApproval)
            throw new InvalidOperationException(GoodsIssueConstants.ErrorMessages.InvalidStatusTransition);

        // Update fields
        existingGoodsIssue.CustomerId = dto.CustomerId;
        existingGoodsIssue.RequestedDeliveryDate = dto.RequestedDeliveryDate;
        existingGoodsIssue.Notes = dto.Notes;

        // Remove existing details and add new ones
        _context.GoodsIssueDetails.RemoveRange(existingGoodsIssue.GoodsIssueDetails);
        
        var newDetails = _mapper.Map<List<GoodsIssueDetail>>(dto.Details);
        foreach (var detail in newDetails)
        {
            detail.GoodsIssueId = existingGoodsIssue.GoodsIssueId;
        }
        
        existingGoodsIssue.GoodsIssueDetails = newDetails;
        existingGoodsIssue.TotalAmount = newDetails.Sum(d => d.Quantity * d.UnitPrice);

        var result = await _goodsIssueRepository.UpdateGoodsIssueAsync(existingGoodsIssue);
        
        // Load related data for response
        var updatedGoodsIssue = await _goodsIssueRepository.GetGoodsIssueByIdAsync(result.GoodsIssueId);
        return _mapper.Map<GoodsIssueDto>(updatedGoodsIssue);
    }

    public async Task<bool> DeleteGoodsIssueAsync(int id)
    {
        if (!await CanDeleteGoodsIssueAsync(id))
            return false;

        return await _goodsIssueRepository.DeleteGoodsIssueAsync(id);
    }

    public async Task<List<GoodsIssueDto>> GetGoodsIssuesByCustomerAsync(int customerId)
    {
        var goodsIssues = await _goodsIssueRepository.GetGoodsIssuesByCustomerAsync(customerId);
        return _mapper.Map<List<GoodsIssueDto>>(goodsIssues);
    }

    public async Task<bool> ValidateGoodsIssueAsync(CreateGoodsIssueDto dto)
    {
        // Check if details exist
        if (dto.Details == null || !dto.Details.Any())
            return false;

        // Check if customer exists (if specified)
        if (dto.CustomerId.HasValue)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == dto.CustomerId);
            if (!customerExists)
                return false;
        }

        // Check if all products exist
        var productIds = dto.Details.Select(d => d.ProductId).ToList();
        var existingProductCount = await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .CountAsync();

        if (existingProductCount != productIds.Count)
            return false;

        // Validate quantities and prices
        foreach (var detail in dto.Details)
        {
            if (detail.Quantity <= 0 || detail.UnitPrice <= 0)
                return false;
        }

        return true;
    }

    public async Task<bool> CanDeleteGoodsIssueAsync(int id)
    {
        var goodsIssue = await _goodsIssueRepository.GetGoodsIssueByIdAsync(id);
        if (goodsIssue == null)
            return false;

        // Can only delete Draft or Rejected status
        return goodsIssue.Status == GoodsIssueConstants.Status.Draft || 
               goodsIssue.Status == GoodsIssueConstants.Status.Rejected;
    }

    private string GetInitialStatusByRole(string userRole)
    {
        return userRole == RoleConstants.Employee 
            ? GoodsIssueConstants.Status.AwaitingApproval 
            : GoodsIssueConstants.Status.Approved;
    }
}
