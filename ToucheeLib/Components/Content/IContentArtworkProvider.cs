using System.Drawing;
using Touchee.Artwork;

namespace Touchee.Components.Content {

    public interface IContentArtworkProvider : IComponent {
        ArtworkStatus GetArtwork(IContainer container, IItem item, out Image artwork);
        ArtworkStatus GetArtwork(IContainer container, Options filter, out Image artwork);
    }

}
