using Microsoft.AspNetCore.Mvc;
using WebApiTeaShopManageMent.DAL;
using WebApiTeaShopManageMent.Models;

namespace WebApiTeaShopManageMent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly MenuDal _dal = new MenuDal();

        [HttpPost("POST")]
        public IActionResult MenuInsert([FromBody] Menu menu)
        {
            if (menu == null) return BadRequest("Menu data is null");

            _dal.insertMenu(menu);
            return Ok(new { message = "Menu placed successfully!" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMenu(int id, [FromBody] Menu menu)
        {
            if (menu == null || id != menu.MenuId)
                return BadRequest("Invalid order");

            bool ok = _dal.menuUpdate(menu);
            if (ok) return Ok(new { message = "Menu updated successfully!" });
            return StatusCode(500, "Update failed");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMenu(int id)
        {
            if (id <= 0)
                return BadRequest(new { Message = "Invalid Order ID" });

            bool result = _dal.menuDelete(id);
            if (result)
                return Ok(new { Success = true, Message = "Menu deleted successfully" });
            else
                return NotFound(new { Success = false, Message = "Menu not found" });
        }

        // GET: api/Menu
        [HttpGet]
        public IActionResult GetMenu()
        {
            var menu = _dal.GetMenu();
            return Ok(menu);
        }
    }
}
