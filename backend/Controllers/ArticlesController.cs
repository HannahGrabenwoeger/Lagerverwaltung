using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    // Beispiel-Artikeldaten
    private static readonly List<Article> Articles = new List<Article>
    {
        new Article { Id = 1, Name = "Laptop", Quantity = 15, Location = "Lager A" },
        new Article { Id = 2, Name = "Maus", Quantity = 30, Location = "Lager B" },
        new Article { Id = 3, Name = "Tastatur", Quantity = 20, Location = "Lager C" }
    };

    // GET: /api/articles
    [HttpGet]
    public IActionResult GetArticles()
    {
        return Ok(Articles);
    }

    // GET: /api/articles/{id}
    [HttpGet("{id}")]
    public IActionResult GetArticleById(int id)
    {
        var article = Articles.FirstOrDefault(a => a.Id == id);
        if (article == null)
        {
            return NotFound(new { message = "Artikel nicht gefunden" });
        }
        return Ok(article);
    }

    // POST: /api/articles
    [HttpPost]
    public IActionResult AddArticle([FromBody] Article newArticle)
    {
        if (newArticle == null || string.IsNullOrEmpty(newArticle.Name))
        {
            return BadRequest(new { message = "Ungültige Artikeldaten" });
        }

        newArticle.Id = Articles.Max(a => a.Id) + 1; // Neue ID vergeben
        Articles.Add(newArticle);
        return CreatedAtAction(nameof(GetArticleById), new { id = newArticle.Id }, newArticle);
    }

    // DELETE: /api/articles/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteArticle(int id)
    {
        var article = Articles.FirstOrDefault(a => a.Id == id);
        if (article == null)
        {
            return NotFound(new { message = "Artikel nicht gefunden" });
        }

        Articles.Remove(article);
        return NoContent();
    }
}

// Artikel-Datenmodell
public class Article
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Standardwert
    public int Quantity { get; set; }
    public string Location { get; set; } = string.Empty; // Standardwert
}