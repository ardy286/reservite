using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;
using ReserViteApplication.Models;

namespace ReserViteApplication.Controllers
{
    public class PaiementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaiementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Paiements
        public async Task<IActionResult> Index()
        {
            return View(await _context.Paiement.ToListAsync());
        }

        public IActionResult Index(int idChambre, int idUt)
        {
            var montant = 100; // Exemple de montant à calculer

            // Ici, vous pouvez également récupérer une réservation liée si besoin
            var viewModel = new Paiement
            {
                Montant = montant,
                DatePaiement = DateTime.Now,
                ChambreId = idChambre,
                UtilisateurId = idUt,
                ReservationId = 1 // Assigner l'ID de la réservation approprié, si disponible
            };

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult ProcessPaiement(Paiement model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Logique pour enregistrer le paiement
            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            return View();
        }

        // GET: Paiements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paiement = await _context.Paiement
                .FirstOrDefaultAsync(m => m.PaiementId == id);
            if (paiement == null)
            {
                return NotFound();
            }

            return View(paiement);
        }

        // GET: Paiements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Paiements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaiementId,Montant,DatePaiement,ModeDePaiement")] Paiement paiement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paiement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paiement);
        }

        // GET: Paiements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paiement = await _context.Paiement.FindAsync(id);
            if (paiement == null)
            {
                return NotFound();
            }
            return View(paiement);
        }

        // POST: Paiements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaiementId,Montant,DatePaiement,ModeDePaiement")] Paiement paiement)
        {
            if (id != paiement.PaiementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paiement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaiementExists(paiement.PaiementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paiement);
        }

        // GET: Paiements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paiement = await _context.Paiement
                .FirstOrDefaultAsync(m => m.PaiementId == id);
            if (paiement == null)
            {
                return NotFound();
            }

            return View(paiement);
        }

        // POST: Paiements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paiement = await _context.Paiement.FindAsync(id);
            if (paiement != null)
            {
                _context.Paiement.Remove(paiement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaiementExists(int id)
        {
            return _context.Paiement.Any(e => e.PaiementId == id);
        }
    }
}
