using System.Collections.Generic;

namespace Touchee.Components.Content {

    public interface IContentProvider : IComponent {
        bool ProvidesFrontend { get; }
        object GetContents(Container container, Options filter);
        IEnumerable<IItem> GetItems(Container container, Options filter);
    }

}
