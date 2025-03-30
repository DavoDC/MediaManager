# Things to Do 

## Analyser
Get from AudioManager
Add analyser, do each field
DO THIS NEXT TO GET LISTS OF VARIANTS

## IMPROVING PARSING
- You'll see incorrect stuff come up
    - e.g. EAC being assigned to quality title instead of audio codec 
- Use analysis results to come up with custom fixes
- Find out if can add to regex to specifically not assign EAC to other groups

### One approach
Define list of audio codecs
If in list , add audio codec prefix like edition uses
Match based on prefix not value
Guaranteed match!

## Improve Folder parsing - InitialiseFieldsUsingMediaFolderName()
Code is almost the same for movies and episodes when parsing folder names
Generalise it!! Use one regex for both, shift into MediaFile class

## Look for duplicate code 
Init of common fields in movie and ep