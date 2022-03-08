using SharpWebServer.Interfaces;

namespace SharpWebServer.Models;

public abstract class Controller : IController
{
    public virtual IActionResult Ok(object? content = null) => new ActionResult(200, content);
    public virtual IActionResult NoContent(object? content = null) => new ActionResult(204, content);
    public virtual IActionResult PartialContent(object? content = null) => new ActionResult(206, content);
    public virtual IActionResult MovedPermanently(object? content = null) => new ActionResult(301, content);
    public virtual IActionResult Found(object? content = null) => new ActionResult(302, content);
    public virtual IActionResult SeeOther(object? content = null) => new ActionResult(303, content);
    public virtual IActionResult NotModified(object? content = null) => new ActionResult(304, content);
    public virtual IActionResult TemporaryRedirect(object? content = null) => new ActionResult(307, content);
    public virtual IActionResult BadRequest(object? content = null) => new ActionResult(400, content);
    public virtual IActionResult Unauthorized(object? content = null) => new ActionResult(401, content);
    public virtual IActionResult PaymentRequired(object? content = null) => new ActionResult(402, content);
    public virtual IActionResult Forbidden(object? content = null) => new ActionResult(403, content);
    public virtual IActionResult NotFound(object? content = null) => new ActionResult(404, content);
    public virtual IActionResult InternalServerError(object? content = null) => new ActionResult(500, content);
    public virtual IActionResult NotImplemented(object? content = null) => new ActionResult(501, content);
    public virtual IActionResult ServiceUnavailable(object? content = null) => new ActionResult(503, content);
}