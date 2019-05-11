namespace SafeILGenerator.Ast
{
    public class AstLabel
    {
        public string Name;

        protected AstLabel(string name)
        {
            Name = name;
        }

        public static AstLabel CreateLabel(string name = "<Unknown>")
        {
            return new AstLabel(name);
        }

        //static public AstLabel CreateNewLabelFromILGenerator(ILGenerator ILGenerator, string Name = "<Unknown>")
        //{
        //	return new AstLabel((ILGenerator != null) ? ILGenerator.DefineLabel() : default(Label), Name);
        //}

        public override string ToString()
        {
            return $"AstLabel({Name})";
        }
    }
}