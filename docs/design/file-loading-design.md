\# Film File Loading Design

Owner: Kushal C Bhagan

Card: 307



\## Purpose

Load film details from a csv file chosen by the user.



\## User input

The program asks the user for the film file.

The file name must not be hard-coded in the program.

The FileLoader receives the file path when it is created.



Example:

Enter the film file name: films\_sample.csv



\## File format

The first line is the header:

id,title,genre,year,duration\_minutes,age\_rating,description,poster\_file



Each film is stored on one line, with exactly 8 fields separated by commas.



Example line:

1,The Last Voyage,Adventure,2023,128,12,A crew sails to the edge of the map,last\_voyage.jpg



Rule: descriptions cannot contain commas. A line with an extra comma has more than 8 fields, so it is skipped.



\## Fields

1\. Id

2\. Title

3\. Genre

4\. Year

5\. Duration in minutes

6\. Age rating (0, 12, 15 or 18)

7\. Description

8\. Poster file name



\## Loading process

1\. Ask the user for the file name.

2\. Check whether the file exists.

3\. If the file does not exist, throw a FileNotFoundException. The window shows a clear error message.

4\. If the file exists, read all lines.

5\. Skip the first line (the header).

6\. Ignore empty lines (they are not counted).

7\. Split each line using commas.

8\. Check that the line has exactly 8 fields.

9\. Convert Id, Year, Duration and Age rating into numbers using TryParse.

10\. Create a Film object.

11\. Check the film with Film.IsValid.

12\. Add the Film to HashTable<int, Film> using the Id as the key.

13\. Count the line as loaded if it was added.

14\. Return the number of films loaded and the number skipped.



\## Error handling

If the file is missing:

A FileNotFoundException is thrown. The window shows:

File not found. Please check the file name and try again.



A line is skipped and counted as skipped if:

\- it does not have exactly 8 fields

\- the Id, Year, Duration or Age rating is not a number

\- the film fails validation (Film.IsValid)

\- the film Id already exists (the duplicate is not added)



An empty file (header only) returns 0 loaded and 0 skipped.



\## Result

LoadFilms returns (loaded, skipped).



\## Sample file

data/films\_sample.csv has a header, 10 good films and 2 bad lines (a bad year and extra commas).

Expected result: 10 loaded, 2 skipped.

