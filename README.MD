FireMap
=======

FireMap is a Source Generator that creates mapping code at compile-time. Using them is really easy. For a given type you can use either a `MapClassTo` attribute or a `MapClassFrom` attribute like this:

```cs

	[MapClassTo(typeof(SimpleUserEntity))]
	public class SimpleUserDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string ZipCode { get; set; }
		public string Phone { get; set; }
	}

	// or

	[MapClassFrom(typeof(SimpleUserDTO))]
	public class SimpleUserEntity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string ZipCode { get; set; }
		public string Phone { get; set; }
	}

```
When these attributes are applied to one or more classes, a new generated source file `Mapper.cs` and `IMapper.cs` are added to the project under the `FireMap` namespace. It can therefore be used like this:

```cs
	IMapper mapper = new Mapper(); // Note: this could come from DI
	var simpleUserEntityObject = mapper.ToSimpleUserEntity(value);
```

And that would allow you to easily map between objects based on the convention `To{Type}`. Because you have both a `MapClassTo` or `MapClassFrom` attribute, you only really need to be able to add this attribute to one side of the mapping.

You can also change the name of this method as another parameter on the `MapClassTo` or `MapClassFrom` attributes. This is pretty much required if you have the object name exactly the same but just in different namespaces. For example:

```cs
	[MapClassTo(typeof(Entities.Order), MethodName = "ToOrderEntity", Reverse = true, ReverseMethodName = "ToOrderModel")]
	public class Order
	{
		public int Id { get; set; }
		public decimal Total { get; set; }
		public string CustomerName { get; set; }
	}
```
In the example above, the To/From object are both named `Order`. We therefore need to include part of that namespace. To prevent multiple `ToOrder` methods from being generated and causing confusion, you can simply state what you wish the mapping method to be named. This also works with the `MapClassFrom` attribute.

== Reverse Calls ==
Mappings between two models can be made by using the `Reverse` property on either of the `MapClassTo` or `MapClassFrom` attributes.

```cs

	[MapClassTo(typeof(SimpleUserEntity), Reverse = true)]
	public class SimpleUserDTO
	...

```

== Custom Mapping Fields ==

If all of the names match exactly and all of the types align, then there isn't much to do, but if the names are different then you can accomodate with the `MapMemberTo` attribute

```cs

[MapClassTo(typeof(SimpleUserEntity))]
	public class DifferingNamesDTO
	{
		[MapMemberTo(typeof(SimpleUserEntity), Name = "Id")]
		public int DifferingNamesId { get; set; }

		[MapMemberTo(typeof(SimpleUserEntity), Name = "Name")]
		public string FullName { get; set; }

		...
	}

```

You can apply as many attributes to each class or each field as needed.

== Mapping Different Types / Overriding Mapping ==
The goal of this initial release is to keep it pretty simple. Therefore, mapping between different types or a many to one type scenerios aren't supported. 

The way to add these kinds of features is to override the mapping method implimentation in the Mapper class. As the methods are all declaired `virtual`, you can override any one of the methods.

Example Output:
```cs

namespace FireMap
{
	public class Mapper : IMapper
	{
		public virtual Tester.Data.Entities.Order ToOrderEntity(Tester.Models.Order source) => new Tester.Data.Entities.Order
		{
			Id = source.Id,
			Total = source.Total,
			CustomerName = source.CustomerName
		};

		public virtual Tester.Models.Order ToOrderModel(Tester.Data.Entities.Order source) => new Tester.Models.Order
		{
			Id = source.Id,
			Total = source.Total,
			CustomerName = source.CustomerName
		};

		public virtual Tester.ViewModels.Order ToOrderVM(Tester.Models.Order source) => new Tester.ViewModels.Order
		{
			Id = source.Id,
			Total = source.Total,
			CustomerName = source.CustomerName
		};

```

Please feel free to open any issues, fork the code, contribute new features, etc. We're going to try to be great maintainers of this code.
