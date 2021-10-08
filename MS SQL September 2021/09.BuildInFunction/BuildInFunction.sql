SELECT FirstName, LastName
FROM Employees
WHERE FirstName LIKE 'Sa%'

SELECT FirstName, LastName
FROM Employees
WHERE LastName LIKE '%ei%'

SELECT FirstName
FROM Employees
WHERE DepartmentID = 3 OR DepartmentID = 10 AND HireDate >= '1995-01-01' AND HireDate <= '2005-01-01'

SELECT FirstName, LastName
FROM Employees
WHERE JobTitle NOT LIKE '%engineer%'

SELECT [Name]
FROM Towns
WHERE LEN([Name]) = 5 OR LEN([Name]) = 6
ORDER BY [Name]

SELECT TownID, [Name]
FROM Towns
WHERE [Name] LIKE 'M%' OR [Name] LIKE 'K%' OR [Name] LIKE 'B%' OR [Name] LIKE 'E%'
ORDER BY [Name]

SELECT TownID, [Name]
FROM Towns
WHERE [Name] NOT LIKE 'R%' AND [Name] NOT LIKE 'B%' AND [Name] NOT LIKE 'D%'
ORDER BY [Name]

GO

CREATE VIEW V_EmployeesHiredAfter2000 AS
SELECT FirstName, LastName
FROM Employees
WHERE HireDate > '2001-01-01';

SELECT FirstName, LastName
FROM Employees
WHERE LEN(LastName) = 5

SELECT EmployeeID, FirstName, LastName, Salary, DENSE_RANK() OVER(PARTITION BY Salary ORDER BY EmployeeID) AS [Rank]
FROM Employees
WHERE Salary BETWEEN 10000 AND 50000
ORDER BY Salary DESC

SELECT CountryName, IsoCode
FROM Countries
WHERE CountryName LIKE '%a%a%a%'
ORDER BY IsoCode

SELECT Peaks.PeakName, Rivers.RiverName, 
LOWER((Peaks.PeakName) + SUBSTRING(Rivers.RiverName,2,LEN(Rivers.Rivername))) AS 'Mix' 
FROM Peaks
JOIN Rivers
ON RIGHT(Peaks.PeakName,1) = LEFT(Rivers.RiverName,1)
ORDER BY Mix

SELECT TOP(50) [Name], FORMAT([Start], 'yyyy-MM-dd') AS [Start]
FROM Games
WHERE [Start] BETWEEN '2011-01-01' AND '2012-12-30'
ORDER BY [Start], [Name]

SELECT * FROM Users

SELECT Username, RIGHT(Email, CHARINDEX('@', REVERSE(Email)) - 1) AS 'Email Provider'
FROM Users 
ORDER BY [Email Provider], Username

SELECT Username, IpAddress
FROM Users
WHERE IpAddress LIKE '___.1%.%.___'
ORDER BY Username

SELECT * FROM Games

SELECT [Name], CASE
				WHEN DATEPART(HOUR, [Start]) >= 0 AND DATEPART(HOUR, [Start]) < 12 THEN 'Morning'
				WHEN DATEPART(HOUR, [Start]) >= 12 AND DATEPART(HOUR, [Start]) < 18 THEN 'Afternoon'
				WHEN DATEPART(HOUR, [Start]) >= 18 AND DATEPART(HOUR, [Start]) < 24 THEN 'Evening' 
				END AS [Part of the Day],

				CASE
				WHEN Duration <= 3 THEN 'Extra Short'
				WHEN Duration >= 4 AND Duration <= 6 THEN 'Short'
				WHEN Duration > 6 THEN 'Long'
				WHEN Duration IS NULL THEN 'Extra Long'
				END AS [Duratiion]
FROM Games
ORDER BY [Name], Duratiion

SELECT ProductName, OrderDate, DATEADD(DAY, 3, OrderDate) AS [Pay Due], DATEADD(MONTH, 1, OrderDate) AS [Deliver Due]
FROM Orders 