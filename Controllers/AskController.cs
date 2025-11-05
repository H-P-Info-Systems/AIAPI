using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApi.Data;
using SmartApi.Services;

namespace SmartApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AskController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAIService _ai;

    public AskController(AppDbContext db, IAIService ai)
    {
        _db = db;
        _ai = ai;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] QueryModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        // 1. Ask AI to extract intent/keywords
        var intent = await _ai.ExtractIntent(request.Question);
        if (string.IsNullOrWhiteSpace(intent))
            intent = request.Question; // fallback: use full question

        // 2. Search DB with simple contains on name/description and an example numeric filter extraction
        var query = _db.Products.AsQueryable();

        // numeric filter example: look for "under <number>" or "below <number>"
        var lower = ExtractNumberFilter(intent);
        if (lower.HasValue)
        {
            query = query.Where(p => p.Price <= lower.Value);
        }
        else
        {
            // fallback: search keywords in Name or Description
            query = query.Where(p => p.Name.Contains(intent) || p.Description.Contains(intent));
        }

        var results = await query.Take(50).ToListAsync();

        // 3. Let AI format the answer
        var answer = await _ai.GenerateAnswer(request.Question, results);
        return Ok(new { answer, count = results.Count });
    }

    private decimal? ExtractNumberFilter(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        // simple parser: find first integer in the text
        foreach (var token in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (decimal.TryParse(token, out var v))
                return v;
            // handle currency symbols like ₹1000 or Rs.1000
            var cleaned = new string(token.Where(ch => char.IsDigit(ch) || ch=='.').ToArray());
            if (decimal.TryParse(cleaned, out v))
                return v;
        }
        return null;
    }
}

public class QueryModel
{
    public string Question { get; set; } = string.Empty;
}
