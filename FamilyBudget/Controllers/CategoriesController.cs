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
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
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
                var project = _context.Projects.Find(category.ProjectId);
                project.UpdateTime = DateTime.Now;
                _context.Update(project);

                category.CreateTime = DateTime.Now;
                category.UpdateTime = DateTime.Now;
                category.Name = category.Name.ToLower();
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", "Home", new { id = category.ProjectId });
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
                    var project = _context.Projects.Find(category.ProjectId);
                    project.UpdateTime = DateTime.Now;
                    _context.Update(project);

                    category.UpdateTime = DateTime.Now;
                    category.Name = category.Name.ToLower();
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
                return RedirectToAction("Edit", "Home", new { id = category.ProjectId });
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

            if (!user.CanDelete(category, _context))
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

            var project = await _context.Projects.FindAsync(category.ProjectId);
            project.UpdateTime = DateTime.Now;
            _context.Update(project);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Home", new { id = category.ProjectId });
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

        public async Task<IActionResult> SearchCategories(int id, string term)
        {
            try
            {
                var categories = await _context.Categories
                    .Where(a => a.ProjectId == id)
                    .Where(a => a.Name.Contains(term.ToLower()))
                    .Select(a => new { value = a.Name, id = a.Id })
                    .ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> CategoryUnique(int projectId, string categoryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    return Ok(-1);
                }

                var category = _context.Categories.Where(x => x.ProjectId == projectId)
                .FirstOrDefault(e => e.Name == categoryName);
                if (category != null)
                {
                    return Ok(category.Id);
                }

                category = new Category
                {
                    Name = categoryName,
                    ProjectId = projectId
                };
                await Create(category);
                return Ok(category.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
