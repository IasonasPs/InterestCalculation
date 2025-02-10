
# InterestCalculation

A C# project in **.NET 8.0**  designed to calculate various types of interest.

## Features
- Provides methods for calculating both **Legal** and **delay** interest.
- Can be easily extended for other financial calculations.

## Conclusions
   - This project was initially designed with **memory efficiency** and **speed** in mind, which is why **tuples** were used extensively.
   - While tuples as value types, do offer memory efficiency and reduced overhead , the use of custom classes -or even better, records-
   would enhance readability and maintanability in the long run.

## Dependencies

### This project utilizes the following NuGet packages:

1. **HtmlAgilityPack** (Version 1.11.72)  
   - A powerful library for parsing and manipulating HTML documents.  
   - Used for tasks such as web scraping, HTML document modification, and data extraction.  

2. **Spectre.Console** (Version 0.49.1)  
   - A modern .NET library for creating beautiful console applications.  
   - Provides rich text formatting, tables, progress bars, prompts, and more.  


## Installation

1. Clone the repository:
   ```
   git clone https://github.com/IasonasPs/InterestCalculation.git
   ```
2. Open the solution in Visual Studio or your preferred C# IDE.

## License
This project is licensed under the **Unlicense**.

 
