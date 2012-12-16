using System.Collections.Generic;

namespace Touchee.Components.Content {

    public interface IContentProvider : IComponent {
        bool ProvidesFrontend { get; }
        object GetContents(IContainer container, Options filter);
        IEnumerable<IItem> GetItems(IContainer container, Options filter);
    }

}
