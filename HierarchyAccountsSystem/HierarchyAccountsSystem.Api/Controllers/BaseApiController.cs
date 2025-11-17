using Microsoft.AspNetCore.Mvc;

namespace HierarchyAccountsSystem.Api.Controllers {
  [Route("api/v{version:apiVersion}/[controller]")]
  [ApiController]
  public class BaseApiController : ControllerBase {

  }
}
