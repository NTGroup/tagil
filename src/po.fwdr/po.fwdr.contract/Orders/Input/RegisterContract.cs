using System;

namespace po.fwdr.contract.Orders.Input
{
	public class RegisterContract
	{
		public RegisterContract(
			string tenantId,
			string nqtId,
			string nqtObject,
			string pnrState,
			DateTime? sysTimeLimit = null,
			decimal? totalAmount = null,
			decimal? totalMarkup = null,
			string pnrId = null
		)
		{
			TenantId = tenantId;
			NqtId = nqtId;
			NqtObject = nqtObject;
			SysTimeLimit = sysTimeLimit;
			TotalAmount = totalAmount;
			TotalMarkup = totalMarkup;
			PnrId = pnrId;
			PnrState = pnrState;
		}

		public readonly string TenantId;
		public readonly string NqtId;
		public readonly string NqtObject;
		public readonly DateTime? SysTimeLimit;
		public readonly decimal? TotalAmount;
		public readonly decimal? TotalMarkup;
		public readonly string PnrId;
		public readonly string PnrState;
	}
}
