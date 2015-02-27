
namespace po.fwdr.contract.Markups
{
	public abstract class BaseMarkupContract
	{
		const string CommonDefaultValueString = "default";
		const string ValidatingCarrierDefaultValueString = "YY";

		public BaseMarkupContract(
			string validatingCarrier,
			string classOfService,
			decimal markupFixValue,
			decimal markupRateValue
		)
		{
			ValidatingCarrier = validatingCarrier;
			ClassOfService = classOfService;
			MarkupFixValue = markupFixValue;
			MarkupRateValue = markupRateValue;
		}

		public readonly string ValidatingCarrier;

		public readonly string ClassOfService;

		public readonly decimal MarkupFixValue;

		public readonly decimal MarkupRateValue;

		public bool IsDefault
		{
			get
			{
				return ValidatingCarrier == ValidatingCarrierDefaultValueString
					&& ClassOfService == CommonDefaultValueString;
			}
		}
	}
}
