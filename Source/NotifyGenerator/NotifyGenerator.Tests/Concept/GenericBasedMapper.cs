namespace NotifyGenerator.Tests.Concept
{
	public class GenericBasedMapper
	{
		public TOut MapTo<TOut>(Entity.Order value) where TOut : class, new()
		{
			// All of the mappings with `NotifyGenerator.Tests.Concept.Entity` as input,
			// which output should we choose?

			if (typeof(TOut) == typeof(NotifyGenerator.Tests.Concept.ViewModel.Order))
			{
				// Map this sucka!
				return new TOut();
			}

			return default;
		}
	}
}

namespace NotifyGenerator.Tests.Concept.Entity
{
}

namespace NotifyGenerator.Tests.Concept.ViewModel
{
}

namespace NotifyGenerator.Tests.Concept.Model
{
}
