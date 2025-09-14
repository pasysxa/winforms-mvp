using System;
using System.Collections.Generic;
using System.Linq;

namespace WinformsMVP.MVP.ViewActions
{
    public class QualifiedViewActionFactory
    {
        private readonly string[] _qualifiers;

        internal QualifiedViewActionFactory(IEnumerable<string> qualifiers)
        {
            _qualifiers = qualifiers.ToArray();
        }

        public ViewAction Create(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            var qualifiers = _qualifiers.Length > 0 ? string.Join(".", _qualifiers) + "." : string.Empty;
            if (string.IsNullOrEmpty(qualifiers))
            {
                return ViewAction.Create(name);
            }
            return ViewAction.Create($"{qualifiers}.{name}");
        }
    }
}
