define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/artists',
  './../models/view_models/grouped_tracks',
  'views/contents/common_table',
  'views/contents/common_split',
  'views/contents/common_split_details',
  'views/contents/common_grouped_table',
  'text!./_artist_header.html'
], function($, _, Backbone, Artists, GroupedTracks, CommonTableView, CommonSplitView, CommonSplitDetailsView, CommonGroupedTableView, artistHeaderTemplate) {
  

  var ArtistsView = CommonSplitView.extend({
    
    // SplitView options
    className:    'artists',
    viewModel:    Artists,
    
    // View for the left list
    listView:     CommonTableView.extend({
      className:    'artists',
      index:        'artistSort',
      selection:    { keep:true },
      columns:      ['artist$']
    }),

    // Combined view for the right details
    detailView:   CommonSplitDetailsView.extend({
      className:    'artist',
      viewModel:    GroupedTracks.extend({ groupByAttr:'artist' }),
      header:       _.template(artistHeaderTemplate),

      // The actual detail list
      contentView:  CommonGroupedTableView.extend({
        className:    'tracks',
        quickscroll:  true,
        index:        function(item) { return item.get('album$'); },
        columns:      ['trackNumber', 'title', 'duration$'],
        artworkSize:  250
      })
      
    })
    
  });
  
  
  return ArtistsView;
  
});