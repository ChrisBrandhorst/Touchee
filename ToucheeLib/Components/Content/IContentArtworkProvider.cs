using System.Drawing;
using Touchee.Artwork;

namespace Touchee.Components.Content {

    public interface IArtworkProvider : IComponent {
        Image GetArtwork(IContainer container, Options filter);
    }

}
