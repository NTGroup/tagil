
namespace po.fwdr.contract.Markups
{
	public class PerSegmentMarkupContract : BaseMarkupContract
	{
		public PerSegmentMarkupContract(
			string validatingCarrier,
			string classOfService,
			decimal markupFixValue,
			int minLimit,
			int maxLimit
		)
			: base(
				validatingCarrier,
				classOfService,
				markupFixValue,
				0
			)
		{
			MinLimit = minLimit;
			MaxLimit = maxLimit;
		}

		public readonly int MinLimit;
		public readonly int MaxLimit;
	}
}
