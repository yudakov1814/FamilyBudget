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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FamilyBudget.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private IdentityUser user { get { return CurrentUser(); } }

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var all_projects = await _context.Projects.Include(p => p.Owner).ToListAsync();
            var viewable_projects = all_projects.Where(x => user.CanView(x, _context)).ToList();

            return View(viewable_projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            if (!user.CanView(project, _context))
            {
                return Forbid();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["OwnerId"] = new SelectList(_context.Users.Where(c => c.Id == user.Id), "Id", "Id");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,OwnerId,CreateTime,UpdateTime")] Project project)
        {
            if (!user.CanEdit(project, _context))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                project.CreateTime = DateTime.Now;
                project.UpdateTime = DateTime.Now;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["OwnerId"] = new SelectList(_context.Users.Where(c => c.Id == user.Id), "Id", "Id", project.OwnerId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            if (!user.CanEdit(project, _context))
            {
                return Forbid();
            }

            ViewData["OwnerId"] = new SelectList(_context.Users.Where(c => c.Id == user.Id), "Id", "Id", project.OwnerId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OwnerId,UpdateTime")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.UpdateTime = DateTime.Now;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            ViewData["OwnerId"] = new SelectList(_context.Users, "Id", "Id", project.OwnerId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            if (!user.CanDelete(project, _context))
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
        private IdentityUser CurrentUser()
        {
            var username = HttpContext.User.Identity.Name;
            return _context.Users
                .FirstOrDefault(m => m.UserName == username);
        }
    }
}
