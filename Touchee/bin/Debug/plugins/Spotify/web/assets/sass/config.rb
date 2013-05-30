# Delineate the directory for our SASS/SCSS files (this directory)
sass_path = File.dirname(__FILE__)
 
# Delineate the CSS directory (under resources/stylesheets in this demo)
css_path = File.join(sass_path, '../stylesheets')
 
# Delinate the images directory
images_path = File.join(sass_path, '../images')

# Syntax used
preferred_syntax = :sass

# Specify the output style/environment
# output_style = :compressed
# environment = :development