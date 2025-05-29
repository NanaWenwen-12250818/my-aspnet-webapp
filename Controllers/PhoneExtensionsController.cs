using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PhoneExtensionsController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<PhoneExtensionsController> _logger;

        public PhoneExtensionsController(WebAPIContext context, ILogger<PhoneExtensionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhoneExtensions()
        {
            try
            {
                var phoneExtensions = await _context.PhoneExtension
                    .Select(p => new
                    {
                        org_name = p.OrgName!.Trim(),
                        subunit_name = p.SubunitName!.Trim(),
                        description = p.Description!.Trim(),
                        extension = p.Extension!.Trim()
                    })
                    .ToListAsync();

                if (phoneExtensions.Any())
                {
                    // 依照 org_name 分類，再依照 subunit_name 分類
                    var groupedData = phoneExtensions
                        .GroupBy(p => p.org_name)
                        .Select(orgGroup => new
                        {
                            org_name = orgGroup.Key,
                            subunits = orgGroup
                                .GroupBy(s => s.subunit_name)
                                .Select(subGroup => new
                                {
                                    subunit_name = subGroup.Key,
                                    items = subGroup.Select(item => new
                                    {
                                        description = item.description,
                                        extension = item.extension
                                    }).ToList()
                                }).ToList()
                        }).ToList();

                    return Ok(new { success = true, data = groupedData });
                }
                else
                {
                    return Ok(new { success = false, message = "找不到任何電話分機資料" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得電話分機資料時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }
    }
}