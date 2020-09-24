# CSV Consolidator
Contains a command line program that compiles values from multiple CSV files into a new CSV file.
## Specification
The specification for this project comes from a career readiness exercise from a course I took at Columbia College Chicago.
The exact problem specification goes as such; Given a series of csv files, write a command line program that will combine the spreadsheet values and return the sum and average for each header type. The first row of each CSV contains the headers (which may vary from file to file) that categorize expenses listed in the following rows. Identical headers should be combined and a new CSV file should be created containing all headers with their sums and average values in the second and third rows. Input values can be listed in integer, decimal, or accounting formats and the program must be able to interpret these. It can be assumed that a common currency symbol is used.
For this project we were only allowed to use the .NET framework.
## Contents
This repo shows off:
- Processing and error checking command line arguments
- File IO and CSV processing/creation
- Value parsing using regular expressions
- Unit testing with mock input/output
