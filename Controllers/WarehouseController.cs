using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManage.Constants;
using WarehouseManage.DTOs.Warehouse;
using WarehouseManage.Interfaces;

namespace WarehouseManage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehouseController> _logger;

    public WarehouseController(IWarehouseService warehouseService, ILogger<WarehouseController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách tất cả kho hàng
    /// </summary>
    /// <returns>Danh sách kho hàng</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAllWarehouses()
    {
        try
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách kho hàng thành công",
                data = warehouses
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all warehouses");
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.General.INTERNAL_ERROR
            });
        }
    }

    /// <summary>
    /// Lấy thông tin kho hàng theo ID
    /// </summary>
    /// <param name="id">ID của kho hàng</param>
    /// <returns>Thông tin kho hàng</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetWarehouseById(int id)
    {
        try
        {
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id);
            if (warehouse == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = ErrorMessages.Warehouse.NOT_FOUND
                });
            }

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin kho hàng thành công",
                data = warehouse
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.General.INTERNAL_ERROR
            });
        }
    }

    /// <summary>
    /// Tạo kho hàng mới
    /// </summary>
    /// <param name="warehouseDto">Thông tin kho hàng cần tạo</param>
    /// <returns>Thông tin kho hàng đã tạo</returns>
    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse([FromBody] WarehouseDto warehouseDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ErrorMessages.General.INVALID_INPUT,
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Đảm bảo đây là create request
            warehouseDto.WarehouseId = null;

            var warehouse = await _warehouseService.CreateWarehouseAsync(warehouseDto);
            return CreatedAtAction(nameof(GetWarehouseById), new { id = warehouse.WarehouseId }, new
            {
                success = true,
                message = "Tạo kho hàng thành công",
                data = warehouse
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse");
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.Warehouse.CREATE_FAILED
            });
        }
    }

    /// <summary>
    /// Cập nhật thông tin kho hàng
    /// </summary>
    /// <param name="id">ID của kho hàng</param>
    /// <param name="warehouseDto">Thông tin cập nhật</param>
    /// <returns>Thông tin kho hàng đã cập nhật</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseDto>> UpdateWarehouse(int id, [FromBody] WarehouseDto warehouseDto)
    {
        try
        {
            if (id != warehouseDto.WarehouseId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID trong URL không khớp với ID trong body"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ErrorMessages.General.INVALID_INPUT,
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // Đảm bảo có ID cho update
            warehouseDto.WarehouseId = id;

            var warehouse = await _warehouseService.UpdateWarehouseAsync(warehouseDto);
            return Ok(new
            {
                success = true,
                message = "Cập nhật kho hàng thành công",
                data = warehouse
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.Warehouse.UPDATE_FAILED
            });
        }
    }

    /// <summary>
    /// Xóa kho hàng
    /// </summary>
    /// <param name="id">ID của kho hàng</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWarehouse(int id)
    {
        try
        {
            var result = await _warehouseService.DeleteWarehouseAsync(id);
            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = ErrorMessages.Warehouse.NOT_FOUND
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xóa kho hàng thành công"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.Warehouse.DELETE_FAILED
            });
        }
    }

    /// <summary>
    /// Kiểm tra kho hàng có tồn tại không
    /// </summary>
    /// <param name="id">ID của kho hàng</param>
    /// <returns>True nếu tồn tại, False nếu không</returns>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<bool>> CheckWarehouseExists(int id)
    {
        try
        {
            var exists = await _warehouseService.WarehouseExistsAsync(id);
            return Ok(new
            {
                success = true,
                data = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking warehouse existence {WarehouseId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = ErrorMessages.General.INTERNAL_ERROR
            });
        }
    }
}
