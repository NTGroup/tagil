using System.Threading.Tasks;
using System.Web.Http;
using po.fwdr.api.Models;
using po.fwdr.contract.Locations;

namespace po.fwdr.api.Controllers
{
	public class DirectoryController : ApiController
	{
		public DirectoryController()
		{
			_poService = new PoService();
		}

		[Route("locations")]
		public async Task<IHttpActionResult> GetLocations([FromUri] string[] loc)
		{
			LocationContract[] result = await _poService.FindLocationsAsync(loc);

			return Ok(result);
		}

		[Route("geoitems")]
		public async Task<IHttpActionResult> GetGeoItems()
		{
			var res = await _poService.ListGeoItemsAsync();

			return Ok(res);
		}

		[Route("companies")]
		public async Task<IHttpActionResult> GetCompanies()
		{
			var res = await _poService.ListCompaniesAsync();

			return Ok(res);
		}

		[Route("planetypes")]
		public async Task<IHttpActionResult> GetPlaneTypes()
		{
			var res = await _poService.ListPlaneTypesAsync();

			return Ok(res);
		}

		private readonly PoService _poService;
	}
}
