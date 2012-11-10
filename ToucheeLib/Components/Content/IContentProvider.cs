using System.Collections.Generic;

namespace Touchee.Components.Content {

    public interface IContentProvider : IComponent {
        bool ProvidesFrontend { get; }
        IEnumerable<IItem> GetItems(IContainer container, Options filter);
        Contents GetContent(IContainer container, Options filter);
    }

}
