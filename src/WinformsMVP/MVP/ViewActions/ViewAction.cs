using System;

namespace WinformsMVP.MVP.ViewActions
{
    public class ViewAction : IEquatable<ViewAction>
    {
        public string Name { get; }
        private ViewAction(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        public static ViewAction Create(string name) => new ViewAction(name);
        public static QualifiedViewActionFactory Factory(params string[] qualifiers) => new QualifiedViewActionFactory(qualifiers);

        public ViewAction WithSuffix(string suffix, string sep = ".")
            => new ViewAction($"{Name}{sep}{suffix}");

        public bool Equals(ViewAction other)
            => other != null &&
               Name == other.Name;
        public override bool Equals(object obj)
            => Equals(obj as ViewAction);

        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
