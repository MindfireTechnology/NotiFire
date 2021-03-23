using System;
using System.Collections.Generic;
using System.Text;

namespace NotifyGenerator.Tests.Concept
{
	public class GenericBasedMapper
	{
		public TOut MapTo<TOut>(NotifyGenerator.Tests.Concept.Entity.Order value) where TOut : class, new()
		{
			// All of the mappings with `NotifyGenerator.Tests.Concept.Entity` as input,
			// which output should we choose?

			if (typeof(TOut) == typeof(NotifyGenerator.Tests.Concept.ViewModel.Order))
			{
				// Map this sucka!
				return new TOut();
			}

			return default(TOut);
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
