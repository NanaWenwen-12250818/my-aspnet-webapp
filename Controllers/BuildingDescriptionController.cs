using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<BuildingController> _logger;

        public BuildingController(WebAPIContext context, ILogger<BuildingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> BuildingGet(string? building_id = null)
        {
            try
            {
                // 如果提供了building_id，返回特定建築的資訊
                if (!string.IsNullOrEmpty(building_id))
                {
                    var buildingEntity = await _context.BuildingDescription
                        .Where(b => b.BuildingId.Trim() == building_id)
                        .FirstOrDefaultAsync();

                    if (buildingEntity != null)
                    {
                        var building = new
                        {
                            building_id = buildingEntity.BuildingId.Trim(),
                            name = buildingEntity.Name?.Trim() ?? string.Empty,
                            description = buildingEntity.Description?.Trim() ?? string.Empty
                        };

                        return Ok(new
                        {
                            success = true,
                            message = "找到對應的建築資訊",
                            building_id = building.building_id,
                            name = building.name,
                            description = building.description
                        });
                    }
                    else
                    {
                        return Ok(new { success = false, message = "找不到對應的建築資訊" });
                    }
                }
                // 如果沒有提供building_id，返回所有建築的列表
                else
                {
                    var buildingEntities = await _context.BuildingDescription.ToListAsync();
                    var buildings = buildingEntities.Select(b => new
                    {
                        building_id = b.BuildingId.Trim(),
                        name = b.Name?.Trim() ?? string.Empty,
                        description = b.Description?.Trim() ?? string.Empty
                    }).ToList();

                    if (buildings.Any())
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "成功獲取建築列表",
                            buildings = buildings
                        });
                    }
                    else
                    {
                        return Ok(new { success = false, message = "沒有建築資訊" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得建築資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }
    }
}