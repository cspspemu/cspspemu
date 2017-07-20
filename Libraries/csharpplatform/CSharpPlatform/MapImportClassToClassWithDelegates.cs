namespace CSharpPlatform
{
    //internal class MapImportClassToClassWithDelegates
    //{
    //	static public void Map(Type TypeWithImports, Type TypeWithDelegates)
    //	{
    //		//Console.WriteLine("Binding!");
    //		var DelegateFields = TypeWithDelegates.GetFields(BindingFlags.Static | BindingFlags.Public).CreateDictionary(Field => Field.Name);
    //		var ImportMethods = TypeWithImports.GetMethods(BindingFlags.Static | BindingFlags.Public);
    //		//Console.WriteLine(TypeWithDelegates.GetFields(BindingFlags.Static | BindingFlags.Public).Count());
    //		//Console.WriteLine(ImportMethods.Count());
    //		foreach (var ImportMethod in ImportMethods)
    //		{
    //			if (DelegateFields.ContainsKey(ImportMethod.Name))
    //			{
    //				var DelegateField = DelegateFields[ImportMethod.Name];
    //				//Console.WriteLine("{0} -> {1}", ImportMethod, DelegateField);
    //				DelegateField.SetValue(null, ImportMethod.CreateDelegate(DelegateField.FieldType));
    //			}
    //		}
    //
    //		Marshal.GetDelegateForFunctionPointer(
    //	}
    //}
}