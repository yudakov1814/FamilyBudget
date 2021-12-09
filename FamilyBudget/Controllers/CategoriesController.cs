using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyBudget.Data;
using FamilyBudget.Models;
using FamilyBudget.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FamilyBudget.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IdentityUser user { get { return CurrentUser(); } }

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var all_categories = await _context.Categories.Include(c => c.Project).ToListAsync();
            var viewable_categories = all_categories.Where(x => user.CanView(x, _context)).ToList();

            return View(viewable_categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (!user.CanView(category, _context))
            {
                return Forbid();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            var all_projects = _context.Projects.Include(c => c.Owner).ToList();
            var available_projects = all_projects.Where(c => user.CanEdit(c,_context)).ToList();

            if (!available_projects.Any())
            {
                return Forbid();
            }

            ViewData["ProjectId"] = new SelectList(available_projects, "Id", "Name");
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ProjectId,CreateTime,UpdateTime")] Category category)
        {
            if (!user.CanEdit(category, _context))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                category.CreateTime = DateTime.Now;
                category.UpdateTime = DateTime.Now;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", category.ProjectId);

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(category, _context))
            {
                return Forbid();
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", category.ProjectId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ProjectId,UpdateTime")] Category category)
        {
            if (!user.CanEdit(category, _context))
            {
                return Forbid();
            }

            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.UpdateTime = DateTime.Now;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", category.ProjectId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(category, _context))
            {
                return Forbid();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private IdentityUser CurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            return _context.Users
                .FirstOrDefault(m => m.UserName == username);
        }
    }
}
