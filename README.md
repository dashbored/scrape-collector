# scrape-collector

### .NET Command line tool to scrape a website and saving it with the same file structure.
A simple application that navigates links and downloads each href/src link. Use at your own risk!

Application consists of a library project and a console project for running.

## Requirements
* .NET 8

## How-To-Use
* Build the project, navigate to output folder and run with a url and root folder:
  
  <code>scrape-collector.TUI -u [URL] -r [Root folder]</code>
  
  <code>scrape-collector.TUI -u http://example.com -r "C:/temp/"</code>
