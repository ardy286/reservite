using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;

namespace ReserViteApplication.Controllers
{
    public class AdminsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminsController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GererChambres()
        {
            return View(await _context.Chambres.ToListAsync());
        }
    }
}
