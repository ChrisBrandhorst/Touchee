using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using Touchee.Artwork;

namespace Touchee {

    public interface IContentsPlugin {
        bool CustomFrontend { get; }
        IEnumerable<IItem> GetItems(IContainer container, Options filter);
        Contents GetContents(IContainer container, Options filter);
        ArtworkStatus GetArtwork(IContainer container, IItem item, out Image artwork);
        ArtworkStatus GetArtwork(IContainer container, Options filter, out Image artwork);
    }

}
