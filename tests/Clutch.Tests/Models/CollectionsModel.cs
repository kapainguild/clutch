using System.Collections.Generic;

namespace Clutch.Tests.Models
{
    interface ICollectionModel
    {
        IList<string> StringList { get; }

        string Str { get; set; }
    }
}
