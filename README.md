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
