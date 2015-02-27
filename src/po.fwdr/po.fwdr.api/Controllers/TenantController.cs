using System.Threading.Tasks;
using System.Web.Http;
using po.fwdr.api.Models;

namespace po.fwdr.api.Controllers
{
	public class TenantController : ApiController
	{
		public TenantController()
		{
			_poService = new PoService();
		}

		[Route("tenants")]
		public async Task<IHttpActionResult> GetTenants([FromUri] string email)
		{
			if (string.IsNullOrEmpty(email))
				return BadRequest();

			var res = await _poService.GetTenant(email);

			return Ok(res);
		}

		private readonly PoService _poService;
	}
}
