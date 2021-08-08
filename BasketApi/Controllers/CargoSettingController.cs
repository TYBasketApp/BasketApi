using DataAccessLayer.Entities;
using DataAccessLayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CargoSettingController : ControllerBase
    {
        private CargoSettingService _cargoSettingService;
        
        public CargoSettingController()
        {
            _cargoSettingService = new CargoSettingService();
        }
        
        [HttpPost]
        public IActionResult Add([FromBody] CargoSettings cargoSettings)
        {
            _cargoSettingService.Add(cargoSettings);

            return Ok();
        }
        
        [HttpPost]
        public IActionResult Update([FromBody] CargoSettings cargoSettings)
        {
            _cargoSettingService.Update(cargoSettings);

            return Ok();
        }
    }
}