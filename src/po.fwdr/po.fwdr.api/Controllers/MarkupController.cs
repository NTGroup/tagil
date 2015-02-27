using System.Threading.Tasks;
using System.Web.Http;
using po.fwdr.api.Models;
using po.fwdr.contract.Markups;

namespace po.fwdr.api.Controllers
{
	public class MarkupController : ApiController
	{
		public MarkupController()
		{
			_poService = new PoService();
		}

		[Route("markups")]
		public async Task<IHttpActionResult> GetMarkups([FromUri] string[] loc)
		{
			MarkupBundleContract result = await _poService.FindMarkupsAsync();

			return Ok(result);
		}

		private readonly PoService _poService;
	}
}
