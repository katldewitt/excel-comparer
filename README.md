# excel-comparer
A set of methods to compare two excels (.xlsx) that expand on existing excel comparisons.

[![.NET](https://github.com/katldewitt/excel-comparer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/katldewitt/excel-comparer/actions/workflows/dotnet.yml)

## Problem

At the highest level, this project tries to solve problems created by lacking version control when making changes to excel books. Excel spreadsheets are not databases, but many organizations rely on spreadsheets to track, update, and excecute on operations. By creating the ability to quickly understand how spreadsheets differ based on a unique key, this project assists end users to reconcile and make decisions.

### A concrete example

When running a camp, the head counselor creates a spreadsheet to track data on campers. The spreadsheet is sent to all counselors, yourself included, who make local copies.

| Person Name | TShirt Size | RSVP | Additional Guests? |
|-------|-----|------|---------|
| Henry | M | True | 1 |
| Henrietta | XS | True | 21 |
| Humphrey | XL | False | 7 |

The week before camp starts, you make the following changes:

1. Henry's mom calls to report Henry should have a Large Tshirt instead
2. Henrietta's parents decide to disenroll her from your camp
3. You add a column adding allergy status
4. Helga, your cousin, enrolls at the camp

| Person Name | TShirt Size | RSVP | Additional Guests? | New: Allergies? |
|-------|-----|------|------|---|
| Henry | **Change: L** | True | 1 | New: Yes|
| ~~Deleted: Henrietta~~ | ~~Deleted: XS~~ | ~~Deleted: True~~ | ~~Deleted: 21~~ |
| Humphrey | XL | False | 7 | New: No |
| **New: Helga** | **New: M** | **New: True** | **New: 0** | New: No |

When you send the spreadsheet back to the head counselor, the head counselor scratches his head in consternation because he had added a column including cabin assignments.

| Person Name | TShirt Size | RSVP | Additional Guests? | New: Cabin |
|-------|-----|------|-----|----|
| Henry | M | True | 1 | **New:Rosewood** |
| Henrietta | XS | True | 21 | **New:Maple** |
| Humphrey | XL | False | 7 | **New:Pine** |

In the simple example, we can easily figure out how to reconcile the two spreadsheets via scanning and doing a spreadsheet compare to assist. However, if you add a few hundred additional rows and change the ordering of the columns, the process becomes increasingly more challenging.

//TODO: verify output of this program for the above use case.

### Additional Example Use cases

1. Migrating manually added comments in excel reports produced output
2. Reconciling version control issues in participant tracking spreadsheets
3. Verifying a spreadsheet's data has not changed after making formatting changes

### Assumptions

//TODO: Flesh out (1. unique key assumption, 2. exact match in string comparisons)

## System Design

### Read In
### Conduct Comparison
### Write Output

### An aside on SQL Joins as a Method to Understand Output Options

Most programmers are familiar with SQL (structured query language), so it is helpful to recognize how this program's output is similar to SQL Joins. I have added a visual below of SQL joins that will be helpful to map onto the existing report options.

![name](https://www.devtodev.com/upload/images/sql5_2.png)

There are 6 reports that are available when running the comparisons. Those reports are as follows:

| Report Name | SQL Equivalent | Example Use Case |
|----|----|----|
| In Both | Inner Join | Say you're tracking participants. This view will allow you to reconcile participant's data who existed in both excels. |
| Only in Source | Left Exclusive | Say you're tracking participants. This view will allow you to identify participants who have disenrolled. |
| Only in Comparison | Right Exclusive | Say you're tracking participants. This view will allow you to identify participants who have enrolled. |
| All | Full Outer Join | Say you're creating a report table that has comments from a reviewer [orig] and updates from an analyst [comp]. This can help you to migrate the comments from the reviewer and the work of the analyst. |
| In Source | Left Inclusive | Say you're creating a report table that has comments from a reviewer and updates from an analyst. This can help you to migrate the comments from the reviewer. |
| In Comparison | Right Inclusive | Say you're creating a report table that has comments from a reviewer and updates from an analyst. This can help you incorporate the work of the analyst. |

You may notice that there is no report for the Full Outer Exclusive Join. I could not think of a good use case for this view. If you have a use case for wanting to see the rows that were only in comp and only in original in the same sheet, please feel free to file an issue.

The other parameter available when designing reports is `prioritizeSource`, a bool that when TRUE has the Original's values placed in the excel and the Comparison's values placed in the comment.

| prioritizeSource | Example Use Case |
|----|----|
| True | Say you're running an automated report that generates data you need to audit. This view will allow you to identify patterns or mistakes in data entry that can be used for training since it focuses on the original data. |
| False | Say you're running an automated report that generates data you need to audit. This view will help you see the changes that have been made between the two excels since you care more about the final state of the excel than the original. |
