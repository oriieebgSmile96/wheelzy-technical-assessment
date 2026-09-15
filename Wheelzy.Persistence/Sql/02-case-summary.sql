-- Question 1: car info, current buyer + quote, current status + status date.
-- Only the columns requested. Current quote is not assumed to be the highest.

SELECT
    c.Year,
    mk.Name AS Make,
    md.Name AS Model,
    sm.Name AS Submodel,
    b.Name AS CurrentBuyerName,
    q.Amount AS CurrentQuoteAmount,
    st.Name AS CurrentStatusName,
    h.StatusDate AS CurrentStatusDate
FROM dbo.SaleCase AS c
INNER JOIN dbo.CarSubmodel AS sm ON sm.SubmodelId = c.SubmodelId
INNER JOIN dbo.CarModel AS md ON md.ModelId = sm.ModelId
INNER JOIN dbo.CarMake AS mk ON mk.MakeId = md.MakeId
LEFT JOIN dbo.CaseQuote AS q
    ON q.CaseId = c.CaseId
   AND q.IsCurrent = 1
LEFT JOIN dbo.Buyer AS b ON b.BuyerId = q.BuyerId
LEFT JOIN dbo.CaseStatusHistory AS h
    ON h.CaseId = c.CaseId
   AND h.IsCurrent = 1
LEFT JOIN dbo.CaseStatusType AS st ON st.StatusTypeId = h.StatusTypeId;
