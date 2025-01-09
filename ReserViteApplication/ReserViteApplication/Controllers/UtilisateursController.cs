using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;
using ReserViteApplication.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Scripting;
using ReserViteApplication.Services;

namespace ReserViteApplication.Controllers
{
    public class UtilisateursController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ISMSSenderService _smsSenderService;

        public UtilisateursController(ApplicationDbContext context, ISMSSenderService smsSenderService)
        {
            _context = context;
            _smsSenderService = smsSenderService;
        }

        


        public IActionResult Paiement(int idChambre, int idUt)
        {
            var montant = 100;
            var viewModel = new Paiement
            {
                Montant = montant,
                DatePaiement = DateTime.Now,
                ChambreId = idChambre,
                UtilisateurId = idUt,
                ReservationId = 1
            };

            return View(viewModel);
        }

        public IActionResult MessagerieUt()
        {
            return View();
        }

        public IActionResult demandeNumero()
        {
            return View();
        }

        public async Task<IActionResult> resetPassword(string numero)
        {
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Numero == numero);
            if (utilisateur != null)
            {
                // Générer un nouveau mot de passe aléatoire
                var nouveauMotDePasse = Guid.NewGuid().ToString().Substring(0, 8);

                // Mettre à jour le mot de passe de l'utilisateur
                utilisateur.MotDePasse = nouveauMotDePasse;
                _context.Update(utilisateur);
                await _context.SaveChangesAsync();

                // Envoyer le nouveau mot de passe via un service SMS
                var message = $"Votre nouveau mot de passe est : {nouveauMotDePasse}. N'oublie pas de le changer en vous connectant sur le site.";
                await _smsSenderService.SendSmsAsync("+1" + numero, message);

                ViewBag.Message = "Un nouveau mot de passe a été envoyé à votre numéro de téléphone.";
                return RedirectToAction("Index", "Chambres");
            }
            else
            {
                ViewBag.ErrorMessage = "Le numéro n'existe pas.";
                return View("demandeNumero");
            }
        }

        public async Task<IActionResult> confirmerEmail(string numero)
        {
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Numero == numero);
            if (utilisateur != null)
            {
                // Générer un code aléatoire à 6 chiffres
                var code = new Random().Next(100000, 999999).ToString();

                // Enregistrer ce code dans une base de données temporaire ou dans la session
                HttpContext.Session.SetString("CodeVerification", code);

                // Envoyer le code via un service SMS (par exemple, Twilio)
                var message = $"Votre code de vérification est : {code}";
                await _smsSenderService.SendSmsAsync("+1" + numero, message);

                // Passer le code généré et le numéro à la vue
                ViewBag.CodeConfirmation = code;
                ViewBag.Numero = numero;

                return View(utilisateur);  // Exemple de redirection
            }
            else
            {
                ViewBag.ErrorMessage = "Le numéro n'existe pas.";
                return View("demandeNumero");
            }
        }

        // GET: Utilisateurs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Utilisateurs.ToListAsync());
        }

        // Action pour la page de connexion
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.Email == email);

                if (utilisateur != null)
                {
                    HttpContext.Session.SetInt32("UtilisateurId", utilisateur.UtilisateurId);
                    HttpContext.Session.SetString("Nom", utilisateur.Nom);
                    HttpContext.Session.SetString("Prenom", utilisateur.Prenom);
                    HttpContext.Session.SetString("Email", utilisateur.Email);
                    HttpContext.Session.SetInt32("EstAdmin", utilisateur.EstAdmin ? 1 : 0);

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, utilisateur.Prenom),
                new Claim(ClaimTypes.Role, utilisateur.EstAdmin ? "Administrateur" : "Utilisateurs"),
                new Claim(ClaimTypes.MobilePhone, utilisateur.Numero)

            };

                    var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                    await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                    return utilisateur.EstAdmin
                        ? RedirectToAction("Index", "Admins")
                        : RedirectToAction("Index", "Chambres");
                }

                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            }

            return View();
        }


        public async Task<IActionResult> Logout()
        {
            // Pour les sessions
            HttpContext.Session.Clear();


            return RedirectToAction("Index", "Chambres");
        }


        // GET: Utilisateurs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(m => m.UtilisateurId == id);
            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        // GET: Utilisateurs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Utilisateurs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UtilisateurId,Nom,Prenom,Email,Numero,MotDePasse,EstAdmin")] Utilisateur utilisateur)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'email existe déjà
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == utilisateur.Email))
                {
                    ModelState.AddModelError("", "Cet email est déjà utilisé.");
                    return View(utilisateur);
                }

                // Vérifier si le numéro de téléphone existe déjà
                if (await _context.Utilisateurs.AnyAsync(u => u.Numero == utilisateur.Numero))
                {
                    ModelState.AddModelError("", "Ce numéro de téléphone est déjà utilisé.");
                    return View(utilisateur);
                }

                _context.Add(utilisateur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "Chambres");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmationCode(string nom, string prenom, string email, string numero, string password, string confirmPassword)
        {
            if (password == confirmPassword)
            {
                var utilisateur = new Utilisateur
                {
                    Nom = nom,
                    Prenom = prenom,
                    Email = email,
                    Numero = numero,
                    MotDePasse = password,
                    EstAdmin = false
                };
                // Générer un code aléatoire à 6 chiffres
                var code = new Random().Next(100000, 999999).ToString();

                // Enregistrer ce code dans une base de données temporaire ou dans la session
                HttpContext.Session.SetString("CodeVerification", code);

                // Envoyer le code via un service SMS (par exemple, Twilio)
                var message = $"Votre code de vérification est : {code}";
                await _smsSenderService.SendSmsAsync("+1" + numero, message);

                // Passer le code généré à la vue
                ViewBag.CodeConfirmation = code;

                return View(utilisateur);  // Exemple de redirection
            }

            // Si le modèle est invalide, revenir à la page d'inscription avec les erreurs
            return RedirectToAction("Register", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUt(string nom, string prenom, string email, string password, string numero, string confirmPassword)
        {
            // Vérifier si les mots de passe correspondent
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Le mot de passe et sa confirmation doivent correspondre.");
                return View();
            }

            // Vérifier si l'email existe déjà
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Cet email est déjà utilisé.");
                return View();
            }

            // Vérifier si le numéro de téléphone existe déjà
            if (await _context.Utilisateurs.AnyAsync(u => u.Numero == numero))
            {
                ModelState.AddModelError("", "Ce numéro de téléphone est déjà utilisé.");
                return View();
            }

            // Créer un nouvel utilisateur
            var utilisateur = new Utilisateur
            {
                Nom = nom,
                Prenom = prenom,
                Email = email,
                MotDePasse = password,
                Numero = numero,
                EstAdmin = false // Toujours définir à faux
            };

            // Valider les données avec ModelState
            if (!TryValidateModel(utilisateur))
            {
                return View();
            }

            // Ajouter l'utilisateur à la base de données
            _context.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Rediriger vers une action après succès
            return RedirectToAction("Index", "Chambres");
        }


        // GET: Utilisateurs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            return View(utilisateur);
        }

        public async Task<IActionResult> EditUt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound();
            }
            return View(utilisateur);
        }

        // POST: Utilisateurs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UtilisateurId,Nom,Prenom,Email,Numero,MotDePasse,EstAdmin")] Utilisateur utilisateur)
        {
            if (id != utilisateur.UtilisateurId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(utilisateur);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilisateurExists(utilisateur.UtilisateurId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var ut = await _context.Utilisateurs.FindAsync(id);
                if(ut.EstAdmin == true)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Index", "Chambres");
                }
                
            }
            return View(utilisateur);
        }



        // GET: Utilisateurs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(m => m.UtilisateurId == id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }



        // POST: Utilisateurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur != null)
            {
                _context.Utilisateurs.Remove(utilisateur);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Utilisateurs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUtConfirmed(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur != null)
            {
                _context.Utilisateurs.Remove(utilisateur);
                HttpContext.Session.Clear();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Chambres");

        }

        private bool UtilisateurExists(int id)
        {
            return _context.Utilisateurs.Any(e => e.UtilisateurId == id);
        }
    }

    
    

}
