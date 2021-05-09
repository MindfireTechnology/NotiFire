namespace NotifyGenerator.Tests.Concept
{
	public class DirectMapper
	{
		public ViewModel.Order ToOrder(Entity.Order value)
		{
			return new ViewModel.Order { };
		}
	}
}
