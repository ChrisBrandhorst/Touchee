define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/genres',
  './../models/view_models/grouped_tracks',
  'views/contents/common_table',
  'views/contents/common_split',
  'views/contents/common_split_details',
  'views/contents/common_grouped_table',
  'text!./_genre_header.html'
], function($, _, Backbone, Genres, GroupedTracks, CommonTableView, CommonSplitView, CommonSplitDetailsView, CommonGroupedTableView, genreHeaderTemplate) {
  

  var GenresView = CommonSplitView.extend({
    
    // SplitView options
    className:    'genres',
    viewModel:    Genres,
    
    // View for the left list
    listView:     CommonTableView.extend({
      className:    'genres',
      index:        'genreSort',
      selection:    { keep:true },
      columns:      ['genre$']
    }),

    // Combined view for the right details
    detailView:   CommonSplitDetailsView.extend({
      className:    'genre',
      viewModel:    GroupedTracks.extend({ groupByAttr:'genre' }),
      header:       _.template(genreHeaderTemplate),

      // The actual detail list
      contentView:  CommonGroupedTableView.extend({
        className:    'tracks',
        quickscroll:  true,
        index:        function(item) { return item.get('album$') + " - " + item.get('albumArtist$'); },
        columns:      ['trackNumber', 'title', 'duration$'],
        artworkSize:  250
      })
      
    })
    
  });
  
  
  return GenresView;
  
});