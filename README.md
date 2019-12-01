# Create IMDB NFO Files

Create movie .nfo files with IMDB URLs to fix movies not showing up correctly in Kodi.

The directory name is used to determine the movie title and year to search IMDB via HTML screen-scraping. If IMDB find a match then a .nfo file is created, or replaced, containing the IMDB URL for that movie.
Kodi will be able to read these .nfo files and use the specified URLs to load movie information from IMDB without having to perform its own movie search.

CLI usage: `dotenet CreateImdbNfoFiles.dll <BaseDiectory>`

The base directory should contain one or more movie subdirectories that follow this naming format: `Movie Name (YYYY)`

There are a few hardcoded values that I would normally put in a config file, such as URLs and file extensions, but I just needed a quick solution.
The source code is all here if you need to make changes for your own use.
