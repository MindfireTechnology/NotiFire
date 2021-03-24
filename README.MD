NotiFire
=======

FireMap is a Source Generator that creates automatic implementations of the INotifyPropertyChanged interface. For a given type, you can use the `Notify` attribute like this:

```cs

	[Notify]
	public partial class SimpleUserDTO
	{
		private int IdField;
		private string NameField;
		private string AddressField;
		private string CityField;
		private string ZipCodeField;
		private string PhoneField;
	}

```

Classes must be declared as `partial` for the generator to work. All fields in the class will have a property generated for them that will notify when changed. If you would like to specify specific fields to generate instead of all of them, use the `Notify` attribute on only the fields you wish to use. You can also specify the name you would like to have created for the given field using the `PropertyName` property:

```cs
	public partial class SimpleUserDTO
	{
		[Notify(PropertyName = "SimpleUserId")]
		private int IdField;

		[Notify]
		private string NameField;
	}
```

By default, the generated property names will follow this convention:
1. If the field name ends in "field", drop the "field" and use the remaining name.
2. If the field name starts with a _, drop the _ and use the remaining name.
3. If neither of the above apply, append the word "Property" to the field name.
4. If the name is null or whitespace after steps 1 or 2, use rule 3 as the name.

If there are fields you would like the generator to ignore, simply use the `Exclude` attribute on the field you wish to ignore:

```cs
	[Notify]
	public partial class SimpleUserDTO
	{
		[Exclude]
		private int Id;
		private string NameField;
		private string AddressField;
		private string CityField;
		private string ZipCodeField;
		private string PhoneField;
	}
```

Please feel free to open any issues, fork the code, contribute new features, etc. We're going to try to be great maintainers of this code.
