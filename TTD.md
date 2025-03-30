# Things to Do 


## IMPROVING PARSING

- You'll see incorrect stuff come up
    - e.g. EAC being assigned to quality title instead of audio codec 
    - Not picking video codec - white lotus, fawtly  , h264, h265
- Use analysis results to come up with custom fixes
- Find out if can add to regex to specifically not assign EAC to other groups

### One approach
Define list of audio codecs
If in list , add audio codec prefix like edition uses
Match based on prefix not value
Guaranteed match!

add to dynamicrange: DV,HDR10+,HDR10P,PQ,HDR10,DVHDR10+,DVHDR10P,DVHDR

add HEVC to vid codex regex also another term for x265

 dynamicrange:
DV HDR10+
DV HDR10
DV
HDR10+
DV HLG
DV SDR
HDR10
HDR
HDR Undefined
HLG
PQ

## Combine stats
see playHD and PlayHD separate. possible to combine the count of those?
Erai-raws and Erai-raw



## Refactor: Improve Folder parsing - InitialiseFieldsUsingMediaFolderName()
Code is almost the same for movies and episodes when parsing folder names
Generalise it!! Use one regex for both, shift into MediaFile class

## Refactor: Look for duplicate code 
Init of common fields in movie and ep