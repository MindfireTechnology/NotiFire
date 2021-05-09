namespace NotifyGenerator.Tests.Concept.Entity
{
	[MapClassTo(typeof(ViewModel.Order), MethodName = "ToOrderVM", Reverse = true)]
	public class Order
	{
		public int Id { get; set; }
		public decimal Total { get; set; }
		public string CustomerName { get; set; }
	}
}
