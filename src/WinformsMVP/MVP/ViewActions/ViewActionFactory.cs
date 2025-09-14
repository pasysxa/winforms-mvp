namespace WinformsMVP.MVP.ViewActions
{
    public class ViewActionFactory
    {
        public static ViewAction Create(string name) => ViewAction.Create(name);
        public static QualifiedViewActionFactory WithQualifier(params string[] qualifiers)
            => new QualifiedViewActionFactory(qualifiers);
    }
}
