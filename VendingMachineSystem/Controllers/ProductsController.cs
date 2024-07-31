using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using static VendingMachineContext;

//[Authorize(Roles = "Admin, User")]

public class ProductsController : Controller
{
    private readonly VendingMachineContext _context;
   
    private const int PageSize = 10; // Number of items per page

    public ProductsController(VendingMachineContext context)
    {
        _context = context;
    }



    //public async Task<IActionResult> Index()
    //{
    //    return View(await _context.Products.ToListAsync());
    //}

    public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)

    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
        ViewData["CurrentFilter"] = searchString;

        var products = from p in _context.Products
                       select p;

        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.Name.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                products = products.OrderByDescending(p => p.Name);
                break;
            case "Price":
                products = products.OrderBy(p => p.Price);
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.Price);
                break;
            default:
                products = products.OrderBy(p => p.Name);
                break;
        }

        int pageSize = PageSize;
        int currentPageNumber = pageNumber ?? 1;

        return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), currentPageNumber, pageSize));
    }




    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.ID == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([Bind("ID,Name,Price,QuantityAvailable")] Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Price,QuantityAvailable")] Product product)
    {
        if (id != product.ID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ID))
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
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(m => m.ID == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.ID == id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Purchase(int id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null || product.QuantityAvailable < quantity)
        {
            return NotFound();
        }

        product.QuantityAvailable -= quantity;

        var transaction = new Transaction
        {
            ProductID = product.ID,
            UserID = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value,
            PurchaseDate = DateTime.Now,
            QuantityPurchased = quantity
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
