using System.Threading.Tasks;
using System.Web.Http;
using po.fwdr.api.Models;
using po.fwdr.contract.Orders.Input;

namespace po.fwdr.api.Controllers
{
	[RoutePrefix("register")]
	public class RegisterController : ApiController
	{
		public RegisterController()
		{
			_poService = new PoService();
		}

		[Route("air")]
		public async Task<IHttpActionResult> PostAirRegisterAsync([FromBody] RegisterContract registerContract)
		{
			if (registerContract == null)
				return BadRequest();

			await _poService.RegisterAsync(
				registerContract.TenantId,
				registerContract.NqtId,
				registerContract.NqtObject,
				registerContract.PnrState,
				registerContract.SysTimeLimit,
				registerContract.TotalAmount,
				registerContract.TotalMarkup,
				registerContract.PnrId
			);

			return Ok();
		}

		[Route("pay/{id}")]
		public async Task<IHttpActionResult> PutPayAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
				return BadRequest();

			await _poService.PayAsync(id);

			return Ok();
		}

		[Route("manual/{id}")]
		public async Task<IHttpActionResult> PutManualAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
				return BadRequest();

			await _poService.ManualAsync(id);

			return Ok();
		}

		[Route("statuses")]
		public async Task<IHttpActionResult> GetPnrStatuses([FromUri] string[] id)
		{
			var res = await _poService.ListPnrStatuses(id ?? new string[0]);

			return Ok(res);
		}

		[Route("commission/{id}")]
		public async Task<IHttpActionResult> GetCommission(string id)
		{
			if (string.IsNullOrEmpty(id))
				return BadRequest();

			var res = await _poService.GetCommission(id);

			return Ok(res);
		}

		private readonly PoService _poService;
	}
}
