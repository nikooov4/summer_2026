CREATE VIEW Organization.InstructorsView AS
SELECT pe.PersonID,
       pe.PositionID,
       pe.OrgUnitID,
       pe.IsMain,
       pe.StartDate,
       pe.EndDate
FROM Person.PersonEmployments pe
UNION ALL
SELECT pei.PersonID,
       NULL AS PositionID,
       pei.OrgUnitID,
       NULL AS IsMain,
       pei.StartDate,
       pei.EndDate
FROM Person.PersonEmploymentsInformal pei;