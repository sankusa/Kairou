using System.Collections.Generic;

namespace Kairou
{
    public interface IValidatableAsCommandField
    {
        IEnumerable<string> Validate(Command command, string fieldName);
    }
}