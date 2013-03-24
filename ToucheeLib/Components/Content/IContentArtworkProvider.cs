using System.Drawing;
using Touchee.Artwork;

namespace Touchee.Components.Content {

    public interface IArtworkProvider : IComponent {
        Image GetArtwork(Container container, Options filter);
    }

}
