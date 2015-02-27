
namespace po.fwdr.contract.Markups
{
	public class MarkupBundleContract
	{
		public MarkupBundleContract()
		{
			PerSegments = new PerSegmentMarkupContract[0];
			PerPassenger = new PerPassengerMarkupContract[0];
		}

		public PerSegmentMarkupContract[] PerSegments;
		public PerPassengerMarkupContract[] PerPassenger;
	}
}
