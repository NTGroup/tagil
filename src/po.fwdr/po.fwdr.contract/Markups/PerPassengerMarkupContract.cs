
namespace po.fwdr.contract.Markups
{
	public class PerPassengerMarkupContract : BaseMarkupContract
	{
		public PerPassengerMarkupContract(
			string validatingCarrier,
			string classOfService,
			decimal markupFixValue,
			decimal markupRateValue
		)
			: base(
				validatingCarrier,
				classOfService,
				markupFixValue,
				markupRateValue
			)
		{ }
	}
}
