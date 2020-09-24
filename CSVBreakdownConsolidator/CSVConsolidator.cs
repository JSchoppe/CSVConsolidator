using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSVBreakdownConsolidator
{
    public static class CSVConsolidator
    {
        // Define program parameters.
        public static readonly string HELP_MESSAGE = @"
The breakdown consolidator utility must be ran with at least one .csv (comma seperated values) file.
Foreach CSV file include the file path as an input. For example:
accounts/assets.csv accounts/expenses.csv reciepts.csv";
        public static readonly string OUTPUT_CSV_NAME = "consolidated.csv";

        /// <summary>
        /// Attempts to consolidate the given CSV files and push the result to an output file.
        /// </summary>
        /// <param name="paths">The paths of the files to operate on.</param>
        public static void Execute(string[] paths)
        {
            // Extract the rows from each CSV file.
            string[][] CSVs = new string[paths.Length][];
            for (int i = 0; i < paths.Length; i++)
                CSVs[i] = File.ReadAllLines(paths[i]);
            // Consolidate the extracted data.
            string output = Consolidate(CSVs);
            // Output the data to a new file.
            File.WriteAllText(OUTPUT_CSV_NAME, output);
        }

        /// <summary>
        /// Takes a series of CSV rows and combines similar headers.
        /// Outputs file content for a new CSV with headers, sums, and averages.
        /// </summary>
        /// <param name="CSVs">The CSV content indexed by file, row.</param>
        /// <returns>The consolidated CSV content.</returns>
        public static string Consolidate(string[][] CSVs)
        {
            // Store the accumulated values based on their header.
            Dictionary<string, double> labelSums =
                new Dictionary<string, double>();
            // Store the total count for each header.
            Dictionary<string, int> labelEntryCounts =
                new Dictionary<string, int>();

            foreach (string[] rows in CSVs)
            {
                List<string> header = SeperateValues(rows[0]);
                // Scan the header to make sure it contains labels, not numbers.
                foreach (string label in header)
                {
                    if (TryParseValue(label, out double parsedValue))
                        throw new Exception
                            ($"Header contains numerical value instead of label in file: {rows}");
                    // Initialize any new category labels.
                    if (!labelSums.ContainsKey(label))
                    {
                        labelSums.Add(label, 0);
                        labelEntryCounts.Add(label, 0);
                    }
                }

                // Process remaining rows and add them to
                // their respective label accumulators.
                for (int i = 1; i < rows.Length; i++)
                {
                    List<string> values = SeperateValues(rows[i]);
                    // Report dangling values.
                    if (values.Count > header.Count)
                        throw new Exception
                            ($"A value is under a column with no header in file: {rows}");

                    // Parse each value.
                    for (int j = 0; j < values.Count; j++)
                    {
                        if (TryParseValue(values[j], out double parsedValue))
                        {
                            // Ignore values of 0 and empty fields.
                            if (parsedValue != 0)
                            {
                                labelSums[header[j]] += parsedValue;
                                labelEntryCounts[header[j]]++;
                            }
                        }
                        else
                            throw new Exception
                                ($"Non-value `{values[j]}` found outside of header");
                    }
                }
            }

            // Construct the output CSV rows.
            string headers = string.Empty;
            string sums = string.Empty;
            string averages = string.Empty;
            foreach (KeyValuePair<string, double> labelSum in labelSums)
            {
                headers += $"{labelSum.Key},";
                sums += $"\"{labelSum.Value:C}\",";
                // Calculate the average value for this label.
                // Check to avoid possible division by zero.
                if (labelEntryCounts[labelSum.Key] == 0)
                    averages += $"\"{0:C}\",";
                else
                    averages += $"\"{(labelSum.Value / labelEntryCounts[labelSum.Key]):C}\",";
            }
            // Strip the final comma in each row.
            headers = headers.Remove(headers.Length - 1);
            sums = sums.Remove(sums.Length - 1);
            averages = averages.Remove(averages.Length - 1);
            // Add labels to a new leftmost column.
            headers = "," + headers;
            sums = "total," + sums;
            averages = "average," + averages;
            // Return the file content.
            return headers + Environment.NewLine
                + sums + Environment.NewLine + averages + Environment.NewLine;
        }

        // This method serves as the CSV processor/interpreter.
        public static List<string> SeperateValues(string line)
        {
            List<string> accumulatedValues = new List<string>();

            // Setup logic for processing the CSV escape sequences.
            string currentValue = string.Empty;
            bool quoteEscapeIsActive = false;
            // Parse character by character.
            for(int i = 0; i < line.Length; i++)
            {
                switch (line[i])
                {
                    // If the escape is active commas are interpreted literally.
                    // Otherwise they signify the end of the current value.
                    case ',':
                        if (quoteEscapeIsActive)
                            currentValue += ',';
                        else
                        {
                            accumulatedValues.Add(currentValue);
                            currentValue = string.Empty;
                        }
                        break;

                    // Quotes can be either delimiter escapes
                    // or literal quote when "" is inside a quote escape.
                    case '"':
                        if (quoteEscapeIsActive)
                        {
                            // Look forward to see if the next
                            // character isn't another quote.
                            if (i == line.Length - 1 || line[i + 1] != '"')
                                quoteEscapeIsActive = false;
                            else
                            {
                                // If it is another quote than this quote
                                // is a quote literal and the next quote
                                // will be skipped.
                                currentValue += '"';
                                i++;
                            }
                        }
                        else
                            quoteEscapeIsActive = true;
                        break;

                    // Other characters are added directly to the value.
                    default:
                        currentValue += line[i];
                        break;
                }
            }
            // Add the final value (since it isn't followed by a comma delimiter).
            accumulatedValues.Add(currentValue);

            return accumulatedValues;
        }

        // Define numeric input processing.
        private static readonly Regex valueExpression =
            new Regex(@"^""?\(?-?(\$)?((\d)*|(\d){1,3}(,(\d){3})+)(\.(\d)+)?(\$)?\s*\)?""?$",
                RegexOptions.ECMAScript);
        /*
            Regex explanation:
            ^                 - Start Line
            "?                - Optional: Quote (delimiter breaker for values with ,)
            \(?               - Optional: ( denotes negative value
            -?                - Optional: Negative value
            (\$)?             - Optional: Currency symbol prefix
            (                          
                (\d)*         - A sequence of digits
                |             - OR
                (\d){1,3}     - One, two, or three digits
                (,(\d){3})+   - and one or more comma digit digit digit sequences
            )                          
            (\.(\d)+)?        - Optional: Decimal followed by a digit sequence
            (\$)?             - Optional: Currency symbol suffix
            \s*               - Tolerate trailing whitespace
            \)?               - Optional: ) ends denotation of negative value
            "?                - Optional: Quote (delimiter breaker for values with ,)
            $                 - End Line
        */
        public static bool TryParseValue(string input, out double value)
        {
            value = 0;

            // If input field is empty, do not run regex.
            // Just return a value of zero since the cell is empty.
            if (input.Length == 0)
                return true;

            if (valueExpression.IsMatch(input))
            {
                // Determine the sign of this value.
                double negation = 1;
                if (input.Contains("(") && input.Contains(")"))
                    negation *= -1;
                if (input.Contains("-"))
                    negation *= -1;

                // Remove format specific characters.
                input = Remove(input, "$", "\"", ",", "-", " ", "(", ")");

                // Try converting the remaining string to double.
                if (double.TryParse(input, out double parsedValue))
                {
                    value = parsedValue * negation;
                    return true;
                }
            }
            return false;
        }

        private static string Remove(string input, params string[] toRemove)
        {
            foreach (string subString in toRemove)
                input = input.Replace(subString, string.Empty);
            return input;
        }
    }
}
